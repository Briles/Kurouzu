using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Kurouzu.Defaults
{

    public class GameInfo
    {
        public string Title { get; set; }
        public string Binary { get; set; }
        public string Source { get; set; }
        public int Leaf { get; set; }
        public Dictionary<string, string> Dimensions { get; set; }

        public static GameInfo GetGamebyProp(string title)
        {
            return Globals.Games.First(item => string.Equals(item.Title, title, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static class Globals
    {
        public static class Paths
        {
            public static readonly string Work = (Directory.GetCurrentDirectory() + @"\");
            public static readonly string Assets = Path.Combine(Work, "Assets");
            public static readonly string Home = (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\");
            public static readonly string ConfigurationFile = Path.Combine(Home, "conf.ini");
            public static readonly string Data = Path.Combine(Home, "data");
            public static readonly string Logs = Path.Combine(Home, "logs");
            public static readonly string[] Drives = Environment.GetLogicalDrives();
            public static readonly string[] Arguments = Environment.GetCommandLineArgs();
        }

        public static readonly List<GameInfo> Games = new List<GameInfo>() {
            new GameInfo {
                Title = "Dawngate",
                Binary = "Dawngate.exe",
                Source = @"C:\Program Files (x86)\Electronic Arts\Dawngate\game\",
                Leaf = 1,
                Dimensions = new Dictionary<string, string>() {
                    { "Abilities", "64x64" },
                    { "Items", "256x256" },
                    { "Shapers\\Portrait", "64x128" },
                    { "Shapers\\Square", "256x256" },
                    { "Sparks", "64x64" },
                    { "Spells", "256x256" },
                    { "Spiritstones", "64x64" }
                }
            },
            new GameInfo { 
                Title = "Dota 2",
                Binary = "dota.exe",
                Source = @"C:\Program Files (x86)\Steam\SteamApps\common\dota 2 beta\",
                Leaf = 1,
                Dimensions = new Dictionary<string, string>() {
                    { "Heroes\\Landscape", "128x72" },
                    { "Heroes\\Mini", "32x32" },
                    { "Heroes\\Portrait", "71x94" },
                    { "Items", "88x64" },
                    { "Spells", "128x128" }
                }
            },
            new GameInfo {
                Title = "Hearthstone",
                Binary = "Hearthstone.exe",
                Source = @"C:\Program Files (x86)\Hearthstone\",
                Leaf = 1,
                Dimensions = new Dictionary<string, string>() {}
            },
            new GameInfo {
                Title = "Heroes of Newerth",
                Binary = "hon.exe",
                Source = @"C:\Program Files (x86)\Heroes of Newerth\",
                Leaf = 1,
                Dimensions = new Dictionary<string, string>() {
                    { "Abilities", "128x128" },
                    { "Heroes", "128x128" },
                    { "Items", "128x128" }
                }
            },
            new GameInfo {
                Title = "Heroes of the Storm",
                Binary = "Heroes of the Storm.exe",
                Source = @"C:\Program Files (x86)\Heroes of the Storm\",
                Leaf = 1,
                Dimensions = new Dictionary<string, string>() {
                    { "Heroes\\Hexagon", "157x127" },
                    { "Heroes\\HexagonM", "80x92" },
                    { "Heroes\\Portrait", "149x347" },
                    { "Heroes\\Round", "32x32" },
                    { "Heroes\\Square", "92x93" },
                    { "Talents", "76x76" }
                }
            },
            new GameInfo {
                Title = "League of Legends",
                Binary = "lol.launcher.exe",
                Source = @"C:\Program Files (x86)\League of Legends\",
                Leaf = 1,
                Dimensions = new Dictionary<string, string>() {
                    { "Abilities", "64x64"},
                    { "Champions\\Landscape", "1215x717" },
                    { "Champions\\Portrait", "308x560" },
                    { "Champions\\Square", "120x120" },
                    { "Items", "64x64" },
                    { "Masteries", "64x64" },
                    { "Runes", "64x64" },
                    { "Spells", "64x64" }
                }
            },
            new GameInfo {
                Title = "Smite",
                Binary = "Smite.exe",
                Source = @"C:\Program Files (x86)\Hi-Rez Studios\HiRezGames\smite\",
                Leaf = 3,
                Dimensions = new Dictionary<string, string>() {
                    { "Abilities", "64x64" },
                    { "Gods\\Portrait", "388x512" },
                    { "Gods\\Square", "128x128" },
                    { "Items", "128x128" }
                }
            },
            new GameInfo {
                Title = "StarCraft II",
                Binary = "StarCraft II.exe",
                Source = @"C:\Program Files (x86)\StarCraft II\",
                Leaf = 1,
                Dimensions = new Dictionary<string, string>() {
                    { "Abilities", "64x64" },
                    { "Buildings", "76x76" },
                    { "UI", "64x64" },
                    { "Units\\Portrait", "152x232" },
                    { "Units\\Square", "76x76" },
                    { "Upgrades", "64x64" }
                }
            },
            new GameInfo {
                Title = "Strife",
                Binary = "strife.exe",
                Source = @"C:\Program Files (x86)\Steam\SteamApps\common\Strife\", 
                Leaf = 2,
                Dimensions = new Dictionary<string, string>() {}
            }
        };
    }
}
