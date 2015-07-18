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
using Kurouzu.Helpers;
using Kurouzu.Defaults;

namespace Kurouzu.Games
{
    public class StarCraftII
    {
        public static void Process()
        {
            string units_portrait = @"StarCraft II\Units\Portrait\";
            string units_square = @"StarCraft II\Units\Square\";
            string buildings = @"StarCraft II\Buildings\";
            string abilities = @"StarCraft II\Abilities\";
            string upgrades = @"StarCraft II\Upgrades\";
            string ui = @"StarCraft II\UI\";
            string[] dirs = { units_portrait, units_square, buildings, abilities, upgrades, ui };
            Helper.BuildDirectoryTree(dirs);
            // Get the path of the source
            INIFile ini = new INIFile(Globals.Paths.Conf);
            string source_path = ini.INIReadValue("Game Paths", "StarCraft II");
            // Get the source
            string[] filters = { "*portrait*static.dds", "*-unit-*.dds", "*-building-*.dds", "*-ability-*.dds", "*-armor-*.dds", "*-upgrade-*.dds", "*icon-*nobg.dds" };
            string[] mpqs = Directory.GetFiles(Path.Combine(source_path, @"Mods"), "Base.SC2Assets", SearchOption.AllDirectories);
            foreach(string mpq in mpqs)
            {
                string leaf = (Directory.GetParent(mpq).Name).Replace(".SC2Mod", "");
                string dest = Path.Combine(Globals.Paths.Assets,"Source","StarCraft II",leaf);
                foreach (string filter in filters)
                {
                    var mpqeditor = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "MPQEditor.exe",
                            Arguments = String.Format(" e \"{0}\" \"{1}\" \"{2}\" /fp", mpq, filter, dest),
                            WindowStyle = ProcessWindowStyle.Hidden,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                    mpqeditor.Start();
                    while (!mpqeditor.StandardOutput.EndOfStream)
                    {
                        string line = mpqeditor.StandardOutput.ReadLine();
                        Console.WriteLine(line);
                    }
                }
            }
            // Copy the rest of the source assets
            // Copy jobs take the form { string output path, { string start path, bool recursion flag, string search pattern, string exclude pattern } }
            List<CopyJob> copyjobs = new List<CopyJob>
            {
                new CopyJob(units_portrait, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "*portrait_static.dds", null),
                new CopyJob(units_square, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "btn-unit-*.dds", null),
                new CopyJob(buildings, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "btn-building-*.dds", null),
                new CopyJob(abilities, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "btn-ability-*.dds", null),
                new CopyJob(abilities, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "btn-armor-*.dds", null),
                new CopyJob(upgrades, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "btn-upgrade-*.dds", null),
                new CopyJob(ui, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "icon-*-nobg.dds", null),
                new CopyJob(ui, Path.Combine(Globals.Paths.Assets, @"Source\StarCraft II"), true, "icon-supply*_nobg.dds", null)
            };
            Helper.BatchFileCopy(copyjobs);
            // Rename all the things
            Helper.BatchFileRename("StarCraft II");
            // Scale all the things
            // Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
            List<ScalingJob> scalingjobs = new List<ScalingJob>
            {
                new ScalingJob(units_portrait, "*.dds"),
                new ScalingJob(units_square, "*.dds"),
                new ScalingJob(buildings, "*.dds"),
                new ScalingJob(abilities, "*.dds"),
                new ScalingJob(upgrades, "*.dds"),
                new ScalingJob(ui, "*.dds")
            };
            // Helper.BatchIMScale(scalingjobs);
        }
    }
}
