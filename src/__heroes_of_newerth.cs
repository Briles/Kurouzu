using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
//
using Blazinix.INI;
using Kurouzu.Helpers;
using Kurouzu.Defaults;

namespace Kurouzu.Games
{
    public class HeroesofNewerth
    {
        public static void Process()
        {
            string heroes_mini = @"Heroes of Newerth\Heroes\Mini\";
            string heroes_landscape = @"Heroes of Newerth\Heroes\Landscape\";
            string heroes_portrait = @"Heroes of Newerth\Heroes\Portrait\";
            string items = @"Heroes of Newerth\Items\";
            string spells = @"Heroes of Newerth\Spells\";
            string[] dirs = { heroes_mini, heroes_landscape, heroes_portrait, items, spells };
            Helper.BuildDirectoryTree(dirs);
            // Get the path of the source
            INIFile ini = new INIFile(Globals.Paths.Conf);
            string source_path = ini.INIReadValue("Game Paths", "Heroes of Newerth");
            // Get the source
            string[] vpks = {"heroes", @"heroes\selection", "miniheroes", "spellicons", "items"};
            foreach(string vpk in vpks)
            {
                var hlextract = new Process
                {
                    StartInfo = new ProcessStartInfo {
                        FileName = "hlextract.exe",
                        Arguments = String.Format(" -p \"{0}\" -d \"{1}\" -e \"{2}\"", Path.Combine(source_path, @"dota\pak01_dir.vpk"),Path.Combine(Globals.Paths.Assets, @"Source\Heroes of Newerth"), String.Format(@"root\resource\flash3\images\{0}", vpk)),
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                hlextract.Start();
                while (!hlextract.StandardOutput.EndOfStream)
                {
                    string line = hlextract.StandardOutput.ReadLine();
                    Console.WriteLine(line);
                }
            }
            // Copy the rest of the source assets
            // Copy jobs take the form { string output path, { string start path, bool recursion flag, string search pattern, string exclude pattern } }
            List<CopyJob> copyjobs = new List<CopyJob>
            {
                new CopyJob(heroes_portrait, Path.Combine(Globals.Paths.Assets, @"Source\Heroes of Newerth\selection"), true, "npc_dota_hero_*.png", null),
                new CopyJob(heroes_landscape, Path.Combine(Globals.Paths.Assets, @"Source\Heroes of Newerth\heroes"), false, "*.png", null),
                new CopyJob(heroes_mini, Path.Combine(Globals.Paths.Assets, @"Source\Heroes of Newerth\miniheroes"), true, "*.png", null),
                new CopyJob(spells, Path.Combine(Globals.Paths.Assets, @"Source\Heroes of Newerth\spellicons"), true, "*.png", null),
                new CopyJob(items, Path.Combine(Globals.Paths.Assets, @"Source\Heroes of Newerth\items"), true, "*.png", null)
            };
            Helper.BatchFileCopy(copyjobs);
            // Rename all the things
            Helper.BatchFileRename("Heroes of Newerth");
            // Scale all the things
            // Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
            List<ScalingJob> scalingjobs = new List<ScalingJob>
            {
                new ScalingJob(heroes_landscape, "*.png"),
                new ScalingJob(heroes_mini, "*.png"),
                new ScalingJob(heroes_portrait, "*.png"),
                new ScalingJob(items, "*.png"),
                new ScalingJob(spells, "*.png")
            };
            // Helper.BatchIMScale(scalingjobs);
        }
    }
}
