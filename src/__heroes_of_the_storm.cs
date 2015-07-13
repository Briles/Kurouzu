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
    public class HeroesoftheStorm
    {
        public static void Process()
        {
            string units_portrait = @"Heroes of the Storm\Heroes\Portrait\";
            string units_landscape = @"Heroes of the Storm\Heroes\Portrait\";
            string units_round = @"Heroes of the Storm\Heroes\Round\";
            string units_banner = @"Heroes of the Storm\Heroes\Banner\";
            string talents = @"Heroes of the Storm\Talents\";
            string ui = @"Heroes of the Storm\UI\";
            string[] dirs = { units_portrait, units_landscape, units_round, units_banner, talents, ui };
            Helper.BuildDirectoryTree(dirs);
            // Get the path of the source
            INIFile ini = new INIFile(Globals.Paths.Conf);
            string source_path = ini.INIReadValue("Game Paths", "Heroes of the Storm");
            // Get the source
            // string[] filters = { "*portrait*static.dds", "*-unit-*.dds", "*-building-*.dds", "*-ability-*.dds", "*-armor-*.dds", "*-upgrade-*.dds", "*icon-*nobg.dds" };
            string hotsdata = Path.Combine(source_path, @"HeroesData");
            string dest = Path.Combine(Globals.Paths.Assets, "Source", "Heroes of the Storm");
            var cascview = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "CascView.exe",
                    Arguments = String.Format(" \"{0}\" \"*.dds\" \"{1}\" /fp", hotsdata, dest),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            cascview.Start();
            while (!cascview.StandardOutput.EndOfStream)
            {
                string line = cascview.StandardOutput.ReadLine();
                Console.WriteLine(line);
            }
            // Copy the rest of the source assets
            // Copy jobs take the form { string output path, { string start path, bool recursion flag, string search pattern, string exclude pattern } }
            List<CopyJob> copyjobs = new List<CopyJob>
            {
                new CopyJob(units_portrait, Path.Combine(Globals.Paths.Assets, @"Source\Heroes of the Storm"), true, "*portrait_static.dds", null)
            };
            Helper.BatchFileCopy(copyjobs);
            // Rename all the things
            Helper.BatchFileRename("Heroes of the Storm");
            // Scale all the things
            // Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
            List<ScalingJob> scalingjobs = new List<ScalingJob>
            {
                new ScalingJob(units_portrait, "*.dds")
            };
            // Helper.BatchIMScale(scalingjobs);
        }
    }
}
