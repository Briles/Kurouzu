using Blazinix.INI;
using Kurouzu.Defaults;
using Kurouzu.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Kurouzu.Games
{
    public class StarCraftII
    {
        public static void Process()
        {
            const string unitsPortrait = @"StarCraft II\Units\Portrait\";
            const string unitsSquare = @"StarCraft II\Units\Square\";
            const string buildings = @"StarCraft II\Buildings\";
            const string abilities = @"StarCraft II\Abilities\";
            const string upgrades = @"StarCraft II\Upgrades\";
            const string ui = @"StarCraft II\UI\";
            string[] directories = { unitsPortrait, unitsSquare, buildings, abilities, upgrades, ui };
            Helper.BuildDirectoryTree(directories);

            // Get the path of the source
            INIFile ini = new INIFile(Globals.Paths.ConfigurationFile);
            string sourcePath = ini.INIReadValue("Game Paths", "StarCraft II");

            // Get the source
            string[] filters = { "*portrait*static.dds", "*-unit-*.dds", "*-building-*.dds", "*-ability-*.dds", "*-armor-*.dds", "*-upgrade-*.dds", "*icon-*nobg.dds" };
            string[] packages = Directory.GetFiles(Path.Combine(sourcePath, @"Mods"), "Base.SC2Assets", SearchOption.AllDirectories);
            foreach(string package in packages)
            {
                string pathLeaf = (Directory.GetParent(package).Name).Replace(".SC2Mod", "");
                string destination = Path.Combine(Globals.Paths.Assets,"Source","StarCraft II",pathLeaf);
                foreach (string filter in filters)
                {
                    var mpqEditor = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "MPQEditor.exe",
                            Arguments = $" e \"{package}\" \"{filter}\" \"{destination}\" /fp",
                            WindowStyle = ProcessWindowStyle.Hidden,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                    mpqEditor.Start();
                    while (!mpqEditor.StandardOutput.EndOfStream)
                    {
                        string standardOutputLine = mpqEditor.StandardOutput.ReadLine();
                        Console.WriteLine(standardOutputLine);
                    }
                }
            }

            // Copy the rest of the source assets
            // Copy jobs take the form { string output path, { string start path, bool recursion flag, string search pattern, string exclude pattern } }
            List<CopyJob> copyJobs = new List<CopyJob>
            {
                new CopyJob(unitsPortrait, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "*portrait_static.dds", null),
                new CopyJob(unitsSquare, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "btn-unit-*.dds", null),
                new CopyJob(buildings, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "btn-building-*.dds", null),
                new CopyJob(abilities, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "btn-ability-*.dds", null),
                new CopyJob(abilities, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "btn-armor-*.dds", null),
                new CopyJob(upgrades, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "btn-upgrade-*.dds", null),
                new CopyJob(ui, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "icon-*-nobg.dds", null),
                new CopyJob(ui, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "icon-supply*_nobg.dds", null)
            };
            Helper.BatchFileCopy(copyJobs);

            // Rename all the things
            Helper.BatchFileRename("StarCraft II");

            // Scale all the things
            // Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
            List<ScalingJob> scalingJobs = new List<ScalingJob>
            {
                new ScalingJob(unitsPortrait, "*.dds"),
                new ScalingJob(unitsSquare, "*.dds"),
                new ScalingJob(buildings, "*.dds"),
                new ScalingJob(abilities, "*.dds"),
                new ScalingJob(upgrades, "*.dds"),
                new ScalingJob(ui, "*.dds")
            };
            Helper.BatchIMScale(scalingJobs);
        }
    }
}
