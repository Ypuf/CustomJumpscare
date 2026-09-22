# Custom Jumpscare

Every second there is a chance of a full screen jumpscare. The chance, volume and number of frames can be changed in the mod settings

## Custom jumpscares

Put your files in the `custom` folder inside the mod (`Rain World/RainWorld_Data/StreamingAssets/mods/CustomJumpscare/custom/`). If the folder doesn't exist, it's created the first time the game starts with the mod enabled.

Frames of the jumpscare need to be stored as a .png stacked vertically (first frame at the top). A gif can be converted to this at https://ezgif.com/gif-to-sprite by choosing the vertical stacking option. Set "Sprite sheet frames" in the Remix settings to match the number of frames.

Frames and how to set up ezgif here:

![Visual of what to look for in ezgif](https://b.ypuf.xyz/u/best-scholarly-barnowl.png)

.mp3, .ogg and .wav files are the only sound files supported.

If there's more than one image or sound, the first one alphabetically is used. If either is missing, the built in one is used instead. New files are used when the game starts or when the mod's settings are applied.

## Steps to build

1. Download the source, either with `git clone https://github.com/Ypuf/CustomJumpscare.git` or as a ZIP from GitHub into `Rain World/RainWorld_Data/StreamingAssets/mods/`
2. Install the [.NET SDK](https://dotnet.microsoft.com/download) (version 9 or newer)
3. Make sure Rain World has been launched at least once with mods enabled so BepInEx has generated `HOOKS-Assembly-CSharp.dll`
4. Open a terminal in the `CustomJumpscare` folder and run:

   ```
   dotnet build -c Release
   ```

   If Rain World isn't installed at `C:\Program Files (x86)\Steam\steamapps\common\Rain World`, tell it where the game is:

   ```
   dotnet build -c Release -p:RainWorldDir="D:\SteamLibrary\steamapps\common\Rain World"
   ```

5. Copy `bin/Release/Jumpscare.dll` into the `plugins` folder, replacing the old one

## Credit

Full credit to damoonlord for the base code and assets: 

https://steamcommunity.com/sharedfiles/filedetails/?id=3497571857
