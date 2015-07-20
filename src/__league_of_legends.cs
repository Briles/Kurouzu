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
            string champions_square = @"League of Legends\Champions\Square\";
            string champions_portrait = @"League of Legends\Champions\Portrait\";
            string champions_landscape = @"League of Legends\Champions\Landscape\";
            string abilities = @"League of Legends\Abilities\";
            string items = @"League of Legends\Items\";
            string spells = @"League of Legends\Spells\";
            string masteries = @"League of Legends\Masteries\";
            string runes = @"League of Legends\Runes\";
            string[] dirs = { champions_square, champions_portrait, champions_landscape, abilities, items, spells, masteries, runes };
            Helper.BuildDirectoryTree(dirs);
            // Get the path of the source
            INIFile ini = new INIFile(Globals.Paths.Conf);
            string source_path = ini.INIReadValue("Game Paths", "League of Legends");
            // Get the source
            string[] swfs_to_get = {"ImagePack_spells.swf","ImagePack_masteryIcons.swf","ImagePack_items.swf"};
            foreach(string swf_to_get in Directory.GetFiles(source_path, "ImagePack_*.swf", SearchOption.AllDirectories).Where(f => swfs_to_get.Contains(Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)).ToList())
            {
                File.Copy(swf_to_get,Path.Combine(Globals.Paths.Assets,"Source","League of Legends",Path.GetFileName(swf_to_get)) , true);
                Console.WriteLine("Copying {0}", Path.GetFileName(swf_to_get));
            }
            // Extract the SWFs
            foreach(string swf in Directory.GetFiles(Path.Combine(Globals.Paths.Assets, "Source", "League of Legends"), "*.swf", SearchOption.AllDirectories).ToList())
            {
                string output_path = null;
                switch (Path.GetFileName(swf))
                {
                    case "ImagePack_items.swf":
                        output_path = items;
                        break;
                    case "ImagePack_spells.swf":
                        output_path = spells;
                        break;
                    case "ImagePack_masteryIcons.swf":
                        output_path = masteries;
                        break;
                    default:
                        break;
                }
                Helper.SWFExtract(swf, Path.Combine(Globals.Paths.Assets, output_path, "Source"));
            }
            // Copy the rest of the source assets
            // Copy jobs take the form { output path = string, { string start path, bool recursion flag, string search pattern, string exclude pattern } }
            string source_releases = @"RADS\projects\lol_air_client\releases";
            string source_version = Directory.GetDirectories(Path.Combine(source_path, source_releases))[0];
            string source_assets = Path.Combine(source_path, source_releases, source_version, @"deploy\assets");
            List<CopyJob> copyjobs = new List<CopyJob>
            {
                new CopyJob(champions_portrait, Path.Combine(source_assets, @"images\champions"), true, "*_0.jpg", "*_S*_*.jpg"),
                new CopyJob(champions_landscape, Path.Combine(source_assets, @"images\champions"), true, "*_Splash_0.jpg", null),
                new CopyJob(champions_square, Path.Combine(source_assets, @"images\champions"), true, "*_square_0.png", null),
                new CopyJob(abilities, Path.Combine(source_assets, @"images\abilities"), true, "*.png", null),
                new CopyJob(runes, Path.Combine(source_assets, @"storeImages\content\runes"), true, "*.png", null)
            };
            Helper.BatchFileCopy(copyjobs);
            // Rename all the things
            Helper.BatchFileRename("League of Legends");
            // Scale all the things
            // Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
            List<ScalingJob> scalingjobs = new List<ScalingJob>
            {
                new ScalingJob(champions_landscape, "*.jpg"),
                new ScalingJob(champions_portrait, "*.jpg"),
                new ScalingJob(champions_square, "*.png"),
                new ScalingJob(abilities, "*.png"),
                new ScalingJob(items, "*.png"),
                new ScalingJob(spells, "*.png"),
                new ScalingJob(masteries, "*.png"),
                new ScalingJob(runes, "*.png")
            };
            Helper.BatchIMScale(scalingjobs);
        }
    }
}
