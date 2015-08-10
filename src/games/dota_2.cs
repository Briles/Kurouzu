using System;
using System.Collections.Generic;
using System.IO;
using Blazinix.INI;
using Kurouzu.Defaults;
using Kurouzu.Helpers;
using SharpVPK;

namespace Kurouzu.Games
{
    public class Dota2
    {
        public static void Process()
        {
            const string heroesMini = @"Dota 2\Heroes\Mini\";
            const string heroesLandscape = @"Dota 2\Heroes\Landscape\";
            const string heroesPortrait = @"Dota 2\Heroes\Portrait\";
            const string items = @"Dota 2\Items\";
            const string spells = @"Dota 2\Spells\";
            string[] directories = { heroesMini, heroesLandscape, heroesPortrait, items, spells };
            Helper.BuildDirectoryTree(directories);

            // Get the path of the source
            INIFile ini = new INIFile(Globals.Paths.ConfigurationFile);
            string sourcePath = ini.INIReadValue("Game Paths", "Dota 2");

            string vpkPath = Path.Combine(sourcePath, @"dota\pak01_dir.vpk");
            const string archivePrepend = @"resource/flash3/images/{0}";

            // Get the source
            string[] valvePackages = {"heroes", @"heroes\selection", "miniheroes", "spellicons", "items"};
            foreach(string valvePackage in valvePackages)
            {
                var valveArchive = new VpkArchive();
                valveArchive.Load(vpkPath);
                foreach (var directory in valveArchive.Directories)
                {
                    string rootDirectory = string.Format(archivePrepend, valvePackage);
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
            List<CopyJob> copyJobs = new List<CopyJob>
            {
                new CopyJob(heroesPortrait, Path.Combine(Globals.Paths.Assets, @"Source\Dota 2\heroes\selection"), true, "npc_dota_hero_*.png", null),
                new CopyJob(heroesLandscape, Path.Combine(Globals.Paths.Assets, @"Source\Dota 2\heroes"), false, "*.png", null),
                new CopyJob(heroesMini, Path.Combine(Globals.Paths.Assets, @"Source\Dota 2\miniheroes"), true, "*.png", null),
                new CopyJob(spells, Path.Combine(Globals.Paths.Assets, @"Source\Dota 2\spellicons"), true, "*.png", null),
                new CopyJob(items, Path.Combine(Globals.Paths.Assets, @"Source\Dota 2\items"), true, "*.png", null)
            };
            Helper.BatchFileCopy(copyJobs);

            // Rename all the things
            Helper.BatchFileRename("Dota 2");

            // Scale all the things
            // Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
            List<ScalingJob> scalingJobs = new List<ScalingJob>
            {
                new ScalingJob(heroesLandscape, "*.png"),
                new ScalingJob(heroesMini, "*.png"),
                new ScalingJob(heroesPortrait, "*.png"),
                new ScalingJob(items, "*.png"),
                new ScalingJob(spells, "*.png")
            };
            Helper.BatchIMScale(scalingJobs);
        }
    }
}
