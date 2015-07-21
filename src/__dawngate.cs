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
    public class Dawngate
    {
        public static void Process()
        {
            const string Abilities = @"Dawngate\Abilities\";
            const string Items = @"Dawngate\Items\";
            const string ShapersPortrait = @"Dawngate\Shapers\Portrait\";
            const string ShapersSquare = @"Dawngate\Shapers\Square\";
            const string Sparks = @"Dawngate\Sparks\";
            const string Spells = @"Dawngate\Spells\";
            const string Spiritstones = @"Dawngate\Spiritstones\";
            string[] Directories = { Abilities, Items, ShapersPortrait, ShapersSquare, Sparks, Spells, Spiritstones };
            Helper.BuildDirectoryTree(Directories);
            // Get the path of the source
            INIFile INI = new INIFile(Globals.Paths.ConfigurationFile);
            string SourcePath = INI.INIReadValue("Game Paths", "Dawngate");
            // Get the source
            string QuickBMSSnapFile = Path.Combine(Globals.Paths.Home, @"bin\quickbms_snap.txt");
            string[] SourcePackages = Directory.GetFiles(Path.Combine(SourcePath, "data"), "*.snap", SearchOption.AllDirectories);
            foreach(string SourcePackage in SourcePackages)
            {
                var QuickBMS = new Process
                {
                    StartInfo = new ProcessStartInfo {
                        FileName = "quickbms.exe",
                        Arguments = string.Format(" -o -. -Y -f \"*_full.dds,*_portrait.dds,*_purchase.dds\" \"{0}\" \"{1}\" \"{2}\"", QuickBMSSnapFile, SourcePackage, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate")),
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                QuickBMS.Start();
                while (!QuickBMS.StandardOutput.EndOfStream)
                {
                    string StandardOutputLine = QuickBMS.StandardOutput.ReadLine();
                    Console.WriteLine(StandardOutputLine);
                }
            }
            var StragglingQuickBMS = new Process
            {
                StartInfo = new ProcessStartInfo {
                    FileName = "quickbms.exe",
                    Arguments = string.Format(" -o -. -Y -f \"*advanced_*.dds,*basic_*.dds,*legendary_*.dds,*consumable_*.dds,*Spell_*.dds,*inventory_perk_shape_*.dds,*perk_gem_*.dds\" \"{0}\" \"{1}\" \"{2}\"", QuickBMSSnapFile, Path.Combine(SourcePath, @"Data\UI_Icons.snap"), Path.Combine(Globals.Paths.Assets, @"Source\Dawngate")),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            StragglingQuickBMS.Start();
            while (!StragglingQuickBMS.StandardOutput.EndOfStream)
            {
                string line = StragglingQuickBMS.StandardOutput.ReadLine();
                Console.WriteLine(line);
            }
            // Copy the rest of the source assets
            // Copy jobs take the form { output path = string, { string start path, bool recursion flag, string search pattern, string exclude pattern } }
            List<CopyJob> CopyJobs = new List<CopyJob>
            {
                new CopyJob(Abilities, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate\heroes"), true, "*_full.dds", null),
                new CopyJob(Items, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate\UI\Icons\ShopIcons\textures"), true, "*.dds", "Spell_*.dds"),
                new CopyJob(ShapersPortrait, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate\heroes"), true, "*_Purchase.dds", "*_*_Purchase.dds"),
                new CopyJob(ShapersSquare, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate\heroes"), true, "*_Portrait.dds", null),
                new CopyJob(Sparks, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate\UI\Icons\GemIcons\textures"), true, "perk_gem_*.dds", null),
                new CopyJob(Spells, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate\UI\Icons\ShopIcons\textures"), true, "Spell_*.dds", null),
                new CopyJob(Spiritstones, Path.Combine(Globals.Paths.Assets, @"Source\Dawngate\UI\Icons\ShapeIcons\textures"), true, "inventory_perk_shape_*.dds", null)
            };
            Helper.BatchFileCopy(CopyJobs);
            // Rename all the things
            Helper.BatchFileRename("Dawngate");
            // Scale all the things
            // Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
            List<ScalingJob> ScalingJobs = new List<ScalingJob>
            {
                new ScalingJob(Abilities),
                new ScalingJob(Items),
                new ScalingJob(ShapersPortrait),
                new ScalingJob(ShapersSquare),
                new ScalingJob(Sparks),
                new ScalingJob(Spells),
                new ScalingJob(Spiritstones)
            };
            Helper.BatchIMScale(ScalingJobs);
        }
    }
}
