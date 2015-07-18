using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
//
using Blazinix.INI;
using SpawnedIn.GGA.Games;
using SpawnedIn.GGA.Helpers;
using SpawnedIn.GGA.Defaults;

namespace SpawnedIn.GGA.Main
{

    class GetGameAssets
    {

        static void StartProcess(string classname)
        {
            Type t = Type.GetType("SpawnedIn.GGA.Games." + classname);
            MethodInfo m = t.GetMethod("Process");
            m.Invoke(null, null);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("\nRunning in {0} from {1}\n", Globals.Paths.Work, Globals.Paths.Home);
            Helper.VerifyINI();
            string[] games;
            if (args.Length > 0)
            {
                games = args[0].Split(',');
            }
            else
            {
                games = Globals.Games.Select(x => x.Title).ToArray();
            }
            foreach (string input_game in games)
            {
                string proper = Game.GetGamebyProp(input_game.Trim()).Title;
                string game = (proper).Replace(" ", "");
                Helper.PreCleanup(proper);
                Console.WriteLine("Processing {0}", proper);
                Helper.BuildSourceDirectory(proper);
                StartProcess(game);
                Helper.PostCleanup(proper);
            }
            if (args.Contains("-min", StringComparer.OrdinalIgnoreCase))
            {
                // Minify
                Helper.MinifyPNG();
            }
        }
    }
}
