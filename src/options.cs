using CommandLine;

namespace Kurouzu.Args
{
    class Options {
    [Option('g', "game", Required = true, HelpText = "The name of the game to be processed.")]
    public string InputGame { get; set; }

    [Option('m', "min", Required = false, HelpText = "Minify the resulting images. May take a long time.")]
    public bool Minification { get; set; }
  }
}
