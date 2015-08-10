using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Blazinix.INI;
using Kurouzu.Defaults;
using Kurouzu.Helpers;

namespace Kurouzu.Games
{
    public class Dawngate
    {
        public static void Process()
        {
            const string abilities = @"Dawngate\Abilities\";
            const string items = @"Dawngate\Items\";
            const string shapersPortrait = @"Dawngate\Shapers\Portrait\";
            const string shapersSquare = @"Dawngate\Shapers\Square\";
            const string sparks = @"Dawngate\Sparks\";
            const string spells = @"Dawngate\Spells\";
            const string spiritstones = @"Dawngate\Spiritstones\";
            string[] directories = { abilities, items, shapersPortrait, shapersSquare, sparks, spells, spiritstones };
            Helper.BuildDirectoryTree(directories);

            // Get the path of the source
            INIFile ini = new INIFile(Globals.Paths.ConfigurationFile);
            string sourcePath = ini.INIReadValue("Game Paths", "Dawngate");

            // Get the source
            string quickBmsSnapFile = Path.Combine(Globals.Paths.Home, @"lib\quickbms_snap.txt");
            string[] sourcePackages = Directory.GetFiles(Path.Combine(sourcePath, "data"), "*.snap", SearchOption.AllDirectories);

            foreach(string sourcePackage in sourcePackages)
            {
                var quickBms = new Process
                {
                    StartInfo = new ProcessStartInfo {
                        FileName = "quickbms.exe",
                        Arguments =
                            $" -o -. -Y -f \"*_full.dds,*_portrait.dds,*_purchase.dds\" \"{quickBmsSnapFile}\" \"{sourcePackage}\" \"{Path.Combine(Globals.Paths.Assets, @"Source\Dawngate")}\"",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                quickBms.Start();
                while (!quickBms.StandardOutput.EndOfStream)
                {
                    string standardOutputLine = quickBms.StandardOutput.ReadLine();
                    Console.WriteLine(standardOutputLine);
                }
            }
            var stragglingQuickBms = new Process
            {
                StartInfo = new ProcessStartInfo {
                    FileName = "quickbms.exe",
                    Arguments =
                        $" -o -. -Y -f \"*advanced_*.dds,*basic_*.dds,*legendary_*.dds,*consumable_*.dds,*Spell_*.dds,*inventory_perk_shape_*.dds,*perk_gem_*.dds\" \"{quickBmsSnapFile}\" \"{Path.Combine(sourcePath, @"Data\UI_Icons.snap")}\" \"{Path.Combine(Globals.Paths.Assets, @"Source\Dawngate")}\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            stragglingQuickBms.Start();
            while (!stragglingQuickBms.StandardOutput.EndOfStream)
            {
                string line = stragglingQuickBms.StandardOutput.ReadLine();
                Console.WriteLine(line);
            }

            // Copy the rest of the source assets
            // Copy jobs take the form { output path = string, { string start path, bool recursion flag, string search pattern, string exclude pattern } }
            List<CopyJob> copyJobs = new List<CopyJob>
            {
                new CopyJob(abilities, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate\heroes"), true, "*_full.dds", null),
                new CopyJob(items, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate\UI\Icons\ShopIcons\textures"), true, "*.dds", "Spell_*.dds"),
                new CopyJob(shapersPortrait, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate\heroes"), true, "*_Purchase.dds", "*_*_Purchase.dds"),
                new CopyJob(shapersSquare, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate\heroes"), true, "*_Portrait.dds", null),
                new CopyJob(sparks, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate\UI\Icons\GemIcons\textures"), true, "perk_gem_*.dds", null),
                new CopyJob(spells, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate\UI\Icons\ShopIcons\textures"), true, "Spell_*.dds", null),
                new CopyJob(spiritstones, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate\UI\Icons\ShapeIcons\textures"), true, "inventory_perk_shape_*.dds", null)
            };
            Helper.BatchFileCopy(copyJobs);

            // Rename all the things
            Helper.BatchFileRename("Dawngate");

            // Scale all the things
            // Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
            List<ScalingJob> scalingJobs = new List<ScalingJob>
            {
                new ScalingJob(abilities),
                new ScalingJob(items),
                new ScalingJob(shapersPortrait),
                new ScalingJob(shapersSquare),
                new ScalingJob(sparks),
                new ScalingJob(spells),
                new ScalingJob(spiritstones)
            };
            Helper.BatchIMScale(scalingJobs);
        }
    }
}
