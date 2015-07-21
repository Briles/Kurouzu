using CommandLine;

namespace Kurouzu.Args
{
    class Options {
        [Option('g', "game", Required = true, HelpText = "The name of the game to be processed.")]
        public string InputGame { get; set; }

        [Option('m', "min", DefaultValue = false, Required = false, HelpText = "Minify the resulting images. May take a long time.")]
        public bool Minification { get; set; }

        [Option('s', "scale", DefaultValue = false, Required = false, HelpText = "Scale the image assets using ImageMagick. Output sizes are defined in the conf.ini. Scaling may take a long time depending on the input and output dimensions of each image.")]
        public bool Scale { get; set; }

        [Option('l', "log", DefaultValue = false, Required = false, HelpText = "Writes logging information to ./logs/. Useful if maintaining your own rename pairs.")]
        public bool Logging { get; set; }
    }
}
