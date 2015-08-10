using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Blazinix.INI;
using CommandLine;
using Kurouzu.Args;
using Kurouzu.Defaults;

namespace Kurouzu.Helpers
{
    public static class Helper
    {
        /// <summary>
        /// Renames files according to rename pairs found in CSV files in the 'data' directory
        /// </summary>
        /// <param name="game">The name of the game to process</param>
        public static void BatchFileRename(string game)
        {
            string[] csvFiles = Directory.GetFiles(Path.Combine(Globals.Paths.Data, game), "*.csv", SearchOption.AllDirectories).ToArray();
            foreach (string csvFile in csvFiles)
            {
                List<string> renamePairsNotUsed = new List<string>();
                string csvDirectory = Path.GetFileNameWithoutExtension(csvFile);
                List<string> assets = new List<string>(Directory.GetFiles(Path.Combine(Globals.Paths.Assets, game, csvDirectory), "*", SearchOption.AllDirectories));
                using(var renamePairs = new StreamReader(csvFile))
                {
                    string line;
                    while ((line = renamePairs.ReadLine()) != null)
                    {
                        string[] pair = line.Split(',');
                        string oldName= pair[0];
                        string newName = pair[1];
                        string tempOldName = oldName;
                        Regex r = new Regex($"^{tempOldName.Replace("*", ".*")}$", RegexOptions.IgnoreCase);
                        string[] matches = assets.Where(f => r.IsMatch(Path.GetFileNameWithoutExtension(f))).ToArray();
                        if (matches.Length == 0)
                        {
                            renamePairsNotUsed.Add(oldName);
                            continue;
                        }
                        foreach (string asset in matches)
                        {
                            string fileName = Path.GetFileName(asset);
                            string fileExtension = Path.GetExtension(asset);
                            string fileDirectoryName = Path.GetDirectoryName(asset);
                            string destination = Path.Combine(fileDirectoryName, (newName + fileExtension));
                            if (!string.Equals(asset, destination, StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine("Renaming {0} to {1}", fileName, (newName + fileExtension));
                                File.Move(asset, destination);
                                File.Delete(asset);
                            }
                            assets.Remove(asset);
                        }
                    }
                }
                // Don't log anything if logging is disabled.
                var options = new Options();
                if (Parser.Default.ParseArgumentsStrict(Globals.Paths.Arguments, options) && options.Logging)
                {
                    // Any leftover assets are those that are not in the csvs.
                    if (assets.Count > 0)
                    {
                        assets.Sort();
                        const string notMatchedLogMessage = "-----This log lists any files that were not renamed. These names should be added to the csv file-----\r\n";
                        string notMatchedLogPath = (Globals.Paths.Logs + @"\log_unmatched_" + (game.Replace(" ", "-")) + "_" + csvDirectory + ".txt").ToLower();
                        string notMatchedLogContent = notMatchedLogMessage + string.Join("\r\n", assets.Select(f => Path.GetFileNameWithoutExtension(f)).ToArray());
                        File.AppendAllText(notMatchedLogPath, notMatchedLogContent, Encoding.UTF8);
                    }
                    // Any old names in the csv no longer being used should be removed
                    if (renamePairsNotUsed.Count > 0)
                    {
                        renamePairsNotUsed.Sort();
                        const string notUsedLogMessage = "-----This log lists all old names that were not matched against any file names. For performance these old names should be removed from the csv file-----\r\n";
                        string notUsedLogPath = (Globals.Paths.Logs + @"\log_unused_" + (game.Replace(" ", "-")) + "_" + csvDirectory + ".txt").ToLower();
                        string notUsedLogContent = notUsedLogMessage + string.Join("\r\n", renamePairsNotUsed.ToArray());
                        File.AppendAllText(notUsedLogPath, notUsedLogContent, Encoding.UTF8);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a preconfigured INI file according to Kurouzu.Defaults
        /// </summary>
        public static void InitINI()
        {
            Console.Write("This is your first time running Kurouzu. Configuring.");
            const string iniSection = "Game Paths";
            INIFile ini = new INIFile(Globals.Paths.ConfigurationFile);
            Parallel.ForEach(Globals.Games, game =>
            {
                ini.INIWriteValue(iniSection, game.Title, game.Source);
                foreach (KeyValuePair<string, string> dimension in game.Dimensions)
                {
                    ini.INIWriteValue(game.Title, dimension.Key, dimension.Value);
                }
            });
            Console.WriteLine(".Done! :)");
        }

        /// <summary>
        /// Validates and stores the installation paths for games to be processed
        /// </summary>
        /// <param name="gameTitle">The name of the game to validate</param>
        public static void ValidateINI(string gameTitle)
        {
            const string iniSection = "Game Paths";
            GameInfo game = GameInfo.GetGamebyProp(gameTitle);
            INIFile ini = new INIFile(Globals.Paths.ConfigurationFile);
            if (!File.Exists(Globals.Paths.ConfigurationFile))
            {
                InitINI();
            }
            if (!Directory.Exists(ini.INIReadValue(iniSection, game.Title)))
            {
                Console.WriteLine("Finding {0}", game.Title);
                foreach (string drive in Globals.Paths.Drives)
                {
                    Parallel.ForEach(EnumerateFiles(drive, game.Binary, SearchOption.AllDirectories), matchedFile =>
                    {
                        Console.WriteLine("Found {0}", matchedFile);
                        string leafPath = matchedFile;
                        for (int i = 0; i < game.Leaf; ++i)
                        {
                            leafPath = Directory.GetParent(leafPath).ToString();
                        }
                        ini.INIWriteValue(iniSection, game.Title, leafPath + @"\");
                        Console.WriteLine("Storing {0}", leafPath + @"\");
                    });
                }
            }
        }

        /// <summary>
        /// Minifies all PNG images in the 'Assets' directory
        /// </summary>
        public static void MinifyPNG()
        {
            int counter = 0;
            string[] imageFiles = Directory.GetFiles(Globals.Paths.Assets, "*.png", SearchOption.AllDirectories);
            int totalImages = imageFiles.Length;
            foreach (string png in imageFiles)
            {
                // Set up the processes
                var pngout = new Process
                {
                    StartInfo = new ProcessStartInfo {
                        FileName = "pngout.exe",
                        Arguments = $" /q {@png}",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardOutput = false,
                        CreateNoWindow = true
                    }
                };
                var truepng = new Process
                {
                    StartInfo = new ProcessStartInfo {
                        FileName = "truepng.exe",
                        Arguments = $" -o4 /quiet {@png}",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardOutput = false,
                        CreateNoWindow = true
                    }
                };
                var deflopt = new Process
                {
                    StartInfo = new ProcessStartInfo {
                        FileName = "deflopt.exe",
                        Arguments = $" /s {@png}",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardOutput = false,
                        CreateNoWindow = true
                    }
                };
                counter++;
                Console.WriteLine("Minifying {0}/{1}, {2}", counter, totalImages, Path.GetFileName(png));
                pngout.Start();
                truepng.Start();
                deflopt.Start();
            }
        }

        /// <summary>
        /// Cleans up any leftover, unused assets after processing
        /// </summary>
        /// <param name="game">The name of the game to to be cleaned up</param>
        public static void PostCleanup(string game)
        {
            //Delete all source folders
            if (Directory.Exists(Globals.Paths.Assets))
            {
                List<string> sourceDirectories = new List<string>(Directory.GetDirectories(Path.Combine(Globals.Paths.Assets, game), "Source", SearchOption.AllDirectories));
                sourceDirectories.AddRange(Directory.GetDirectories(Path.Combine(Globals.Paths.Assets, "Source"), game, SearchOption.AllDirectories));
                Parallel.ForEach(sourceDirectories, sourceDirectory =>
                {
                    if (Directory.Exists(sourceDirectory))
                    {
                        try
                        {
                            Directory.Delete(sourceDirectory, true);
                            Console.WriteLine("Deleting {0}", sourceDirectory);
                        }
                        catch (IOException ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Processes scaling jobs to scale image assets
        /// </summary>
        /// <param name="scalingJobs">The jobs to be processed</param>
        public static void BatchIMScale(List<ScalingJob> scalingJobs)
        {
            var options = new Options();
            if (Parser.Default.ParseArgumentsStrict(Globals.Paths.Arguments, options) && options.Scale)
            {
                foreach (var scalingJob in scalingJobs)
                {
                    string[] filePathInfo = (scalingJob.Path).Split('\\');
                    string game = filePathInfo[0];
                    string category = filePathInfo[1];
                    if (filePathInfo.Length > 3)
                    {
                        category += @"\" + filePathInfo[2];
                    }

                    // Get the images to scale
                    string[] inputImages;
                    string startPath = Path.Combine(Globals.Paths.Assets, scalingJob.Path, "Source");
                    if (string.IsNullOrEmpty(scalingJob.ExcludePattern))
                    {
                        inputImages = Directory.GetFiles(startPath, scalingJob.SearchPattern, SearchOption.AllDirectories);
                    }
                    else
                    {
                        string excludePattern = scalingJob.ExcludePattern;
                        Regex r = new Regex($"^{excludePattern.Replace("*", ".*")}$", RegexOptions.IgnoreCase);
                        inputImages = (Directory.GetFiles(startPath, scalingJob.SearchPattern, SearchOption.AllDirectories).Where(f => !r.IsMatch(Path.GetFileName(f)))).ToArray();
                    }

                    //Get the desired image sizes
                    if (File.Exists(Globals.Paths.ConfigurationFile))
                    {
                        INIFile ini = new INIFile(Globals.Paths.ConfigurationFile);
                        string[] outputDimensions = (ini.INIReadValue(game, category).Split(','));

                        foreach (string outputDimension in outputDimensions)
                        {
                            //Create a destination directory
                            string outputWidth = outputDimension.Split('x')[0];
                            Directory.CreateDirectory(Path.Combine(Globals.Paths.Assets, scalingJob.Path, outputWidth));
                            foreach (string inputImage in inputImages)
                            {
                                // Get the extension so we only have to use IM identify when necessary
                                string imageExtension = Path.GetExtension(inputImage);
                                string inputDimensions = null;
                                if (imageExtension == ".dds" || imageExtension == ".tga")
                                {
                                    var imageMagickIdentify = new Process
                                    {
                                        StartInfo = new ProcessStartInfo
                                        {
                                            FileName = "identify.exe",
                                            Arguments = $" -format %wx%h {inputImage}",
                                            WindowStyle = ProcessWindowStyle.Hidden,
                                            UseShellExecute = false,
                                            RedirectStandardOutput = true,
                                            CreateNoWindow = true
                                        }
                                    };
                                    imageMagickIdentify.Start();
                                    inputDimensions = imageMagickIdentify.StandardOutput.ReadLine();
                                }
                                else
                                {
                                    Bitmap bitmap = new Bitmap(inputImage);
                                    inputDimensions = $"{bitmap.Width}x{bitmap.Height}";
                                    bitmap.Dispose();
                                }
                                string outputName = $"{Path.GetFileNameWithoutExtension(inputImage)}.png";
                                string destinationPath = Path.Combine(Globals.Paths.Assets, scalingJob.Path, outputWidth, outputName);

                                // Only convert when the sizes are different otherwise just copy
                                if (inputDimensions != outputDimension)
                                {
                                    string imageMagickSettings = null;

                                    // Take care of the numerous cases
                                    switch (scalingJob.Path)
                                    {
                                        // Dota 2 Items
                                        case @"Dota 2\Items\":
                                            imageMagickSettings += "-gravity west -crop ";
                                            switch (inputDimensions)
                                            {
                                                case "128x64":
                                                    imageMagickSettings += "87x64+0+0";
                                                    break;
                                                case "124x62":
                                                    imageMagickSettings += "86x62+0+0";
                                                    break;
                                                case "128x128":
                                                    imageMagickSettings += "128x128+0+0";
                                                    break;
                                                case "124x64":
                                                    imageMagickSettings += "88x64+0+0";
                                                    break;
                                            }
                                            imageMagickSettings += " +repage";
                                            break;
                                        // Smite Abilities
                                        case @"Smite\Abilities\":
                                            imageMagickSettings += "-alpha off";
                                            switch (inputDimensions)
                                            {
                                                // Ability Banners
                                                case "256x128":
                                                    imageMagickSettings += "-gravity center -crop 128x128+0+0 +repage";
                                                    break;
                                                default:
                                                    break;
                                            }
                                            break;
                                        // Smite Gods
                                        case @"Smite\Gods\Portrait\":
                                            imageMagickSettings += "-alpha off -gravity west -crop 388x512+0+0 +repage";
                                            break;
                                        default:
                                            // StarCraft II Upgrades and Abilities
                                            if ((scalingJob.Path == @"StarCraft II\Upgrades\") || (scalingJob.Path == @"StarCraft II\Abilities\"))
                                            {
                                                imageMagickSettings += "-shave 7x7 +repage";
                                            }
                                            // Heroes of Newerth
                                            if (scalingJob.Path.StartsWith("Heroes of Newerth"))
                                            {
                                                imageMagickSettings += "-flip";
                                            }
                                            break;
                                    }
                                    //
                                    Console.WriteLine("Scaling {0} from {1} to {2}", Path.GetFileName(inputImage), inputDimensions, outputDimension);
                                    var imageMagickMagick = new Process
                                    {
                                        StartInfo = new ProcessStartInfo
                                        {
                                            FileName = "magick.exe",
                                            Arguments =
                                                $" \"{inputImage}\" -colorspace RGB -size \"{inputDimensions}\" +sigmoidal-contrast 11.6933 -define filter:filter=Sinc -define filter:window=Jinc -define filter:lobes=3 {imageMagickSettings} -resize \"{outputDimension}\"! -sigmoidal-contrast 11.6933 -colorspace sRGB \"{destinationPath}\"",
                                            WindowStyle = ProcessWindowStyle.Hidden,
                                            UseShellExecute = false,
                                            RedirectStandardOutput = false,
                                            CreateNoWindow = true
                                        }
                                    };
                                    imageMagickMagick.Start();
                                }
                                else
                                {
                                    File.Copy(inputImage, destinationPath, true);
                                    Console.WriteLine("Copying {0} to {1}", Path.GetFileName(inputImage), outputDimension);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Cleans up assets and logs from previous runs of Kurouzu
        /// </summary>
        /// <param name="game">The name of the game to to be cleaned up</param>
        public static void PreCleanup(string game)
        {
            // Delete all game folders
            //
            if (Directory.Exists(Globals.Paths.Assets))
            {
                string[] sourceDirectories = Directory.GetDirectories(Globals.Paths.Assets, game, SearchOption.AllDirectories);
                Parallel.ForEach(sourceDirectories, sourceDirectory =>
                {
                    if (Directory.Exists(sourceDirectory))
                    {
                        try
                        {
                            Directory.Delete(sourceDirectory, true);
                            Console.WriteLine("Deleting {0}", sourceDirectory);
                        }
                        catch (IOException)
                        {
                            Console.WriteLine("The directory could not be deleted because it is open.");
                        }
                    }
                });
            }

            // Delete previous logs.
            //
            if (Directory.Exists(Globals.Paths.Logs))
            {
                string[] sourceLogs = Directory.GetFiles(Globals.Paths.Logs, ("log_*_" + (game.Replace(' ','-').ToLower()) + "_*.txt"), SearchOption.AllDirectories);
                Parallel.ForEach(sourceLogs, logfile =>
                {
                    if (File.Exists(logfile))
                    {
                        File.Delete(logfile);
                        Console.WriteLine("Deleting {0}", logfile);
                    }
                });
            }
            else
            {
                Directory.CreateDirectory(Globals.Paths.Logs);
            }
        }

        /// <summary>
        /// Builds the Assets directory structure
        /// </summary>
        /// <param name="directoryNames">A list of directory names to create in the Assets directory</param>
        public static void BuildDirectoryTree(string[] directoryNames)
        {
            Parallel.ForEach(directoryNames, directoryName =>
            {
                Directory.CreateDirectory(Path.Combine(Globals.Paths.Assets, directoryName, "Source"));
                Console.WriteLine("Creating {0}", directoryName);
            });
        }

        /// <summary>
        /// Builds the Source directory structure
        /// </summary>
        /// <param name="directoryName">The partial path to append to the base path</param>
        public static void BuildSourceDirectory(string directoryName)
        {
            Directory.CreateDirectory(Path.Combine(Globals.Paths.Assets, "Source", directoryName));
            Console.WriteLine("Creating {0}", directoryName);
        }

        /// <summary>
        /// Processes Copy Jobs
        /// </summary>
        /// <param name="copyJobs">A list of CopyJob instances to be processed</param>
        public static void BatchFileCopy(List<CopyJob> copyJobs)
        {
            foreach (CopyJob job in copyJobs)
            {
                SearchOption recursionFlag = SearchOption.TopDirectoryOnly;
                if (job.Recursion)
                {
                    recursionFlag = SearchOption.AllDirectories;
                }
                string[] filesFound;
                if (string.IsNullOrEmpty(job.ExcludePattern))
                {
                    filesFound = Directory.GetFiles(job.Path, job.SearchPattern, recursionFlag);
                }
                else
                {
                    string excludePattern = job.ExcludePattern;
                    Regex r = new Regex($"^{excludePattern.Replace("*", ".*")}$", RegexOptions.IgnoreCase);
                    filesFound = (Directory.GetFiles(job.Path, job.SearchPattern, recursionFlag).Where(f => !r.IsMatch(Path.GetFileName(f)))).ToArray();
                }
                foreach (string foundFile in filesFound)
                {
                    string fileName = Path.GetFileName(foundFile);
                    string destPath = Path.Combine(Globals.Paths.Assets, job.OutputPath, "Source", fileName);

                    Console.WriteLine("Copying {0}", fileName);

                    File.Copy(foundFile, destPath, true);
                }
            }
        }

        //
        //
        public static IEnumerable<string> EnumerateDirectories(string parentDirectory, string searchPattern, SearchOption searchOpt)
        {
            try
            {
                var directories = Enumerable.Empty<string>();
                if (searchOpt == SearchOption.AllDirectories)
                {
                    directories = Directory.EnumerateDirectories(parentDirectory).SelectMany(x => EnumerateDirectories(x, searchPattern, searchOpt));
                }
                return directories.Concat(Directory.EnumerateDirectories(parentDirectory, searchPattern));
            }
            catch (UnauthorizedAccessException)
            {
                return Enumerable.Empty<string>();
            }
        }

        //
        //
        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOpt)
        {
            try
            {
                var dirFiles = Enumerable.Empty<string>();
                if (searchOpt == SearchOption.AllDirectories)
                {
                    dirFiles = Directory.EnumerateDirectories(path).SelectMany(x => EnumerateFiles(x, searchPattern, searchOpt));
                }
                return dirFiles.Concat(Directory.EnumerateFiles(path, searchPattern));
            }
            catch (UnauthorizedAccessException)
            {
                return Enumerable.Empty<string>();
            }
        }
    }

    /// <summary>
    /// A type used for batch copying files
    /// </summary>
    public class CopyJob
    {
        /// <summary>
        /// Creates a new Copy Job
        /// </summary>
        /// <param name="outputpath">The destination path where the files will be copied to</param>
        /// <param name="splats">The parameters to 'splat' against EnumerateFiles</param>
        public CopyJob(string outputpath, params object[] splats)
        {
            Path = (string)splats[0];
            Recursion = (bool)splats[1];
            SearchPattern = (string)splats[2];
            ExcludePattern = (string)splats[3];
            OutputPath = outputpath;
        }

        public string Path { get; set; }

        public bool Recursion { get; set; }

        public string SearchPattern { get; set; }

        public string ExcludePattern { get; set; }

        public string OutputPath { get; set; }
    }

    /// <summary>
    /// A type used for scaling image assets
    /// </summary>
    public class ScalingJob
    {
        /// <summary>
        /// Creates a new Scaling Job
        /// </summary>
        /// <param name="path">The base or starting path to find files from</param>
        /// <param name="searchpattern">Includes filenames matching this pattern</param>
        /// <param name="excludepattern">Excludes filenames matching this pattern</param>
        public ScalingJob(string path, string searchpattern = "*", string excludepattern = null)
        {
            Path = path;
            SearchPattern = searchpattern;
            ExcludePattern = excludepattern;
        }

        public string Path { get; set; }

        public string SearchPattern { get; set; }

        public string ExcludePattern { get; set; }
    }
}

