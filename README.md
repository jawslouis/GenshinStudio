## Note: This program is no longer being maintained. Unfortunately, it does not work with the latest version of the game.

# TeapotStudio
Extract 3D models and textures for use in the [MakePlace: Teapot](https://jawslouis.itch.io/teapot) app. The version distributed with the app requires the .NET Desktop Runtime 6.0. Variants depending on other versions of .NET can be downloaded [here](https://github.com/jawslouis/TeapotStudio/releases).

Based on [AssetStudio](https://github.com/Perfare/AssetStudio) and the [GenshinStudio](https://github.com/Razmoth/GenshinStudio) fork.

## Requirements

- AssetStudio.net472
   - [.NET Framework 4.7.2](https://dotnet.microsoft.com/download/dotnet-framework/net472)
- AssetStudio.net5
   - [.NET Desktop Runtime 5.0](https://dotnet.microsoft.com/download/dotnet/5.0)
- AssetStudio.net6
   - [.NET Desktop Runtime 6.0](https://dotnet.microsoft.com/download/dotnet/6.0)

## Usage

1. Build the BLK map (Misc -> Build BLKMap)
   - Select the GenshinImpact_Data folder when prompted (e.g. `C:\Program Files\Genshin Impact\Genshin Impact game\GenshinImpact_Data`)
   - This will take around 3 min

2. Load the BLKs (Teapot -> Load BLKs)
   - A sequence of actions will take place. Wait for the status at the bottom of the screen to read `Finished loading <x> files with <y> exportable assets`

3. Extract the files (Teapot -> Export files)
   - Be patient. This takes another 5 min.

Done!

## FAQ

### Help! I can't get the tool to work.
Post on the [discord channel](https://discord.gg/YuvcPzCuhq) for help with troubleshooting.

### Why are you not using the latest version of GenshinStudio?
The latest v0.16.50 is not as robust in detecting mesh and texture files - about 30% of files are missing compared to using v0.16.30.

## Donate
Thank you for using this tool. If you enjoy my work and wish to support me, you can use the below links:

Ko-fi: [https://ko-fi.com/jawslouis](https://ko-fi.com/jawslouis)

Patreon: [https://www.patreon.com/jawslouis](https://www.patreon.com/jawslouis)

