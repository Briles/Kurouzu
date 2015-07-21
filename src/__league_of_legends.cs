using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
//
using Blazinix.INI;
using Kurouzu.Helpers;
using Kurouzu.Defaults;

namespace Kurouzu.Games
{
    public class LeagueofLegends
    {
        public static void Process()
        {
            string ChampionsSquare = @"League of Legends\Champions\Square\";
            string ChampionsPortrait = @"League of Legends\Champions\Portrait\";
            string ChampionsLandscape = @"League of Legends\Champions\Landscape\";
            string Abilities = @"League of Legends\Abilities\";
            string Items = @"League of Legends\Items\";
            string Spells = @"League of Legends\Spells\";
            string Masteries = @"League of Legends\Masteries\";
            string Runes = @"League of Legends\Runes\";
            string[] Directories = { ChampionsSquare, ChampionsPortrait, ChampionsLandscape, Abilities, Items, Spells, Masteries, Runes };
            Helper.BuildDirectoryTree(Directories);
            // Get the path of the source
            INIFile INI = new INIFile(Globals.Paths.ConfigurationFile);
            string SourcePath = INI.INIReadValue("Game Paths", "League of Legends");
            // Get the source
            string[] NeededSWFs = {"ImagePack_spells.swf","ImagePack_masteryIcons.swf","ImagePack_items.swf"};
            foreach(string NeededSWF in Directory.GetFiles(SourcePath, "ImagePack_*.swf", SearchOption.AllDirectories).Where(f => NeededSWFs.Contains(Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)).ToList())
            {
                File.Copy(NeededSWF, Path.Combine(Globals.Paths.Assets,"Source","League of Legends",Path.GetFileName(NeededSWF)) , true);
                Console.WriteLine("Copying {0}", Path.GetFileName(NeededSWF));
            }
            // Extract the SWFs
            foreach(string swf in Directory.GetFiles(Path.Combine(Globals.Paths.Assets, "Source", "League of Legends"), "*.swf", SearchOption.AllDirectories).ToList())
            {
                string OutputPath = null;
                switch (Path.GetFileName(swf))
                {
                    case "ImagePack_items.swf":
                        OutputPath = Items;
                        break;
                    case "ImagePack_spells.swf":
                        OutputPath = Spells;
                        break;
                    case "ImagePack_masteryIcons.swf":
                        OutputPath = Masteries;
                        break;
                    default:
                        break;
                }
                Helper.SWFExtract(swf, Path.Combine(Globals.Paths.Assets, OutputPath, "Source"));
            }
            // Copy the rest of the source assets
            // Copy jobs take the form { output path = string, { string start path, bool recursion flag, string search pattern, string exclude pattern } }
            string SourceReleases = @"RADS\projects\lol_air_client\releases";
            string SourceVersion = Directory.GetDirectories(Path.Combine(SourcePath, SourceReleases))[0];
            string SourceAssets = Path.Combine(SourcePath, SourceReleases, SourceVersion, @"deploy\assets");
            List<CopyJob> CopyJobs = new List<CopyJob>
            {
                new CopyJob(ChampionsPortrait, Path.Combine(SourceAssets, @"images\champions"), true, "*_0.jpg", "*_S*_*.jpg"),
                new CopyJob(ChampionsLandscape, Path.Combine(SourceAssets, @"images\champions"), true, "*_Splash_0.jpg", null),
                new CopyJob(ChampionsSquare, Path.Combine(SourceAssets, @"images\champions"), true, "*_square_0.png", null),
                new CopyJob(Abilities, Path.Combine(SourceAssets, @"images\abilities"), true, "*.png", null),
                new CopyJob(Runes, Path.Combine(SourceAssets, @"storeImages\content\runes"), true, "*.png", null)
            };
            Helper.BatchFileCopy(CopyJobs);
            // Rename all the things
            Helper.BatchFileRename("League of Legends");
            // Scale all the things
            // Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
            List<ScalingJob> ScalingJobs = new List<ScalingJob>
            {
                new ScalingJob(ChampionsLandscape, "*.jpg"),
                new ScalingJob(ChampionsPortrait, "*.jpg"),
                new ScalingJob(ChampionsSquare, "*.png"),
                new ScalingJob(Abilities, "*.png"),
                new ScalingJob(Items, "*.png"),
                new ScalingJob(Spells, "*.png"),
                new ScalingJob(Masteries, "*.png"),
                new ScalingJob(Runes, "*.png")
            };
            Helper.BatchIMScale(ScalingJobs);
        }
    }
}
