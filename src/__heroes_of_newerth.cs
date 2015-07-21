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
            const string Heroes = @"Heroes of Newerth\Heroes\";
            const string Abilities = @"Heroes of Newerth\Abilities\";
            const string Items = @"Heroes of Newerth\Items\";
            string[] Directories = { Heroes, Abilities, Items };
            Helper.BuildDirectoryTree(Directories);

            // Get the path of the source
            INIFile INI = new INIFile(Globals.Paths.ConfigurationFile);
            string SourcePath = INI.INIReadValue("Game Paths", "Heroes of Newerth");

            // Get the source
            string ResourcePath = Path.Combine(SourcePath, @"game\textures.s2z");
            string ExtractPath = Path.Combine(Globals.Paths.Assets, "Source", "Heroes of Newerth");

            Regex[] Filters = {
                new Regex(@"00000000/items/[\w]*/icon.*.dds"),
                new Regex(@"00000000/heroes/[\w]*/hero.*.dds"),
                new Regex(@"00000000/heroes/[\w]*/ability.*icon.*.dds"),
                new Regex(@"00000000/heroes/[\w]*/icon.*.dds")
            };

            ZipFile zf = null;
            try
            {
                FileStream fs = File.OpenRead(ResourcePath);
                zf = new ZipFile(fs);

                foreach (ZipEntry zipEntry in zf)
                {
                    string entryFileName = zipEntry.Name;
                    foreach (var Filter in Filters)
                    {
                        if (Filter.IsMatch(entryFileName))
                        {
                            Console.WriteLine("Extracting {0}", entryFileName);
                            byte[] buffer = new byte[4096];
                            Stream zipStream = zf.GetInputStream(zipEntry);

                            string fullZipToPath = Path.Combine(ExtractPath, entryFileName);
                            string directoryName = Path.GetDirectoryName(fullZipToPath);
                            if (directoryName.Length > 0)
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
            foreach (string TextureFile in Directory.GetFiles(Path.Combine(Globals.Paths.Assets, "Source", "Heroes of Newerth"), "*.dds", SearchOption.AllDirectories).ToList())
            {
                int Counter = 0;
                List<string> PathLeaves = new List<string>();

                string[] Leaves = Path.GetDirectoryName(TextureFile).Split('\\');
                Array.Reverse(Leaves);

                do
                {
                    Counter++;
                    string Leaf = Leaves[Counter - 1];
                    PathLeaves.Add(Leaf);
                } while (!PathLeaves.Contains("heroes") && !PathLeaves.Contains("items"));

                PathLeaves = PathLeaves.Where(item => !item.Contains("heroes") && !item.Contains("items") && !item.Contains("icons") && !item.Contains("icon")).ToList();
                PathLeaves.Reverse();

                string LeafString = string.Join("_", PathLeaves.ToArray()).Replace(' ','_').ToLower();
                string NewName = LeafString + Path.GetExtension(TextureFile);

                if (!string.Equals(Path.GetFileNameWithoutExtension(TextureFile), "icon", StringComparison.OrdinalIgnoreCase))
                {
                    NewName = LeafString + '_' + Path.GetFileName(TextureFile);
                }

                Console.WriteLine("Renaming {0} to {1}", TextureFile, NewName);
                File.Move(TextureFile, Path.Combine(Path.GetDirectoryName(TextureFile), NewName));
            }

            // Copy the rest of the source assets
            // Copy jobs take the form { output path = string, { string start path, bool recursion flag, string search pattern, string exclude pattern } }
            //List<CopyJob> CopyJobs = new List<CopyJob>
            //{
            //    new CopyJob(Abilities, Path.Combine(SourceAssets, @"00000000\heroes\"), true, "*ability*.dds", null),
            //    new CopyJob(Heroes, Path.Combine(SourceAssets, @"00000000\heroes\*\*\"), false, "*.dds", "*ability*.dds"),
            //    new CopyJob(Items, Path.Combine(SourceAssets, @"00000000\items\"), true, "*.dds", null)
            //};
            //Helper.BatchFileCopy(CopyJobs);

            //// Rename all the things
            //Helper.BatchFileRename("Heroes of Newerth");

            //// Scale all the things
            //// Scaling jobs take the form { string start path, string search pattern, string exclude pattern }
            //List<ScalingJob> ScalingJobs = new List<ScalingJob>
            //{
            //    new ScalingJob(Heroes, "*.dds"),
            //    new ScalingJob(Abilities, "*.dds"),
            //    new ScalingJob(Items, "*.dds")
            //};
            //Helper.BatchIMScale(ScalingJobs);
        }
    }
}
