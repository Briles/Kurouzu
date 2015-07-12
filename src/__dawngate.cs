using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
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
    public class Dawngate
    {
        public static void Process()
        {
            string abilities = @"Dawngate\Abilities\";
            string items = @"Dawngate\Items\";
            string shapers_portrait = @"Dawngate\Shapers\Portrait\";
            string shapers_square = @"Dawngate\Shapers\Square\";
            string sparks = @"Dawngate\Sparks\";
            string spells = @"Dawngate\Spells\";
            string spiritstones = @"Dawngate\Spiritstones\";
            string[] dirs = { abilities, items, shapers_portrait, shapers_square, sparks, spells, spiritstones };
            Helper.BuildDirectoryTree(dirs);
            // Get the path of the source
            INIFile ini = new INIFile(Globals.Paths.Conf);
            string source_path = ini.INIReadValue("Game Paths", "Dawngate");
            // Get the source
            string quickbms_snap = Path.Combine(Globals.Paths.Home, @"bin\quickbms_snap.txt");
            string[] snaps = Directory.GetFiles(Path.Combine(source_path, "data"), "*.snap", SearchOption.AllDirectories);
            foreach(string snap in snaps)
            {
                var quickbms = new Process
                {
                    StartInfo = new ProcessStartInfo {
                        FileName = "quickbms.exe",
                        Arguments = String.Format(" -o -. -Y -f \"*_full.dds,*_portrait.dds,*_purchase.dds\" \"{0}\" \"{1}\" \"{2}\"", quickbms_snap, snap, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate")),
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                quickbms.Start();
                while (!quickbms.StandardOutput.EndOfStream)
                {
                    string line = quickbms.StandardOutput.ReadLine();
                    Console.WriteLine(line);
                }
            }
            var postbms = new Process
            {
                StartInfo = new ProcessStartInfo {
                    FileName = "quickbms.exe",
                    Arguments = String.Format(" -o -. -Y -f \"*advanced_*.dds,*basic_*.dds,*legendary_*.dds,*consumable_*.dds,*Spell_*.dds,*inventory_perk_shape_*.dds,*perk_gem_*.dds\" \"{0}\" \"{1}\" \"{2}\"", quickbms_snap, Path.Combine(source_path, @"Data\UI_Icons.snap"), Path.Combine(Globals.Paths.Assets, @"Source\Dawngate")),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            postbms.Start();
            while (!postbms.StandardOutput.EndOfStream)
            {
                string line = postbms.StandardOutput.ReadLine();
                Console.WriteLine(line);
            }
            // Copy the rest of the source assets
            // Copy jobs take the form { output path = string, { string start path, bool recursion flag, string search pattern, string exclude pattern } }
            List<CopyJob> copyjobs = new List<CopyJob>
            {
                new CopyJob(abilities, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate\heroes"), true, "*_full.dds", null),
                new CopyJob(items, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate\UI\Icons\ShopIcons\textures"), true, "*.dds", "Spell_*.dds"),
                new CopyJob(shapers_portrait, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate\heroes"), true, "*_Purchase.dds", "*_*_Purchase.dds"),
                new CopyJob(shapers_square, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate\heroes"), true, "*_Portrait.dds", null),
                new CopyJob(sparks, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate\UI\Icons\GemIcons\textures"), true, "perk_gem_*.dds", null),
                new CopyJob(spells, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate\UI\Icons\ShopIcons\textures"), true, "Spell_*.dds", null),
                new CopyJob(spiritstones, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate\UI\Icons\ShapeIcons\textures"), true, "inventory_perk_shape_*.dds", null)
            };
            Helper.BatchFileCopy(copyjobs);
            // Rename all the things
            Helper.BatchFileRename("Dawngate");
            // Scale all the things
            // Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
            List<ScalingJob> scalingjobs = new List<ScalingJob>
            {
                new ScalingJob(abilities),
                new ScalingJob(items),
                new ScalingJob(shapers_portrait),
                new ScalingJob(shapers_square),
                new ScalingJob(sparks),
                new ScalingJob(spells),
                new ScalingJob(spiritstones)
            };
            // Helper.BatchIMScale(scalingjobs);
        }
    }
}
