using Blazinix.INI;
using Kurouzu.Defaults;
using Kurouzu.Helpers;
using Kurouzu.SWF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kurouzu.Games
{
    public class LeagueofLegends
    {
        public static void Process()
        {
            const string championsSquare = @"League of Legends\Champions\Square\";
            const string championsPortrait = @"League of Legends\Champions\Portrait\";
            const string championsLandscape = @"League of Legends\Champions\Landscape\";
            const string abilities = @"League of Legends\Abilities\";
            const string items = @"League of Legends\Items\";
            const string spells = @"League of Legends\Spells\";
            const string masteries = @"League of Legends\Masteries\";
            const string runes = @"League of Legends\Runes\";
            const string wards = @"League of Legends\Wards\";
            string[] directories =
            {
                championsSquare, championsPortrait, championsLandscape, abilities, items, spells,
                masteries, runes, wards
            };
            Helper.BuildDirectoryTree(directories);

            // Get the path of the source
            var ini = new INIFile(Globals.Paths.ConfigurationFile);
            var sourcePath = ini.INIReadValue("Game Paths", "League of Legends");

            // Get the source
            string[] neededSwFs = {"ImagePack_spells.swf", "ImagePack_masteryIcons.swf", "ImagePack_items.swf"};
            foreach (
                var neededSwf in
                    Directory.GetFiles(sourcePath, "ImagePack_*.swf", SearchOption.AllDirectories)
                        .Where(f => neededSwFs.Contains(Path.GetFileName(f), StringComparer.OrdinalIgnoreCase))
                        .ToList())
            {
                File.Copy(neededSwf,
                    Path.Combine(Globals.Paths.Assets, "Source", "League of Legends", Path.GetFileName(neededSwf)), true);
                Console.WriteLine("Copying {0}", neededSwf);
            }

            // Extract the SWFs
            foreach (
                var swfFile in
                    Directory.GetFiles(Path.Combine(Globals.Paths.Assets, "Source", "League of Legends"), "*.swf",
                        SearchOption.AllDirectories).ToList())
            {
                string outputPath = null;
                switch (Path.GetFileName(swfFile))
                {
                    case "ImagePack_items.swf":
                        outputPath = items;
                        break;
                    case "ImagePack_spells.swf":
                        outputPath = spells;
                        break;
                    case "ImagePack_masteryIcons.swf":
                        outputPath = masteries;
                        break;
                    default:
                        break;
                }

                var swf = new SwfFile(swfFile);
                if (outputPath != null) swf.ExtractImages(Path.Combine(Globals.Paths.Assets, outputPath, "Source"));
            }

            // Copy the rest of the source assets
            // Copy jobs take the form { output path = string, { string start path, bool recursion flag, string search pattern, string exclude pattern } }
            const string sourceReleases = @"RADS\projects\lol_air_client\releases";
            var sourceVersion = Directory.GetDirectories(Path.Combine(sourcePath, sourceReleases))[0];
            var sourceAssets = Path.Combine(sourcePath, sourceReleases, sourceVersion, @"deploy\assets");

            var copyJobs = new List<CopyJob>
            {
                new CopyJob(championsPortrait, Path.Combine(sourceAssets, @"images\champions"), false, "*_0.jpg",
                    "*_S*_*.jpg"),
                new CopyJob(championsLandscape, Path.Combine(sourceAssets, @"images\champions"), false, "*_Splash_0.jpg",
                    null),
                new CopyJob(championsSquare, Path.Combine(sourceAssets, @"images\champions"), false, "*_Square_0.png",
                    null),
                new CopyJob(abilities, Path.Combine(sourceAssets, @"images\abilities"), false, "*.png", null),
                new CopyJob(runes, Path.Combine(sourceAssets, @"images\runes"), true, "*.png", null),
                new CopyJob(wards, Path.Combine(sourceAssets, @"images\misc\wards"), false, "wardImage_*.png", null)
            };
            Helper.BatchFileCopy(copyJobs);

            // Rename all the things
            Helper.BatchFileRename("League of Legends");

            // Scale all the things
            // Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
            var scalingJobs = new List<ScalingJob>
            {
                new ScalingJob(championsLandscape, "*.jpg"),
                new ScalingJob(championsPortrait, "*.jpg"),
                new ScalingJob(championsSquare, "*.png"),
                new ScalingJob(abilities, "*.png"),
                new ScalingJob(items, "*.png"),
                new ScalingJob(spells, "*.png"),
                new ScalingJob(masteries, "*.png"),
                new ScalingJob(runes, "*.png"),
                new ScalingJob(wards, "*.png")
            };
            Helper.BatchIMScale(scalingJobs);
        }
    }
}