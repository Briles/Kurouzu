using Blazinix.INI;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Kurouzu.Defaults;
using Kurouzu.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kurouzu.Games
{
    public class HeroesofNewerth
    {
        public static void Process()
        {
            const string heroes = @"Heroes of Newerth\Heroes\";
            const string abilities = @"Heroes of Newerth\Abilities\";
            const string items = @"Heroes of Newerth\Items\";
            string[] directories = { heroes, abilities, items };
            Helper.BuildDirectoryTree(directories);

            // Get the path of the source
            INIFile ini = new INIFile(Globals.Paths.ConfigurationFile);
            string sourcePath = ini.INIReadValue("Game Paths", "Heroes of Newerth");

            // Get the source
            string resourcePath = Path.Combine(sourcePath, @"game\textures.s2z");
            string extractPath = Path.Combine(Globals.Paths.Assets, "Source", "Heroes of Newerth");

            Regex[] filters = {
                new Regex(@"00000000/items/[\w/]*icon*.dds"),
                new Regex(@"00000000/heroes/[\w]*/hero.*.dds"),
                new Regex(@"00000000/heroes/[\w]*/ability.*icon.*.dds"),
                new Regex(@"00000000/heroes/[\w]*/icon.*.dds")
            };

            ZipFile zf = null;
            try
            {
                FileStream fs = File.OpenRead(resourcePath);
                zf = new ZipFile(fs);

                foreach (ZipEntry zipEntry in zf)
                {
                    string entryFileName = zipEntry.Name;
                    foreach (var filter in filters)
                    {
                        if (filter.IsMatch(entryFileName))
                        {
                            Console.WriteLine("Extracting {0}", entryFileName);
                            byte[] buffer = new byte[4096];
                            Stream zipStream = zf.GetInputStream(zipEntry);

                            string fullZipToPath = Path.Combine(extractPath, entryFileName);
                            string directoryName = Path.GetDirectoryName(fullZipToPath);
                            if (!string.IsNullOrEmpty(directoryName))
                            {
                                Directory.CreateDirectory(directoryName);
                            }

                            using (FileStream streamWriter = File.Create(fullZipToPath))
                            {
                                StreamUtils.Copy(zipStream, streamWriter, buffer);
                            }
                        }
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true;
                    zf.Close();
                }
            }

            //
            // Make the filenames somewhat sane
            foreach (string textureFile in Directory.GetFiles(Path.Combine(Globals.Paths.Assets, "Source", "Heroes of Newerth"), "*.dds", SearchOption.AllDirectories).ToList())
            {
                int counter = 0;
                List<string> pathLeaves = new List<string>();

                var directoryName = Path.GetDirectoryName(textureFile);
                if (directoryName != null)
                {
                    string[] leaves = directoryName.Split('\\');
                    Array.Reverse(leaves);

                    do
                    {
                        counter++;
                        string leaf = leaves[counter - 1];
                        pathLeaves.Add(leaf);
                    } while (!pathLeaves.Contains("heroes") && !pathLeaves.Contains("items"));
                }

                pathLeaves = pathLeaves.Where(item => !item.Contains("heroes") && !item.Contains("items") && !item.Contains("icons") && !item.Contains("icon")).ToList();
                pathLeaves.Reverse();

                string leafString = string.Join("_", pathLeaves.ToArray()).Replace(' ','_').ToLower();
                string newName = leafString + Path.GetExtension(textureFile);

                if (!string.Equals(Path.GetFileNameWithoutExtension(textureFile), "icon", StringComparison.OrdinalIgnoreCase))
                {
                    newName = leafString + '_' + Path.GetFileName(textureFile);
                }

                Console.WriteLine("Renaming {0} to {1}", textureFile, newName);
                File.Move(textureFile, Path.Combine(Path.GetDirectoryName(textureFile), newName));
            }

            // Copy the rest of the source assets
            // Copy jobs take the form { output path = string, { string start path, bool recursion flag, string search pattern, string exclude pattern } }
            string destination = Path.Combine(Globals.Paths.Assets, "Source" ,"Heroes of Newerth");
            List<CopyJob> copyJobs = new List<CopyJob>
            {
                new CopyJob(abilities, Path.Combine(destination, @"00000000\heroes\"), true, "*ability*.dds", null),
                new CopyJob(heroes, Path.Combine(destination, @"00000000\heroes\"), true, "*.dds", "*ability*.dds"),
                new CopyJob(items, Path.Combine(destination, @"00000000\items\"), true, "*.dds", null)
            };
            Helper.BatchFileCopy(copyJobs);

            // Rename all the things
            Helper.BatchFileRename("Heroes of Newerth");

            // Scale all the things
            // Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
            List<ScalingJob> scalingJobs = new List<ScalingJob>
            {
                new ScalingJob(heroes, "*.dds"),
                new ScalingJob(abilities, "*.dds"),
                new ScalingJob(items, "*.dds")
            };
            Helper.BatchIMScale(scalingJobs);
        }
    }
}
