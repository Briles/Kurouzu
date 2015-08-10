using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Blazinix.INI;
using Kurouzu.Defaults;
using Kurouzu.Helpers;

namespace Kurouzu.Games
{
    public class HeroesoftheStorm
    {
        public static void Process()
        {
            const string heroesPortrait = @"Heroes of the Storm\Heroes\Portrait\";
            const string heroesLandscape = @"Heroes of the Storm\Heroes\Portrait\";
            const string heroesRound = @"Heroes of the Storm\Heroes\Round\";
            const string heroesBanner = @"Heroes of the Storm\Heroes\Banner\";
            const string talents = @"Heroes of the Storm\Talents\";
            const string ui = @"Heroes of the Storm\UI\";
            string[] directories = { heroesPortrait, heroesLandscape, heroesRound, heroesBanner, talents, ui };
            Helper.BuildDirectoryTree(directories);

            // Get the path of the source
            INIFile ini = new INIFile(Globals.Paths.ConfigurationFile);
            string sourcePath = ini.INIReadValue("Game Paths", "Heroes of the Storm");

            // Get the source
            // string[] filters = { "*portrait*static.dds", "*-unit-*.dds", "*-building-*.dds", "*-ability-*.dds", "*-armor-*.dds", "*-upgrade-*.dds", "*icon-*nobg.dds" };
            string gameData = Path.Combine(sourcePath, @"HeroesData");
            string destinationPath = Path.Combine(Globals.Paths.Assets, "Source", "Heroes of the Storm");

            var cascView = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "CascView.exe",
                    Arguments = $" \"{gameData}\" \"*.dds\" \"{destinationPath}\" /fp",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            cascView.Start();
            while (!cascView.StandardOutput.EndOfStream)
            {
                string standardOutputLine = cascView.StandardOutput.ReadLine();
                Console.WriteLine(standardOutputLine);
            }

            // Copy the rest of the source assets
            // Copy jobs take the form { string output path, { string start path, bool recursion flag, string search pattern, string exclude pattern } }
            List<CopyJob> copyJobs = new List<CopyJob>
            {
                new CopyJob(heroesPortrait, Path.Combine(Globals.Paths.Assets, @"Source\Heroes of the Storm"), true, "*portrait_static.dds", null)
            };
            Helper.BatchFileCopy(copyJobs);

            // Rename all the things
            Helper.BatchFileRename("Heroes of the Storm");

            // Scale all the things
            // Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
            List<ScalingJob> scalingJobs = new List<ScalingJob>
            {
                new ScalingJob(heroesPortrait, "*.dds")
            };
            Helper.BatchIMScale(scalingJobs);
        }
    }
}
