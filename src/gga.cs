using System;
using CommandLine;
using System.Reflection;
//
using Kurouzu.Args;
using Kurouzu.Helpers;
using Kurouzu.Defaults;

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
            var result = CommandLine.Parser.Default.ParseArguments<Options>(args);
            var exitCode = result.Return(
                options =>
                {
                    if (options.InputGame != null)
                    {
                        Console.WriteLine("\nRunning in {0} from {1}\n", Globals.Paths.Work, Globals.Paths.Home);
                        string proper = Game.GetGamebyProp(options.InputGame.Trim()).Title;
                        string game = (proper).Replace(" ", "");
                        Helper.ValidateINI(proper);
                        // Helper.PreCleanup(proper);
                        Console.WriteLine("Processing {0}", proper);
                        // Helper.BuildSourceDirectory(proper);
                        // StartProcess(game);
                        // Helper.PostCleanup(proper);
                        if (options.Minification == true)
                        {
                            // Minify
                            // Helper.MinifyPNG();
                        }
                    }
                    return 0;
                },
                errors =>
                {
                    return 1;
                });
        }
    }
}
