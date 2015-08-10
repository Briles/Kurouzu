using System;
using System.Reflection;
using CommandLine;
using Kurouzu.Args;
using Kurouzu.Defaults;
using Kurouzu.Helpers;

namespace Kurouzu
{

    class GetGameAssets
    {

        static void StartGameProcess(string className)
        {
            var t = Type.GetType("Kurouzu.Games." + className);
            if (t == null) throw new ArgumentNullException(nameof(t));
            MethodInfo m = t.GetMethod("Process");
            m.Invoke(null, null);
        }

        static void Main(string[] args)
        {
            var options = new Options();
            if (Parser.Default.ParseArgumentsStrict(args, options))
            {
                Console.WriteLine("\nRunning in {0} from {1}\n", Globals.Paths.Work, Globals.Paths.Home);

                string proper = GameInfo.GetGamebyProp(options.InputGame.Trim()).Title;
                string game = (proper).Replace(" ", "");

                Console.WriteLine("Processing {0}", proper);

                // Check the INI for configuration values
                Helper.ValidateINI(proper);

                // CLeanup files and directories from previous runs
                Helper.PreCleanup(proper);

                // Create the required directory structure 
                Helper.BuildSourceDirectory(proper);

                StartGameProcess(game);

                // Cleanup files and directories after run
                if (!options.Debugging)
                {
                    Helper.PostCleanup(proper);
                }

                // Minify the resulting PNG assets
                if (options.Minification)
                {
                    Helper.MinifyPNG();
                }
            }
        }
    }
}
