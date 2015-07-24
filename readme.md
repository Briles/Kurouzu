![Kurouzu Logo](Graphics/Logo/logo.png "Kurouzu Logo")

Kurouzu is a Windows Command Line utility for extracting, renaming, and scaling image assets from popular eSports Games.

* * *
### Supported Games

* [Counter-Strike: Global Offensive (planned)](http://www.counter-strike.net/ "Counter-Strike: Global Offensive")
* Dawngate (Legacy support, No longer developed)
* [Dota 2](http://www.dota2.com/ "Dota 2")
* [Hearthstone (planned)](http://www.battle.net/hearthstone/ "Hearthstone")
* [Heroes of Newerth (in progress)](http://www.heroesofnewerth.com/?home "Heroes of Newerth")
* [Heroes of the Storm (planned)](http://www.battle.net/heroes/ "Heroes of the Storm")
* [League of Legends](http://www.leagueoflegends.com/ "League of Legends")
* [Smite](http://www.hirezstudios.com/smite "Smite")
* [StarCraft II](http://www.battle.net/sc2/ "StarCraft II")
* [Strife (planned)](https://strife.com/ "Strife")
* [Unreal Tournament 4 (planned)](https://www.unrealtournament.com/ "Unreal Tournament 4")

### Dependencies
The following need to be on your path for Kurouzu to work.

For Scaling (optional):
* [ImageMagick 7+](http://www.imagemagick.org/download/binaries/ "ImageMagick") `identify.exe & magick.exe`

For minification (optional):
* [PNGOUT](http://advsys.net/ken/utils.htm "PNGOUT") `pngout.exe`
* [DeflOpt](https://chocolatey.org/packages/DeflOpt "DeflOpt") `deflopt.exe`
* [TruePNG](http://x128.ho.ua/pngutils.html "TruePNG") `truepng.exe`

Kurouzu only requires dependencies for the games you want to process.

| Game              | Dependency                                                                                                       |
|:-----------------:|:----------------------------------------------------------------------------------------------------------------:|
| Dawngate          | [QuickBMS](http://aluigi.altervista.org/quickbms.htm/ "QuickBMS") `quickbms_4gb_files.exe`                       |
| Dota 2            | [HLExtract](http://nemesis.thewavelength.net/index.php?p=35 "HLExtract") `hlextract.exe & SDL2.dll (x64 or x86)` |
| Smite             | [Umodel](http://www.gildor.org/en/projects/umodel "Umodel") `umodel.exe`                                         |
| StarCraft II      | [MPQ Editor](http://www.zezula.net/en/mpq/download.html "MPQ Editor") `MPQEditor.exe`                            |

### Usage

Run kurouzu without any arguments to view the help info.

| Flag (shorthand) |   Type  |                                                                                          Description                                                                                         |
|:----------------:|:-------:|:--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------:|
|     game (g)     |  string |                                                          The name of the game to be processed. eg: -g "League of Legends". Required.                                                         |
|     scale (s)    | boolean | Scale the image assets using ImageMagick. Output sizes are defined in the conf.ini. Scaling may take a long time depending on the input and output dimensions of each image. Off by default. |
|      log (l)     | boolean |                                              Writes logging information to ./logs/. Useful if maintaining your own rename pairs. Off by default.                                             |
|      min (m)     | boolean |                                                              Minify the resulting images. May take a long time. Off by default.                                                              |
|     debug (d)    | boolean |                                                                Prevents Kurouzu from cleaning up source files. Off by default.                                                               |

#### Examples
`kurouzu.exe -g "League of Legends" -s -l -m` will extract all League of Legends assets, rename them, scale them, minify them and log the results.

### [Contributing](https://github.com/Briles/Kurouzu/wiki/Contributing)

For contributing guidelines and instructions, please see the [wiki page](https://github.com/Briles/Kurouzu/wiki/Contributing).

### Todo

* Reduce the number of external dependencies
