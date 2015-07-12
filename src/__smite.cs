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
    public class Smite
    {
        public static void Process()
        {
            string abilities = @"Smite\Abilities\";
            string gods_portrait = @"Smite\Gods\Portrait\";
            string gods_square = @"Smite\Gods\Square";
            string items = @"Smite\Items\";
            string[] dirs = { abilities, items, gods_portrait, gods_square };
            Helper.BuildDirectoryTree(dirs);
            // Get the path of the source
            INIFile ini = new INIFile(Globals.Paths.Conf);
            string source_path = ini.INIReadValue("Game Paths", "Smite");
            // Get the source
            string[] upks = Directory.GetFiles(Path.Combine(source_path, @"BattleGame\CookedPC\GUI\Icons"), "*.upk", SearchOption.AllDirectories);
            foreach(string upk in upks)
            {
                var umodel = new Process
                {
                    StartInfo = new ProcessStartInfo {
                        FileName = "umodel.exe",
                        Arguments = String.Format(" -export -uncook -groups -nomesh -noanim -nostat -out=\"{0}\" \"{1}\" -path=\"{2}\"", Path.Combine(Globals.Paths.Assets, @"Source\Smite"), Path.GetFileName(upk), Path.Combine(source_path, @"BattleGame\CookedPC\GUI\Icons")),
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                umodel.Start();
                while (!umodel.StandardOutput.EndOfStream)
                {
                    string line = umodel.StandardOutput.ReadLine();
                    Console.WriteLine(line);
                }
            }
            // Copy the rest of the source assets
            // Copy jobs take the form { string output path, { string start path, bool recursion flag, string search pattern, string exclude pattern } }
            List<CopyJob> copyjobs = new List<CopyJob>
            {
                new CopyJob(abilities, Path.Combine(Globals.Paths.Assets, @"Source\Smite\icons\abilities"), true, "*.tga", null),
                new CopyJob(abilities, Path.Combine(Globals.Paths.Assets, @"Source\Smite\icons\abilitybanners"), true, "Icons_Agni_A01.tga", null),
                new CopyJob(abilities, Path.Combine(Globals.Paths.Assets, @"Source\Smite\icons\abilitybanners"), true, "Icons_Ymir_GlacialStrike.tga", null),
                new CopyJob(gods_portrait, Path.Combine(Globals.Paths.Assets, @"Source\Smite\GodSkins_Cards"), false, "*_Default_Card.tga", null),
                new CopyJob(gods_square, Path.Combine(Globals.Paths.Assets, @"Source\Smite\GodSkins_Portraits_and_Icons"), true, "*_Default_Icon.tga", null),
                // new CopyJob(items, Path.Combine(Globals.Paths.Assets, @"Source\Smite\icons\items_delete"), true, "*.tga", null)
            };
            Helper.BatchFileCopy(copyjobs);
            // Rename all the things
            Helper.BatchFileRename("Smite");
            // Scale all the things
            // Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
            List<ScalingJob> scalingjobs = new List<ScalingJob>
            {
                new ScalingJob(abilities, "agni-q.tga", null),
                new ScalingJob(abilities, "ymir-w.tga", null),
                new ScalingJob(abilities, "*.tga", "agni-q.tga"),
                new ScalingJob(abilities, "*.tga", "ymir-w.tga"),
                new ScalingJob(gods_portrait, "*.tga", null),
                new ScalingJob(gods_square, "*.tga", null),
                new ScalingJob(items, "*.tga", null)
            };
            // Helper.BatchIMScale(scalingjobs);
        }
    }
}
