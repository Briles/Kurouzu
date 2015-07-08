using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
//
using Blazinix.INI;
using SpawnedIn.GGA.Defaults;

namespace SpawnedIn.GGA.Helpers
{
    public static class Helper
    {
        public static void BatchFileRename(string game)
        {
            var ass = Assembly.GetExecutingAssembly();
            string[] reses = ass.GetManifestResourceNames();
            string[] csvs = reses.Where(r => r.EndsWith(".csv") && r.StartsWith(game)).ToArray();
            foreach (string csv in csvs)
            {
                List<string> unused = new List<string>();
                string directory = csv.Replace(game, "").Replace(".csv", "").Trim();
                List<string> assets = new List<string>(Directory.GetFiles(Path.Combine(Globals.Paths.Assets, game, directory), "*", SearchOption.AllDirectories));
                using(var s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(csv))
                using(var re = new StreamReader(s))
                {
                    string l;
                    while ((l = re.ReadLine()) != null)
                    {
                        string[] p = l.Split(',');
                        string nOld= p[0];
                        string nNew = p[1];
                        string e = nOld;
                        Regex r = new Regex(string.Format("^{0}$",e.Replace("*",".*")), RegexOptions.IgnoreCase);
                        string[] matches = assets.Where(f => r.IsMatch(Path.GetFileNameWithoutExtension(f))).ToArray();
                        if (matches.Length == 0)
                        {
                            unused.Add(nOld);
                            continue;
                        }
                        Parallel.ForEach(matches, asset =>
                        {
                            string filename = Path.GetFileName(asset);
                            string file_ext = Path.GetExtension(asset);
                            string file_directory = Path.GetDirectoryName(asset);
                            string dest = Path.Combine(file_directory, (nNew + file_ext));
                            if (!String.Equals(asset, dest, StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine("Renaming {0} to {1}", filename, (nNew + file_ext));
                                File.Move(asset, dest);
                                File.Delete(asset);
                            }
                            assets.Remove(asset);
                        });
                    }
                }
                // Don't log anything if logging is disabled
                if (!Globals.Paths.Args.Contains("-nolog", StringComparer.OrdinalIgnoreCase))
                {
                    // Any leftover assets are those that are not in the csvs
                    if (assets.Count > 0)
                    {
                        assets.Sort();
                        string unmatched_log = (Globals.Paths.Logs + @"\log_unmatched_" + (game.Replace(" ", "-")) + "_" + directory + ".txt").ToLower();
                        string unmatched = "-----This log lists any files that were not renamed. These names should be added to the csv file-----\r\n" + String.Join("\r\n", assets.Select(f => Path.GetFileNameWithoutExtension(f)).ToArray());
                        File.AppendAllText(unmatched_log, unmatched, System.Text.Encoding.UTF8);
                    }
                    // Any old names in the csv no longer being used should be removed
                    if (unused.Count > 0)
                    {
                        unused.Sort();
                        string unused_log = (Globals.Paths.Logs + @"\log_unused_" + (game.Replace(" ", "-")) + "_" + directory + ".txt").ToLower();
                        string unused_str = "-----This log lists all old names that were not matched against any file names. For performance these old names should be removed from the csv file-----\r\n" + String.Join("\r\n", unused.ToArray());
                        File.AppendAllText(unused_log, unused_str, System.Text.Encoding.UTF8);
                    }
                }
            }
        }

        public static void MinifyPNG()
        {
            int num = 0;
            string[] pngs = Directory.GetFiles(Globals.Paths.Assets, "*.png", SearchOption.AllDirectories);
            int count = pngs.Length;
            Parallel.ForEach(pngs, png =>
            {
                // Set up the processes
                var pngout = new Process
                {
                    StartInfo = new ProcessStartInfo {
                        FileName = "pngout.exe",
                        Arguments = String.Format(" /q {0}", @png),
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
                        Arguments = String.Format(" -o4 /quiet {0}", @png),
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
                        Arguments = String.Format(" /s {0}", @png),
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardOutput = false,
                        CreateNoWindow = true
                    }
                };
                num++;
                Console.WriteLine("Minifying {0}/{1}, {2}", num, count, Path.GetFileName(png));
                pngout.Start();
                truepng.Start();
                deflopt.Start();
            });
        }

        public static void PostCleanup(string game)
        {
            //Delete all source folders
            if (Directory.Exists(Globals.Paths.Assets))
            {
                List<string> sources = new List<string>(Directory.GetDirectories(Path.Combine(Globals.Paths.Assets, game), "Source", SearchOption.AllDirectories));
                sources.AddRange(Directory.GetDirectories(Path.Combine(Globals.Paths.Assets, "Source"), game, SearchOption.AllDirectories));
                Parallel.ForEach(sources, source =>
                {
                    if (Directory.Exists(source))
                    {
                        try
                        {
                            Directory.Delete(source, true);
                            Console.WriteLine("Deleting {0}", source);
                        }
                        catch (IOException ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                });
            }
        }

        public static void BatchIMScale(List<ScalingJob> scalingjobs)
        {
            foreach (var job in scalingjobs)
            {
                string[] info = (job.Path).Split('\\');
                string game = info[0];
                string category = info[1];
                if (info.Length > 3)
                {
                    category += @"\" + info[2];
                }
                // Get the images to scale
                string[] images;
                string startpath = Path.Combine(Globals.Paths.Assets, job.Path, "Source");
                if (String.IsNullOrEmpty(job.ExcludePattern))
                {
                    images = Directory.GetFiles(startpath, job.SearchPattern, SearchOption.AllDirectories);
                }
                else
                {
                    string excludepattern = job.ExcludePattern;
                    Regex r = new Regex(string.Format("^{0}$",excludepattern.Replace("*",".*")), RegexOptions.IgnoreCase);
                    images = (Directory.GetFiles(startpath, job.SearchPattern, SearchOption.AllDirectories).Where(f => !r.IsMatch(Path.GetFileName(f)))).ToArray();
                }
                //Get the desired image sizes
                if (File.Exists(Globals.Paths.Conf))
                {
                    string[] sizes;
                    INIFile ini = new INIFile(Globals.Paths.Conf);
                    sizes = (ini.INIReadValue(game, category).Split(','));
                    Parallel.ForEach(sizes, size =>
                    {
                        //Create a destination directory
                        string dWidth = size.Split('x')[0];
                        Directory.CreateDirectory(Path.Combine(Globals.Paths.Assets, job.Path, dWidth));
                        Parallel.ForEach(images, image =>
                        {
                            // Get the extension so we only have to use IM identify when necessary
                            string ext = Path.GetExtension(image);
                            string dimensions = null;
                            if (ext == ".dds" || ext == ".tga")
                            {
                                var identify = new Process
                                {
                                    StartInfo = new ProcessStartInfo {
                                        FileName = "identify.exe",
                                        Arguments = String.Format(" -format %wx%h {0}", image),
                                        WindowStyle = ProcessWindowStyle.Hidden,
                                        UseShellExecute = false,
                                        RedirectStandardOutput = true,
                                        CreateNoWindow = true
                                    }
                                };
                                identify.Start();
                                dimensions = identify.StandardOutput.ReadLine();
                            }
                            else
                            {
                                Bitmap bitmap = new Bitmap(image);
                                dimensions = String.Format("{0}x{1}", bitmap.Width, bitmap.Height);
                                bitmap.Dispose();
                            }
                            string outputname = String.Format("{0}.png", Path.GetFileNameWithoutExtension(image));
                            string destpath = Path.Combine(Globals.Paths.Assets, job.Path, dWidth, outputname);
                            // Only convert when the sizes are different otherwise just copy
                            if (dimensions != size)
                            {
                                string settings = null;
                                // Take care of the numerous cases
                                switch (job.Path)
                                {
                                    // Dota 2 Items
                                    case @"Dota 2\Items\":
                                        settings += "-gravity west -crop ";
                                        switch (dimensions)
                                        {
                                            case "128x64":
                                                settings += "87x64+0+0";
                                                break;
                                            case "124x62":
                                                settings += "86x62+0+0";
                                                break;
                                            case "128x128":
                                                settings += "128x128+0+0";
                                                break;
                                            case "124x64":
                                                settings += "88x64+0+0";
                                                break;
                                        }
                                        settings += " +repage";
                                        break;
                                    // Smite Abilities
                                    case @"Smite\Abilities\":
                                        settings += "-alpha off";
                                        switch (dimensions)
                                        {
                                            // Ability Banners
                                            case "256x128":
                                                settings += "-gravity center -crop 128x128+0+0 +repage";
                                                break;
                                            default:
                                                break;
                                        }
                                        break;
                                    // Smite Gods
                                    case @"Smite\Gods\Portrait\":
                                        settings += "-alpha off -gravity west -crop 388x512+0+0 +repage";
                                        break;
                                    default:
                                        // StarCraft II Upgrades and Abilities
                                        if ((job.Path == @"StarCraft II\Upgrades\") || (job.Path == @"StarCraft II\Abilities\"))
                                        {
                                            settings += "-shave 7x7 +repage";
                                        }
                                        // Heroes of Newerth
                                        if (job.Path.StartsWith("Heroes of Newerth"))
                                        {
                                            settings += "-flip";
                                        }
                                        break;
                                }
                                //
                                Console.WriteLine("Scaling {0} from {1} to {2}", Path.GetFileName(image), dimensions, size);
                                var magick = new Process
                                {
                                    StartInfo = new ProcessStartInfo {
                                        FileName = "magick.exe",
                                        Arguments = String.Format(" \"{0}\" -colorspace RGB -size \"{1}\" +sigmoidal-contrast 11.6933 -define filter:filter=Sinc -define filter:window=Jinc -define filter:lobes=3 {2} -resize \"{3}\"! -sigmoidal-contrast 11.6933 -colorspace sRGB \"{4}\"", @image, dimensions, settings, size, @destpath),
                                        WindowStyle = ProcessWindowStyle.Hidden,
                                        UseShellExecute = false,
                                        RedirectStandardOutput = false,
                                        CreateNoWindow = true
                                    }
                                };
                                magick.Start();
                            }
                            else
                            {
                                File.Copy(image, destpath, true);
                                Console.WriteLine("Copying {0} to {1}", Path.GetFileName(image), size);
                            }
                        });
                    });
                }
            }
        }

        public static void PreCleanup(string game)
        {
            //Delete all game folders
            if (Directory.Exists(Globals.Paths.Assets))
            {
                string[] sources = Directory.GetDirectories(Globals.Paths.Assets, game, SearchOption.AllDirectories);
                Parallel.ForEach(sources, source =>
                {
                    if (Directory.Exists(source))
                    {
                        try
                        {
                            Directory.Delete(source, true);
                            Console.WriteLine("Deleting {0}", source);
                        }
                        catch (IOException)
                        {
                            Console.WriteLine("The directory could not be deleted because it is open!!!");
                        }
                    }
                });
            }
            // Delete previous logs
            if (Directory.Exists(Globals.Paths.Logs))
            {
                string[] sources = Directory.GetFiles(Globals.Paths.Logs, ("log_*_" + (game.Replace(' ','-').ToLower()) + "_*.txt"), SearchOption.AllDirectories);
                Parallel.ForEach(sources, source =>
                {
                    if (File.Exists(source))
                    {
                        File.Delete(source);
                        Console.WriteLine("Deleting {0}", source);
                    }
                });
            }
            else
            {
                Directory.CreateDirectory(Globals.Paths.Logs);
            }
        }

        public static void BuildDirectoryTree(string[] dirs)
        {
            Parallel.ForEach(dirs, dir =>
            {
                Directory.CreateDirectory(Path.Combine(Globals.Paths.Assets, dir, "Source"));
                Console.WriteLine("Creating {0}", dir);
            });
        }

        public static void BuildSourceDirectory(string dir_name)
        {
            Directory.CreateDirectory(Path.Combine(Globals.Paths.Assets, "Source", dir_name));
            Console.WriteLine("Creating {0}", dir_name);
        }

        public static void SWFExtract(string inpath, string outpath)
        {
            List<string> swf_info = new List<string>();
            var pswfinfo = new Process
            {
                StartInfo = new ProcessStartInfo {
                    FileName = "swfdump.exe",
                    Arguments = String.Format("\"{0}\" -u", inpath),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            pswfinfo.Start();
            while (!pswfinfo.StandardOutput.EndOfStream)
            {
                string line = pswfinfo.StandardOutput.ReadLine();
                swf_info.Add(line);
            }
            Regex idrgx = new Regex("^.*id ");
            Regex imrgx = new Regex(" image.*$");
            Regex iprgx = new Regex("ImagePack_.*_Embeds__e_");
            string[] swf_pngs = (swf_info.Where(info => info.Trim().Contains("DEFINEBITSLOSSLESS2")).Select(info => imrgx.Replace(idrgx.Replace(info.Trim(),""),""))).ToArray();
            string[] swf_pairs = (swf_info.Where(info => info.Trim().Contains("exports")).Select(info => (iprgx.Replace(info.Replace("exports ","").Replace(" as ",",").Replace("\"",""),"")).Trim())).ToArray();
            var pswfextractinit = new Process
            {
                StartInfo = new ProcessStartInfo {
                    FileName = "swfextract.exe",
                    Arguments = String.Format("\"{0}\" -u", inpath),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = true
                }
            };
            pswfextractinit.Start();
            Parallel.ForEach(swf_pairs, swf_pair =>
            {
                string[] pair = swf_pair.Split(',');
                string swf_id = pair[0];
                string swf_name = pair[1];
                if (swf_pngs.Contains(swf_id))
                {
                    string output_path = outpath + "\\" + swf_name + ".png";
                    var pswfextract = new Process
                    {
                        StartInfo = new ProcessStartInfo {
                            FileName = "swfextract.exe",
                            Arguments = String.Format(" -p \"{0}\" \"{1}\" -o \"{2}\"", swf_id, inpath, output_path),
                            WindowStyle = ProcessWindowStyle.Hidden,
                            UseShellExecute = false,
                            RedirectStandardOutput = false,
                            CreateNoWindow = true
                        }
                    };
                    pswfextract.Start();
                    Console.WriteLine("Extracting {0}", swf_name);
                }
            });
        }

        public static void BatchFileCopy(List<CopyJob> copyjobs)
        {
            Parallel.ForEach(copyjobs, job =>
            {
                SearchOption recursion = SearchOption.TopDirectoryOnly;
                if (job.Recursion == true)
                {
                    recursion = SearchOption.AllDirectories;
                }
                string[] founds;
                if (String.IsNullOrEmpty(job.ExcludePattern))
                {
                    founds = Directory.GetFiles(job.Path, job.SearchPattern, recursion);
                }
                else
                {
                    string excludepattern = job.ExcludePattern;
                    Regex r = new Regex(string.Format("^{0}$",excludepattern.Replace("*",".*")), RegexOptions.IgnoreCase);
                    founds = (Directory.GetFiles(job.Path, job.SearchPattern, recursion).Where(f => !r.IsMatch(Path.GetFileName(f)))).ToArray();
                }
                Parallel.ForEach(founds, found =>
                {
                    string filename = Path.GetFileName(found);
                    Console.WriteLine("Copying {0}", filename);
                    File.Copy(found, Path.Combine(Globals.Paths.Assets, job.OutputPath, "Source", filename), true);
                });
            });
        }

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

