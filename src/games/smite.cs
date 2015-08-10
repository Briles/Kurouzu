using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Blazinix.INI;
using Kurouzu.Defaults;
using Kurouzu.Helpers;

namespace Kurouzu.Games
{
    public class Smite
    {
        public static void Process()
        {
            const string abilities = @"Smite\Abilities\";
            const string godsPortrait = @"Smite\Gods\Portrait\";
            const string godsSquare = @"Smite\Gods\Square";
            const string items = @"Smite\Items\";
            string[] directories = { abilities, items, godsPortrait, godsSquare };

            Helper.BuildDirectoryTree(directories);

            // Get the path of the source
            var ini = new INIFile(Globals.Paths.ConfigurationFile);
            string sourcePath = ini.INIReadValue("Game Paths", "Smite");

            // Get the source
            string[] smitePackages = { "GodSkins_Cards.upk", "GodSkins_Portraits_and_Icons.upk", "Icons.upk", "Portraits.upk" };
            foreach (string packageName in smitePackages)
            {
                string package = Path.Combine(sourcePath, @"BattleGame\CookedPC\GUI\Icons", packageName);

                var umodel = new Process
                {
                    StartInfo = new ProcessStartInfo {
                        FileName = "umodel.exe",
                        Arguments =
                            $" -export -uncook -groups -nomesh -noanim -nostat -out=\"{Path.Combine(Globals.Paths.Assets, @"Source\Smite")}\" \"{Path.GetFileName(package)}\" -path=\"{Path.Combine(sourcePath, @"BattleGame\CookedPC\GUI\Icons")}\"",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                umodel.Start();
                while (!umodel.StandardOutput.EndOfStream)
                {
                    string standardOutputLine = umodel.StandardOutput.ReadLine();
                    Console.WriteLine(standardOutputLine);
                }
            }

            // Copy the rest of the source assets
            // Copy jobs take the form { string output path, { string start path, bool recursion flag, string search pattern, string exclude pattern } }
            List<CopyJob> copyJobs = new List<CopyJob>
            {
                new CopyJob(abilities, Path.Combine(Globals.Paths.Assets, @"Source\Smite\icons\abilities"), true, "*.tga", null),
                new CopyJob(abilities, Path.Combine(Globals.Paths.Assets, @"Source\Smite\icons\abilitybanners"), true, "Icons_Agni_A01.tga", null),
                new CopyJob(abilities, Path.Combine(Globals.Paths.Assets, @"Source\Smite\icons\abilitybanners"), true, "Icons_Ymir_GlacialStrike.tga", null),
                new CopyJob(godsPortrait, Path.Combine(Globals.Paths.Assets, @"Source\Smite\GodSkins_Cards"), false, "*_Default_Card.tga", null),
                new CopyJob(godsSquare, Path.Combine(Globals.Paths.Assets, @"Source\Smite\GodSkins_Portraits_and_Icons"), true, "*_Default_Icon.tga", null)
                // new CopyJob(items, Path.Combine(Globals.Paths.Assets, @"Source\Smite\icons\items_delete"), true, "*.tga", null)
            };
            Helper.BatchFileCopy(copyJobs);

            // Rename all the things
            Helper.BatchFileRename("Smite");

            // Scale all the things
            // Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
            List<ScalingJob> scalingJobs = new List<ScalingJob>
            {
                new ScalingJob(abilities, "agni-q.tga", null),
                new ScalingJob(abilities, "ymir-w.tga", null),
                new ScalingJob(abilities, "*.tga", "agni-q.tga"),
                new ScalingJob(abilities, "*.tga", "ymir-w.tga"),
                new ScalingJob(godsPortrait, "*.tga", null),
                new ScalingJob(godsSquare, "*.tga", null),
                new ScalingJob(items, "*.tga", null)
            };
            Helper.BatchIMScale(scalingJobs);
        }
    }
}
