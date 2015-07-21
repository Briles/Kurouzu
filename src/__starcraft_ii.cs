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
    public class StarCraftII
    {
        public static void Process()
        {
            const string UnitsPortrait = @"StarCraft II\Units\Portrait\";
            const string UnitsSquare = @"StarCraft II\Units\Square\";
            const string Buildings = @"StarCraft II\Buildings\";
            const string Abilities = @"StarCraft II\Abilities\";
            const string Upgrades = @"StarCraft II\Upgrades\";
            const string UI = @"StarCraft II\UI\";
            string[] Directories = { UnitsPortrait, UnitsSquare, Buildings, Abilities, Upgrades, UI };
            Helper.BuildDirectoryTree(Directories);
            // Get the path of the source
            INIFile INI = new INIFile(Globals.Paths.ConfigurationFile);
            string source_path = INI.INIReadValue("Game Paths", "StarCraft II");
            // Get the source
            string[] Filters = { "*portrait*static.dds", "*-unit-*.dds", "*-building-*.dds", "*-ability-*.dds", "*-armor-*.dds", "*-upgrade-*.dds", "*icon-*nobg.dds" };
            string[] Packages = Directory.GetFiles(Path.Combine(source_path, @"Mods"), "Base.SC2Assets", SearchOption.AllDirectories);
            foreach(string Package in Packages)
            {
                string PathLeaf = (Directory.GetParent(Package).Name).Replace(".SC2Mod", "");
                string Destination = Path.Combine(Globals.Paths.Assets,"Source","StarCraft II",PathLeaf);
                foreach (string Filter in Filters)
                {
                    var MPQEditor = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "MPQEditor.exe",
                            Arguments = String.Format(" e \"{0}\" \"{1}\" \"{2}\" /fp", Package, Filter, Destination),
                            WindowStyle = ProcessWindowStyle.Hidden,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                    MPQEditor.Start();
                    while (!MPQEditor.StandardOutput.EndOfStream)
                    {
                        string StandardOutputLine = MPQEditor.StandardOutput.ReadLine();
                        Console.WriteLine(StandardOutputLine);
                    }
                }
            }
            // Copy the rest of the source assets
            // Copy jobs take the form { string output path, { string start path, bool recursion flag, string search pattern, string exclude pattern } }
            List<CopyJob> CopyJobs = new List<CopyJob>
            {
                new CopyJob(UnitsPortrait, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "*portrait_static.dds", null),
                new CopyJob(UnitsSquare, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "btn-unit-*.dds", null),
                new CopyJob(Buildings, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "btn-building-*.dds", null),
                new CopyJob(Abilities, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "btn-ability-*.dds", null),
                new CopyJob(Abilities, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "btn-armor-*.dds", null),
                new CopyJob(Upgrades, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "btn-upgrade-*.dds", null),
                new CopyJob(UI, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "icon-*-nobg.dds", null),
                new CopyJob(UI, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "icon-supply*_nobg.dds", null)
            };
            Helper.BatchFileCopy(CopyJobs);
            // Rename all the things
            Helper.BatchFileRename("StarCraft II");
            // Scale all the things
            // Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
            List<ScalingJob> ScalingJobs = new List<ScalingJob>
            {
                new ScalingJob(UnitsPortrait, "*.dds"),
                new ScalingJob(UnitsSquare, "*.dds"),
                new ScalingJob(Buildings, "*.dds"),
                new ScalingJob(Abilities, "*.dds"),
                new ScalingJob(Upgrades, "*.dds"),
                new ScalingJob(UI, "*.dds")
            };
            Helper.BatchIMScale(ScalingJobs);
        }
    }
}
