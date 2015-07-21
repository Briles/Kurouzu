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
    public class Dota2
    {
        public static void Process()
        {
            const string HeroesMini = @"Dota 2\Heroes\Mini\";
            const string HeroesLandscape = @"Dota 2\Heroes\Landscape\";
            const string HeroesPortrait = @"Dota 2\Heroes\Portrait\";
            const string Items = @"Dota 2\Items\";
            const string Spells = @"Dota 2\Spells\";
            string[] Directories = { HeroesMini, HeroesLandscape, HeroesPortrait, Items, Spells };
            Helper.BuildDirectoryTree(Directories);
            // Get the path of the source
            INIFile INI = new INIFile(Globals.Paths.ConfigurationFile);
            string source_path = INI.INIReadValue("Game Paths", "Dota 2");
            // Get the source
            string[] ValvePackages = {"heroes", @"heroes\selection", "miniheroes", "spellicons", "items"};
            foreach(string ValvePackage in ValvePackages)
            {
                var HLExtract = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "hlextract.exe",
                        Arguments = String.Format(" -p \"{0}\" -d \"{1}\" -e \"{2}\"", Path.Combine(source_path, @"dota\pak01_dir.vpk"), Path.Combine(Globals.Paths.Assets, @"Source\Dota 2"), String.Format(@"root\resource\flash3\images\{0}", ValvePackage)),
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                HLExtract.Start();
                while (!HLExtract.StandardOutput.EndOfStream)
                {
                    string StandardOutputLine = HLExtract.StandardOutput.ReadLine();
                    Console.WriteLine(StandardOutputLine);
                }
            }
            // Copy the rest of the source assets
            // Copy jobs take the form { string output path, { string start path, bool recursion flag, string search pattern, string exclude pattern } }
            List<CopyJob> CopyJobs = new List<CopyJob>
            {
                new CopyJob(HeroesPortrait, Path.Combine(Globals.Paths.Assets, @"Source\Dota 2\selection"), true, "npc_dota_hero_*.png", null),
                new CopyJob(HeroesLandscape, Path.Combine(Globals.Paths.Assets, @"Source\Dota 2\heroes"), false, "*.png", null),
                new CopyJob(HeroesMini, Path.Combine(Globals.Paths.Assets, @"Source\Dota 2\miniheroes"), true, "*.png", null),
                new CopyJob(Spells, Path.Combine(Globals.Paths.Assets, @"Source\Dota 2\spellicons"), true, "*.png", null),
                new CopyJob(Items, Path.Combine(Globals.Paths.Assets, @"Source\Dota 2\items"), true, "*.png", null)
            };
            Helper.BatchFileCopy(CopyJobs);
            // Rename all the things
            Helper.BatchFileRename("Dota 2");
            // Scale all the things
            // Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
            List<ScalingJob> ScalingJobs = new List<ScalingJob>
            {
                new ScalingJob(HeroesLandscape, "*.png"),
                new ScalingJob(HeroesMini, "*.png"),
                new ScalingJob(HeroesPortrait, "*.png"),
                new ScalingJob(Items, "*.png"),
                new ScalingJob(Spells, "*.png")
            };
            Helper.BatchIMScale(ScalingJobs);
        }
    }
}
