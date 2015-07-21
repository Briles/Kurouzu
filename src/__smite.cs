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
    public class Smite
    {
        public static void Process()
        {
            string Abilities = @"Smite\Abilities\";
            string GodsPortrait = @"Smite\Gods\Portrait\";
            string GodsSquare = @"Smite\Gods\Square";
            string Items = @"Smite\Items\";
            string[] Directories = { Abilities, Items, GodsPortrait, GodsSquare };
            Helper.BuildDirectoryTree(Directories);
            // Get the path of the source
            INIFile INI = new INIFile(Globals.Paths.ConfigurationFile);
            string source_path = INI.INIReadValue("Game Paths", "Smite");
            // Get the source
            string[] SmitePackages = Directory.GetFiles(Path.Combine(source_path, @"BattleGame\CookedPC\GUI\Icons"), "*.upk", SearchOption.AllDirectories);
            foreach(string Package in SmitePackages)
            {
                var Umodel = new Process
                {
                    StartInfo = new ProcessStartInfo {
                        FileName = "umodel.exe",
                        Arguments = String.Format(" -export -uncook -groups -nomesh -noanim -nostat -out=\"{0}\" \"{1}\" -path=\"{2}\"", Path.Combine(Globals.Paths.Assets, @"Source\Smite"), Path.GetFileName(Package), Path.Combine(source_path, @"BattleGame\CookedPC\GUI\Icons")),
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                Umodel.Start();
                while (!Umodel.StandardOutput.EndOfStream)
                {
                    string StandardOutputLine = Umodel.StandardOutput.ReadLine();
                    Console.WriteLine(StandardOutputLine);
                }
            }
            // Copy the rest of the source assets
            // Copy jobs take the form { string output path, { string start path, bool recursion flag, string search pattern, string exclude pattern } }
            List<CopyJob> CopyJobs = new List<CopyJob>
            {
                new CopyJob(Abilities, Path.Combine(Globals.Paths.Assets, @"Source\Smite\icons\abilities"), true, "*.tga", null),
                new CopyJob(Abilities, Path.Combine(Globals.Paths.Assets, @"Source\Smite\icons\abilitybanners"), true, "Icons_Agni_A01.tga", null),
                new CopyJob(Abilities, Path.Combine(Globals.Paths.Assets, @"Source\Smite\icons\abilitybanners"), true, "Icons_Ymir_GlacialStrike.tga", null),
                new CopyJob(GodsPortrait, Path.Combine(Globals.Paths.Assets, @"Source\Smite\GodSkins_Cards"), false, "*_Default_Card.tga", null),
                new CopyJob(GodsSquare, Path.Combine(Globals.Paths.Assets, @"Source\Smite\GodSkins_Portraits_and_Icons"), true, "*_Default_Icon.tga", null),
                // new CopyJob(items, Path.Combine(Globals.Paths.Assets, @"Source\Smite\icons\items_delete"), true, "*.tga", null)
            };
            Helper.BatchFileCopy(CopyJobs);
            // Rename all the things
            Helper.BatchFileRename("Smite");
            // Scale all the things
            // Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
            List<ScalingJob> ScalingJobs = new List<ScalingJob>
            {
                new ScalingJob(Abilities, "agni-q.tga", null),
                new ScalingJob(Abilities, "ymir-w.tga", null),
                new ScalingJob(Abilities, "*.tga", "agni-q.tga"),
                new ScalingJob(Abilities, "*.tga", "ymir-w.tga"),
                new ScalingJob(GodsPortrait, "*.tga", null),
                new ScalingJob(GodsSquare, "*.tga", null),
                new ScalingJob(Items, "*.tga", null)
            };
            Helper.BatchIMScale(ScalingJobs);
        }
    }
}
