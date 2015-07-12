using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
//
using Blazinix.INI;
using SpawnedIn.GGA.Games;
using SpawnedIn.GGA.Helpers;
using SpawnedIn.GGA.Defaults;

namespace SpawnedIn.GGA.Defaults
{
    public static class Globals
    {
        public static class Paths
        {
            public static readonly string Work = (Directory.GetCurrentDirectory() + @"\");
            public static readonly string Assets = Path.Combine(Work, "Assets");
            public static readonly string Home = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\");
            public static readonly string Conf = Path.Combine(Home, "conf.ini");
            public static readonly string Data = Path.Combine(Home, "data");
            public static readonly string Logs = Path.Combine(Home, "logs");
            public static readonly string[] Drives = Environment.GetLogicalDrives();
            public static readonly string[] Args = Environment.GetCommandLineArgs();
        }

        public static class Lists
        {
            public static readonly string[] Games = {
                "Dawngate", 
                "Dota 2", 
                "Heroes of Newerth", 
                "Heroes of the Storm", 
                "League of Legends", 
                "Smite", 
                "StarCraft II", 
                "Strife"
            };
            public static readonly Dictionary<string, string> GameBinaryPaths = new Dictionary<string, string>
            {
                { "Dawngate", "Dawngate.exe" },
                { "Dota 2", "dota.exe" },
                { "Hearthstone", "Hearthstone.exe" },
                { "Heroes of Newerth", "hon.exe" },
                { "Heroes of the Storm", "Heroes of the Storm.exe" },
                { "League of Legends", "lol.launcher.exe" },
                { "Smite", "Smite.exe" },
                { "StarCraft II", "StarCraft II.exe" },
                { "Strife", "strife.exe" }
            };
            public static readonly Dictionary<string, string> GameSourcePaths = new Dictionary<string, string>
            {
                {"Dawngate", @"C:\Program Files (x86)\Electronic Arts\Dawngate\game\"},
                {"Dota 2", @"C:\Program Files (x86)\Steam\SteamApps\common\dota 2 beta\"},
                {"Hearthstone", @"C:\Program Files (x86)\Hearthstone\"},
                {"Heroes of Newerth", @"C:\Program Files (x86)\Heroes of Newerth\"},
                {"Heroes of the Storm", @"C:\Program Files (x86)\Heroes of the Storm\"},
                {"League of Legends", @"C:\Program Files (x86)\League of Legends\"},
                {"Smite", @"C:\Program Files (x86)\Hi-Rez Studios\HiRezGames\smite\"},
                {"StarCraft II", @"C:\Program Files (x86)\StarCraft II\"},
                {"Strife", @"C:\Program Files (x86)\Steam\SteamApps\common\Strife\"}
            };
            public static readonly Dictionary<string, int> GamePathLeaves = new Dictionary<string, int>
            {
                { "Dawngate", 1 },
                { "Dota 2", 1 },
                { "Hearthstone", 1 },
                { "Heroes of Newerth", 1 },
                { "Heroes of the Storm", 1 },
                { "League of Legends", 1 },
                { "Smite", 3 },
                { "StarCraft II", 1 },
                { "Strife", 2 }
            };
        }
    }
}

namespace SpawnedIn.GGA.Main
{

    class GetGameAssets
    {
        private static void VerifyINI()
        {
            // GET PATHS FROM THE INI
            const string ini_section = "Game Paths";
            INIFile ini = new INIFile(Globals.Paths.Conf);
            if (!File.Exists(Globals.Paths.Conf))
            {
                Console.Write("This is your first time running GGA. Configuring.");
                foreach (string game in Globals.Lists.Games)
                {
                    string sourcepath = Globals.Lists.GameSourcePaths[game];
                    ini.INIWriteValue(ini_section, game, sourcepath);
                }
                Console.WriteLine(".Done! :)");
            }
            // READ FROM THE INI
            Dictionary<string, string> game_paths = Globals.Lists.GameBinaryPaths;
            Dictionary<string, int> game_path_leaves = Globals.Lists.GamePathLeaves;
            foreach (string game in Globals.Lists.Games)
            {
                string stored_path = ini.INIReadValue(ini_section, game);
                if (!Directory.Exists(stored_path))
                {
                    Console.WriteLine("Finding {0}", game);
                    string path_to_find = game_paths[game];
                    foreach (string drive in Globals.Paths.Drives)
                    {
                        foreach (string fp in Helper.EnumerateFiles(drive, path_to_find, SearchOption.AllDirectories))
                        {
                            Console.WriteLine("Found {0}", fp);
                            int leafindex = game_path_leaves[game];
                            string mod_path = fp;
                            for (int i = 0; i < leafindex; ++i)
                            {
                                mod_path = Directory.GetParent(mod_path).ToString();
                            }
                            ini.INIWriteValue(ini_section, game, mod_path + @"\");
                            Console.WriteLine("Storing {0}", mod_path + @"\");
                        }
                    }
                }
            }
        }

        static void StartProcess(string classname)
        {
            Type t = Type.GetType("SpawnedIn.GGA.Games." + classname);
            MethodInfo m = t.GetMethod("Process");
            m.Invoke(null, null);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("\nRunning in {0} from {1}\n", Globals.Paths.Work, Globals.Paths.Home);
            VerifyINI();
            string[] games;
            if (args.Length > 0)
            {
                games = args[0].Split(',');
            }
            else
            {
                games = Globals.Lists.Games;
            }
            foreach (string input_game in games)
            {
                string proper = Globals.Lists.Games.First(g => String.Equals(g, input_game.Trim(), StringComparison.OrdinalIgnoreCase));
                string game = (proper).Replace(" ","");
                Helper.PreCleanup(proper);
                Console.WriteLine("Processing {0}", proper);
                Helper.BuildSourceDirectory(proper);
                StartProcess(game);
                // Helper.PostCleanup(proper);
            }
            if (args.Contains("-min", StringComparer.OrdinalIgnoreCase))
            {
                // Minify
                Helper.MinifyPNG();
            }
        }
    }
}
