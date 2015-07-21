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
    public class HeroesoftheStorm
    {
        public static void Process()
        {
            const string HeroesPortrait = @"Heroes of the Storm\Heroes\Portrait\";
            const string HeroesLandscape = @"Heroes of the Storm\Heroes\Portrait\";
            const string HeroesRound = @"Heroes of the Storm\Heroes\Round\";
            const string HeroesBanner = @"Heroes of the Storm\Heroes\Banner\";
            const string Talents = @"Heroes of the Storm\Talents\";
            const string UI = @"Heroes of the Storm\UI\";
            string[] Directories = { HeroesPortrait, HeroesLandscape, HeroesRound, HeroesBanner, Talents, UI };
            Helper.BuildDirectoryTree(Directories);
            // Get the path of the source
            INIFile INI = new INIFile(Globals.Paths.ConfigurationFile);
            string SourcePath = INI.INIReadValue("Game Paths", "Heroes of the Storm");
            // Get the source
            // string[] filters = { "*portrait*static.dds", "*-unit-*.dds", "*-building-*.dds", "*-ability-*.dds", "*-armor-*.dds", "*-upgrade-*.dds", "*icon-*nobg.dds" };
            string GameData = Path.Combine(SourcePath, @"HeroesData");
            string DestinationPath = Path.Combine(Globals.Paths.Assets, "Source", "Heroes of the Storm");
            var CascView = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "CascView.exe",
                    Arguments = string.Format(" \"{0}\" \"*.dds\" \"{1}\" /fp", GameData, DestinationPath),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            CascView.Start();
            while (!CascView.StandardOutput.EndOfStream)
            {
                string StandardOutputLine = CascView.StandardOutput.ReadLine();
                Console.WriteLine(StandardOutputLine);
            }
            // Copy the rest of the source assets
            // Copy jobs take the form { string output path, { string start path, bool recursion flag, string search pattern, string exclude pattern } }
            List<CopyJob> CopyJobs = new List<CopyJob>
            {
                new CopyJob(HeroesPortrait, Path.Combine(Globals.Paths.Assets, @"Source\Heroes of the Storm"), true, "*portrait_static.dds", null)
            };
            Helper.BatchFileCopy(CopyJobs);
            // Rename all the things
            Helper.BatchFileRename("Heroes of the Storm");
            // Scale all the things
            // Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
            List<ScalingJob> ScalingJobs = new List<ScalingJob>
            {
                new ScalingJob(HeroesPortrait, "*.dds")
            };
            Helper.BatchIMScale(ScalingJobs);
        }
    }
}
