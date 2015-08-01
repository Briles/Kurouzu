using Blazinix.INI;
using Kurouzu.Defaults;
using Kurouzu.Helpers;
using SharpVPK;
using System;
using System.Collections.Generic;
using System.IO;

namespace Kurouzu.Games
{
    public class Dota2
    {
        public static void Process()
        {
            const string HeroesMini = @"Dota 2\Heroes\Mini\";
            const string HeroesLandscape = @"Dota 2\Heroes\Landscape\";
            const string HeroesPortrait = @"Dota 2\Heroes\Portrait\";
            const string Items = @"Dota 2\Items\";
            const string Spells = @"Dota 2\Spells\";
            string[] Directories = { HeroesMini, HeroesLandscape, HeroesPortrait, Items, Spells };
            Helper.BuildDirectoryTree(Directories);

            // Get the path of the source
            INIFile INI = new INIFile(Globals.Paths.ConfigurationFile);
            string sourcePath = INI.INIReadValue("Game Paths", "Dota 2");

            string vpkPath = Path.Combine(sourcePath, @"dota\pak01_dir.vpk");
            const string archivePrepend = @"resource/flash3/images/{0}";

            // Get the source
            string[] ValvePackages = {"heroes", @"heroes\selection", "miniheroes", "spellicons", "items"};
            foreach(string ValvePackage in ValvePackages)
            {
                var valveArchive = new VpkArchive();
                valveArchive.Load(vpkPath);
                foreach (var directory in valveArchive.Directories)
                {
                    string rootDirectory = string.Format(archivePrepend, ValvePackage);
                    if (directory.ToString().Contains(rootDirectory)) {
                        foreach (var entry in directory.Entries)
                        {
                            string destPath = Path.Combine(Globals.Paths.Assets, @"Source\Dota 2", entry.ToString().Replace("resource/flash3/images/",""));
                            FileInfo destInfo = new FileInfo(destPath);
                            string destDirectory = destInfo.Directory.ToString();
                            if (!Directory.Exists(destDirectory))
                            {
                                Directory.CreateDirectory(destDirectory);
                                Console.WriteLine("Creating directory {0}", destDirectory);
                            }
                            if (Directory.Exists(destDirectory)) {
                                Console.WriteLine("Extracting {0}", destPath);
                                File.WriteAllBytes(destPath, entry.Data);
                            }
                        }
                    }
                }
            }

            // Copy the rest of the source assets
            // Copy jobs take the form { string output path, { string start path, bool recursion flag, string search pattern, string exclude pattern } }
            List<CopyJob> CopyJobs = new List<CopyJob>
            {
                new CopyJob(HeroesPortrait, Path.Combine(Globals.Paths.Assets, @"Source\Dota 2\heroes\selection"), true, "npc_dota_hero_*.png", null),
                new CopyJob(HeroesLandscape, Path.Combine(Globals.Paths.Assets, @"Source\Dota 2\heroes"), false, "*.png", null),
                new CopyJob(HeroesMini, Path.Combine(Globals.Paths.Assets, @"Source\Dota 2\miniheroes"), true, "*.png", null),
                new CopyJob(Spells, Path.Combine(Globals.Paths.Assets, @"Source\Dota 2\spellicons"), true, "*.png", null),
                new CopyJob(Items, Path.Combine(Globals.Paths.Assets, @"Source\Dota 2\items"), true, "*.png", null)
            };
            Helper.BatchFileCopy(CopyJobs);

            // Rename all the things
            Helper.BatchFileRename("Dota 2");

            // Scale all the things
            // Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
            List<ScalingJob> ScalingJobs = new List<ScalingJob>
            {
                new ScalingJob(HeroesLandscape, "*.png"),
                new ScalingJob(HeroesMini, "*.png"),
                new ScalingJob(HeroesPortrait, "*.png"),
                new ScalingJob(Items, "*.png"),
                new ScalingJob(Spells, "*.png")
            };
            Helper.BatchIMScale(ScalingJobs);
        }
    }
}
