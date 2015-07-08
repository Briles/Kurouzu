using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
//
using Blazinix.INI;
using SpawnedIn.GGA.Helpers;
using SpawnedIn.GGA.Defaults;

namespace SpawnedIn.GGA.Games
{
    public class Dota2
    {
        public static void Process()
        {
            string heroes_mini = @"Dota 2\Heroes\Mini\";
            string heroes_landscape = @"Dota 2\Heroes\Landscape\";
            string heroes_portrait = @"Dota 2\Heroes\Portrait\";
            string items = @"Dota 2\Items\";
            string spells = @"Dota 2\Spells\";
            string[] dirs = { heroes_mini, heroes_landscape, heroes_portrait, items, spells };
            Helper.BuildDirectoryTree(dirs);
            // Get the path of the source
            INIFile ini = new INIFile(Globals.Paths.Conf);
            string source_path = ini.INIReadValue("Game Paths", "Dota 2");
            // Get the source
            string[] vpks = {"heroes", @"heroes\selection", "miniheroes", "spellicons", "items"};
            Parallel.ForEach(vpks, vpk =>
            {
                var hlextract = new Process
                {
                    StartInfo = new ProcessStartInfo {
                        FileName = "hlextract.exe",
                        Arguments = String.Format(" -p \"{0}\" -d \"{1}\" -e \"{2}\"", Path.Combine(source_path, @"dota\pak01_dir.vpk"),Path.Combine(Globals.Paths.Assets, @"Source\Dota 2"), String.Format(@"root\resource\flash3\images\{0}", vpk)),
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
            });
            // Copy the rest of the source assets
            // Copy jobs take the form { string output path, { string start path, bool recursion flag, string search pattern, string exclude pattern } }
            List<CopyJob> copyjobs = new List<CopyJob>
            {
                new CopyJob(heroes_portrait, Path.Combine(Globals.Paths.Assets, @"Source\Dota 2\selection"), true, "npc_dota_hero_*.png", null),
                new CopyJob(heroes_landscape, Path.Combine(Globals.Paths.Assets, @"Source\Dota 2\heroes"), false, "*.png", null),
                new CopyJob(heroes_mini, Path.Combine(Globals.Paths.Assets, @"Source\Dota 2\miniheroes"), true, "*.png", null),
                new CopyJob(spells, Path.Combine(Globals.Paths.Assets, @"Source\Dota 2\spellicons"), true, "*.png", null),
                new CopyJob(items, Path.Combine(Globals.Paths.Assets, @"Source\Dota 2\items"), true, "*.png", null)
            };
            Helper.BatchFileCopy(copyjobs);
            // Rename all the things
            Helper.BatchFileRename("Dota 2");
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
            Helper.BatchIMScale(scalingjobs);
        }
    }
}
