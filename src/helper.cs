using Blazinix.INI;
using FlashTools;
using Kurouzu.Args;
using Kurouzu.Defaults;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kurouzu.Helpers
{
    public static class Helper
    {
        //
        //
        public static void BatchFileRename(string game)
        {
            string[] CSVs = Directory.GetFiles(Path.Combine(Globals.Paths.Data, game), "*.csv", SearchOption.AllDirectories).ToArray();
            Parallel.ForEach(CSVs, CSV =>
            {
                List<string> RenamePairsNotUsed = new List<string>();
                string Directory = Path.GetFileNameWithoutExtension(CSV);
                List<string> Assets = new List<string>(System.IO.Directory.GetFiles(Path.Combine(Globals.Paths.Assets, game, Directory), "*", SearchOption.AllDirectories));
                using(var RenamePairs = new StreamReader(CSV))
                {
                    string Line;
                    while ((Line = RenamePairs.ReadLine()) != null)
                    {
                        string[] Pair = Line.Split(',');
                        string OldName= Pair[0];
                        string NewName = Pair[1];
                        string TempOldName = OldName;
                        Regex r = new Regex(string.Format("^{0}$", TempOldName.Replace("*",".*")), RegexOptions.IgnoreCase);
                        string[] Matches = Assets.Where(f => r.IsMatch(Path.GetFileNameWithoutExtension(f))).ToArray();
                        if (Matches.Length == 0)
                        {
                            RenamePairsNotUsed.Add(OldName);
                            continue;
                        }
                        foreach(string Asset in Matches)
                        {
                            string FileName = Path.GetFileName(Asset);
                            string FileExtension = Path.GetExtension(Asset);
                            string FileDirectoryName = Path.GetDirectoryName(Asset);
                            string Destination = Path.Combine(FileDirectoryName, (NewName + FileExtension));
                            if (!string.Equals(Asset, Destination, StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine("Renaming {0} to {1}", FileName, (NewName + FileExtension));
                                File.Move(Asset, Destination);
                                File.Delete(Asset);
                            }
                            Assets.Remove(Asset);
                        }
                    }
                }
                // Don't log anything if logging is disabled.
                var options = new Options();
                if (CommandLine.Parser.Default.ParseArgumentsStrict(Globals.Paths.Arguments, options) && options.Logging == true)
                {
                    // Any leftover assets are those that are not in the csvs.
                    if (Assets.Count > 0)
                    {
                        Assets.Sort();
                        const string NotMatchedLogMessage = "-----This log lists any files that were not renamed. These names should be added to the csv file-----\r\n";
                        string NotMatchedLogPath = (Globals.Paths.Logs + @"\log_unmatched_" + (game.Replace(" ", "-")) + "_" + Directory + ".txt").ToLower();
                        string NotMatchedLogContent = NotMatchedLogMessage + string.Join("\r\n", Assets.Select(f => Path.GetFileNameWithoutExtension(f)).ToArray());
                        File.AppendAllText(NotMatchedLogPath, NotMatchedLogContent, System.Text.Encoding.UTF8);
                    }
                    // Any old names in the csv no longer being used should be removed
                    if (RenamePairsNotUsed.Count > 0)
                    {
                        RenamePairsNotUsed.Sort();
                        const string NotUsedLogMessage = "-----This log lists all old names that were not matched against any file names. For performance these old names should be removed from the csv file-----\r\n";
                        string NotUsedLogPath = (Globals.Paths.Logs + @"\log_unused_" + (game.Replace(" ", "-")) + "_" + Directory + ".txt").ToLower();
                        string NotUsedLogContent = NotUsedLogMessage + string.Join("\r\n", RenamePairsNotUsed.ToArray());
                        File.AppendAllText(NotUsedLogPath, NotUsedLogContent, System.Text.Encoding.UTF8);
                    }
                }
            });
        }

        //
        //
        public static void InitINI()
        {
            Console.Write("This is your first time running Kurouzu. Configuring.");
            const string INISection = "Game Paths";
            INIFile INI = new INIFile(Globals.Paths.ConfigurationFile);
            Parallel.ForEach(Globals.Games, Game =>
            {
                INI.INIWriteValue(INISection, Game.Title, Game.Source);
                foreach (KeyValuePair<string, string> Dimension in Game.Dimensions)
                {
                    INI.INIWriteValue(Game.Title, Dimension.Key, Dimension.Value);
                }
            });
            Console.WriteLine(".Done! :)");
        }

        //
        //
        public static void ValidateINI(string gameTitle)
        {
            const string INISection = "Game Paths";
            GameInfo game = GameInfo.GetGamebyProp(gameTitle);
            INIFile INI = new INIFile(Globals.Paths.ConfigurationFile);
            if (!File.Exists(Globals.Paths.ConfigurationFile))
            {
                InitINI();
            }
            if (!Directory.Exists(INI.INIReadValue(INISection, game.Title)))
            {
                Console.WriteLine("Finding {0}", game.Title);
                foreach (string Drive in Globals.Paths.Drives)
                {
                    Parallel.ForEach(Helper.EnumerateFiles(Drive, game.Binary, SearchOption.AllDirectories), MatchedFile =>
                    {
                        Console.WriteLine("Found {0}", MatchedFile);
                        string LeafPath = MatchedFile;
                        for (int i = 0; i < game.Leaf; ++i)
                        {
                            LeafPath = Directory.GetParent(LeafPath).ToString();
                        }
                        INI.INIWriteValue(INISection, game.Title, LeafPath + @"\");
                        Console.WriteLine("Storing {0}", LeafPath + @"\");
                    });
                }
            }
        }

        //
        //
        public static void MinifyPNG()
        {
            int Counter = 0;
            string[] ImageFiles = Directory.GetFiles(Globals.Paths.Assets, "*.png", SearchOption.AllDirectories);
            int TotalImages = ImageFiles.Length;
            foreach(string png in ImageFiles)
            {
                // Set up the processes
                var pngout = new Process
                {
                    StartInfo = new ProcessStartInfo {
                        FileName = "pngout.exe",
                        Arguments = string.Format(" /q {0}", @png),
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
                        Arguments = string.Format(" -o4 /quiet {0}", @png),
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
                        Arguments = string.Format(" /s {0}", @png),
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardOutput = false,
                        CreateNoWindow = true
                    }
                };
                Counter++;
                Console.WriteLine("Minifying {0}/{1}, {2}", Counter, TotalImages, Path.GetFileName(png));
                pngout.Start();
                truepng.Start();
                deflopt.Start();
            }
        }

        //
        //
        public static void PostCleanup(string game)
        {
            //Delete all source folders
            if (Directory.Exists(Globals.Paths.Assets))
            {
                List<string> SourceDirectories = new List<string>(Directory.GetDirectories(Path.Combine(Globals.Paths.Assets, game), "Source", SearchOption.AllDirectories));
                SourceDirectories.AddRange(Directory.GetDirectories(Path.Combine(Globals.Paths.Assets, "Source"), game, SearchOption.AllDirectories));
                Parallel.ForEach(SourceDirectories, SourceDirectory =>
                {
                    if (Directory.Exists(SourceDirectory))
                    {
                        try
                        {
                            Directory.Delete(SourceDirectory, true);
                            Console.WriteLine("Deleting {0}", SourceDirectory);
                        }
                        catch (IOException ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                });
            }
        }

        //
        //
        public static void BatchIMScale(List<ScalingJob> scalingJobs)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArgumentsStrict(Globals.Paths.Arguments, options) && options.Scale == true)
            {
                foreach (var ScalingJob in scalingJobs)
                {
                    string[] FilePathInfo = (ScalingJob.Path).Split('\\');
                    string Game = FilePathInfo[0];
                    string Category = FilePathInfo[1];
                    if (FilePathInfo.Length > 3)
                    {
                        Category += @"\" + FilePathInfo[2];
                    }
                    // Get the images to scale
                    string[] InputImages;
                    string StartPath = Path.Combine(Globals.Paths.Assets, ScalingJob.Path, "Source");
                    if (string.IsNullOrEmpty(ScalingJob.ExcludePattern))
                    {
                        InputImages = Directory.GetFiles(StartPath, ScalingJob.SearchPattern, SearchOption.AllDirectories);
                    }
                    else
                    {
                        string ExcludePattern = ScalingJob.ExcludePattern;
                        Regex r = new Regex(string.Format("^{0}$", ExcludePattern.Replace("*", ".*")), RegexOptions.IgnoreCase);
                        InputImages = (Directory.GetFiles(StartPath, ScalingJob.SearchPattern, SearchOption.AllDirectories).Where(f => !r.IsMatch(Path.GetFileName(f)))).ToArray();
                    }
                    //Get the desired image sizes
                    if (File.Exists(Globals.Paths.ConfigurationFile))
                    {
                        string[] OutputDimensions;
                        INIFile INI = new INIFile(Globals.Paths.ConfigurationFile);
                        OutputDimensions = (INI.INIReadValue(Game, Category).Split(','));
                        foreach (string OutputDimension in OutputDimensions)
                        {
                            //Create a destination directory
                            string OutputWidth = OutputDimension.Split('x')[0];
                            Directory.CreateDirectory(Path.Combine(Globals.Paths.Assets, ScalingJob.Path, OutputWidth));
                            foreach (string InputImage in InputImages)
                            {
                                // Get the extension so we only have to use IM identify when necessary
                                string ImageExtension = Path.GetExtension(InputImage);
                                string InputDimensions = null;
                                if (ImageExtension == ".dds" || ImageExtension == ".tga")
                                {
                                    var ImageMagickIdentify = new Process
                                    {
                                        StartInfo = new ProcessStartInfo
                                        {
                                            FileName = "identify.exe",
                                            Arguments = string.Format(" -format %wx%h {0}", InputImage),
                                            WindowStyle = ProcessWindowStyle.Hidden,
                                            UseShellExecute = false,
                                            RedirectStandardOutput = true,
                                            CreateNoWindow = true
                                        }
                                    };
                                    ImageMagickIdentify.Start();
                                    InputDimensions = ImageMagickIdentify.StandardOutput.ReadLine();
                                }
                                else
                                {
                                    Bitmap Bitmap = new Bitmap(InputImage);
                                    InputDimensions = string.Format("{0}x{1}", Bitmap.Width, Bitmap.Height);
                                    Bitmap.Dispose();
                                }
                                string OutputName = string.Format("{0}.png", Path.GetFileNameWithoutExtension(InputImage));
                                string DestinationPath = Path.Combine(Globals.Paths.Assets, ScalingJob.Path, OutputWidth, OutputName);
                                // Only convert when the sizes are different otherwise just copy
                                if (InputDimensions != OutputDimension)
                                {
                                    string ImageMagickSettings = null;
                                    // Take care of the numerous cases
                                    switch (ScalingJob.Path)
                                    {
                                        // Dota 2 Items
                                        case @"Dota 2\Items\":
                                            ImageMagickSettings += "-gravity west -crop ";
                                            switch (InputDimensions)
                                            {
                                                case "128x64":
                                                    ImageMagickSettings += "87x64+0+0";
                                                    break;
                                                case "124x62":
                                                    ImageMagickSettings += "86x62+0+0";
                                                    break;
                                                case "128x128":
                                                    ImageMagickSettings += "128x128+0+0";
                                                    break;
                                                case "124x64":
                                                    ImageMagickSettings += "88x64+0+0";
                                                    break;
                                            }
                                            ImageMagickSettings += " +repage";
                                            break;
                                        // Smite Abilities
                                        case @"Smite\Abilities\":
                                            ImageMagickSettings += "-alpha off";
                                            switch (InputDimensions)
                                            {
                                                // Ability Banners
                                                case "256x128":
                                                    ImageMagickSettings += "-gravity center -crop 128x128+0+0 +repage";
                                                    break;
                                                default:
                                                    break;
                                            }
                                            break;
                                        // Smite Gods
                                        case @"Smite\Gods\Portrait\":
                                            ImageMagickSettings += "-alpha off -gravity west -crop 388x512+0+0 +repage";
                                            break;
                                        default:
                                            // StarCraft II Upgrades and Abilities
                                            if ((ScalingJob.Path == @"StarCraft II\Upgrades\") || (ScalingJob.Path == @"StarCraft II\Abilities\"))
                                            {
                                                ImageMagickSettings += "-shave 7x7 +repage";
                                            }
                                            // Heroes of Newerth
                                            if (ScalingJob.Path.StartsWith("Heroes of Newerth"))
                                            {
                                                ImageMagickSettings += "-flip";
                                            }
                                            break;
                                    }
                                    //
                                    Console.WriteLine("Scaling {0} from {1} to {2}", Path.GetFileName(InputImage), InputDimensions, OutputDimension);
                                    var ImageMagickMagick = new Process
                                    {
                                        StartInfo = new ProcessStartInfo
                                        {
                                            FileName = "magick.exe",
                                            Arguments = string.Format(" \"{0}\" -colorspace RGB -size \"{1}\" +sigmoidal-contrast 11.6933 -define filter:filter=Sinc -define filter:window=Jinc -define filter:lobes=3 {2} -resize \"{3}\"! -sigmoidal-contrast 11.6933 -colorspace sRGB \"{4}\"", InputImage, InputDimensions, ImageMagickSettings, OutputDimension, DestinationPath),
                                            WindowStyle = ProcessWindowStyle.Hidden,
                                            UseShellExecute = false,
                                            RedirectStandardOutput = false,
                                            CreateNoWindow = true
                                        }
                                    };
                                    ImageMagickMagick.Start();
                                }
                                else
                                {
                                    File.Copy(InputImage, DestinationPath, true);
                                    Console.WriteLine("Copying {0} to {1}", Path.GetFileName(InputImage), OutputDimension);
                                }
                            }
                        }
                    }
                }
            }
        }

        //
        //
        public static void PreCleanup(string game)
        {
            // Delete all game folders
            //
            if (Directory.Exists(Globals.Paths.Assets))
            {
                string[] SourceDirectories = Directory.GetDirectories(Globals.Paths.Assets, game, SearchOption.AllDirectories);
                Parallel.ForEach(SourceDirectories, SourceDirectory =>
                {
                    if (Directory.Exists(SourceDirectory))
                    {
                        try
                        {
                            Directory.Delete(SourceDirectory, true);
                            Console.WriteLine("Deleting {0}", SourceDirectory);
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
                string[] SourceLogs = Directory.GetFiles(Globals.Paths.Logs, ("log_*_" + (game.Replace(' ','-').ToLower()) + "_*.txt"), SearchOption.AllDirectories);
                Parallel.ForEach(SourceLogs, Logfile =>
                {
                    if (File.Exists(Logfile))
                    {
                        File.Delete(Logfile);
                        Console.WriteLine("Deleting {0}", Logfile);
                    }
                });
            }
            else
            {
                Directory.CreateDirectory(Globals.Paths.Logs);
            }
        }

        //
        //
        public static void BuildDirectoryTree(string[] directoryNames)
        {
            Parallel.ForEach(directoryNames, DirectoryName =>
            {
                Directory.CreateDirectory(Path.Combine(Globals.Paths.Assets, DirectoryName, "Source"));
                Console.WriteLine("Creating {0}", DirectoryName);
            });
        }

        //
        //
        public static void BuildSourceDirectory(string directoryName)
        {
            Directory.CreateDirectory(Path.Combine(Globals.Paths.Assets, "Source", directoryName));
            Console.WriteLine("Creating {0}", directoryName);
        }

        //
        //
        public static void SWFExtract(string swfFile, string outputPath)
        {
            SWFFile swf = new SWFFile(swfFile);
            foreach (DefineBitsLossless2 image in swf.PNGImages)
            {
                Console.WriteLine("Extracting {0}", image.SymbolName);

                string destinationPath = Path.Combine(outputPath, string.Format("{0}.png", image.SymbolName));

                byte[] BitMapPixelData = new byte[image.BitmapArea];
                BitMapPixelData = image.BitmapPixelData.ToArray();

                GCHandle pinnedArray = GCHandle.Alloc(BitMapPixelData, GCHandleType.Pinned);
                IntPtr pointer = pinnedArray.AddrOfPinnedObject();

                Bitmap newBitmap = new Bitmap(image.BitmapWidth, image.BitmapHeight, image.BitmapStride, PixelFormat.Format32bppPArgb, pointer);
                pinnedArray.Free();

                newBitmap.Save(destinationPath, ImageFormat.Png);
                newBitmap.Dispose();
            }
            swf.Close();
        }

        //
        //
        public static void BatchFileCopy(List<CopyJob> copyJobs)
        {
            Parallel.ForEach(copyJobs, CopyJob =>
            {
                SearchOption RecursionFlag = SearchOption.TopDirectoryOnly;
                if (CopyJob.Recursion == true)
                {
                    RecursionFlag = SearchOption.AllDirectories;
                }
                string[] FilesFound;
                if (string.IsNullOrEmpty(CopyJob.ExcludePattern))
                {
                    FilesFound = Directory.GetFiles(CopyJob.Path, CopyJob.SearchPattern, RecursionFlag);
                }
                else
                {
                    string ExcludePattern = CopyJob.ExcludePattern;
                    Regex r = new Regex(string.Format("^{0}$",ExcludePattern.Replace("*",".*")), RegexOptions.IgnoreCase);
                    FilesFound = (Directory.GetFiles(CopyJob.Path, CopyJob.SearchPattern, RecursionFlag).Where(f => !r.IsMatch(Path.GetFileName(f)))).ToArray();
                }
                foreach(string FoundFile in FilesFound)
                {
                    string FileName = Path.GetFileName(FoundFile);
                    Console.WriteLine("Copying {0}", FileName);
                    File.Copy(FoundFile, Path.Combine(Globals.Paths.Assets, CopyJob.OutputPath, "Source", FileName), true);
                }
            });
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

    //
    //
    public class CopyJob
    {
        // Jobs take the form { output path = string, { start path = string, recursion flag = true/false, search pattern = string, exclude pattern = string (to regex) } }
        private string path;
        private bool recursion;
        private string searchpattern;
        private string excludepattern;
        private string outputpath;

        public CopyJob(string outputpath, params object[] splats)
        {
            this.path = (string)splats[0];
            this.recursion = (bool)splats[1];
            this.searchpattern = (string)splats[2];
            this.excludepattern = (string)splats[3];
            this.outputpath = (string)outputpath;
        }

        public string Path
        {
            get { return path;  }
            set { path = value; }
        }
        public bool Recursion
        {
            get { return recursion;  }
            set { recursion = value; }
        }
        public string SearchPattern
        {
            get { return searchpattern;  }
            set { searchpattern = value; }
        }
        public string ExcludePattern
        {
            get { return excludepattern;  }
            set { excludepattern = value; }
        }
        public string OutputPath
        {
            get { return outputpath;  }
            set { outputpath = value; }
        }
    }

    //
    //
    public class ScalingJob
    {
        // Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
        private string path;
        private string searchpattern;
        private string excludepattern;

        public ScalingJob(string path, string searchpattern = "*", string excludepattern = null)
        {
            this.path = (string)path;
            this.searchpattern = (string)searchpattern;
            this.excludepattern = (string)excludepattern;
        }

        public string Path
        {
            get { return path;  }
            set { path = value; }
        }
        public string SearchPattern
        {
            get { return searchpattern;  }
            set { searchpattern = value; }
        }
        public string ExcludePattern
        {
            get { return excludepattern;  }
            set { excludepattern = value; }
        }
    }
}

