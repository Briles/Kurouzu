using CommandLine;
using Kurouzu.Args;
using Kurouzu.Defaults;
using Kurouzu.Helpers;
using System;
using System.Reflection;

namespace Kurouzu.Main
{

    class GetGameAssets
    {

        static void StartProcess(string className)
        {
            Type t = Type.GetType("Kurouzu.Games." + className);
            MethodInfo m = t.GetMethod("Process");
            m.Invoke(null, null);
        }

        static void Main(string[] args)
        {
            var options = new Options();
            if (Parser.Default.ParseArgumentsStrict(args, options))
            {
                Console.WriteLine("\nRunning in {0} from {1}\n", Globals.Paths.Work, Globals.Paths.Home);
                string proper = Game.GetGamebyProp(options.InputGame.Trim()).Title;
                string game = (proper).Replace(" ", "");
                Helper.ValidateINI(proper);
                Helper.PreCleanup(proper);
                Console.WriteLine("Processing {0}", proper);
                Helper.BuildSourceDirectory(proper);
                StartProcess(game);
                if (options.Debugging == false)
                {
                    Helper.PostCleanup(proper);
                }
                if (options.Minification == true)
                {
                    // Minify
                    Helper.MinifyPNG();
                }
            }
        }
    }
}
