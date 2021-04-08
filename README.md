<p align="center">
  <h3 align="center">AstroModLoader</h3>
</p>
<p align="center"><img src="https://i.imgur.com/CQX1FpH.png"></p>

AstroModLoader is a community-made, open-source mod manager for Astroneer .pak mods on Steam and the Windows Store. It includes support for mod profiles, automatic update integration, and the ability to easily swap between multiple mod versions so that you can worry less about setup and more about playing.

## Features
AstroModLoader includes the following features and more:
* A simple GUI to enable, disable, and switch the version of Astroneer .pak mods
* Mod metadata analysis to provide additional information about compliant mods, such as name, description, and compatible Astroneer version
* Automatic updating of supporting mods
* A profile system, to allow switching between sets of mods for different playthroughs
* Customizable appearance, with both light and dark themes as well as nine accent color presets
* Customizable mod load order by holding SHIFT and using the arrow keys in the mod list
* Easy drag-and-drop functionality to install mods
* Syncing of mods with modded [AstroLauncher](https://github.com/ricky-davis/AstroLauncher) servers
* [Built-in mod integration](https://github.com/AstroTechies/AstroModIntegrator) to help prevent mod conflict

## Usage
To run a local copy of AstroModLoader, visit the [Releases tab](https://github.com/AstroTechies/AstroModLoader/releases) to download the executable, or clone the repository and build AstroModLoader yourself within Visual Studio.

### Mod Installation
To install mods, drag and drop the .zip or the .pak file of your mod onto the AstroModLoader window while it is running.

Alternatively, on Steam, you can also manually add mods for use with AstroModLoader by placing them into the `%localappdata%\Astro\Saved\Mods` directory.
On the Windows Store, you can place them into the `%localappdata%\Packages\SystemEraSoftworks.29415440E1269_ftk5pbg2rayv2\LocalState\Astro\Saved\Mods` directory.

Feel free to join the Astroneer Modding Discord community to learn more about using or creating Astroneer mods: https://discord.gg/bBqdVYxu4k

### Usage Notes
AstroModLoader features a fully-functional set of hotkeys to fully control your mods list. Below is a list of keyboard commands to manipulate the list of mods:
* DEL deletes all versions of the currently selected mod.
* ALT+DEL deletes all versions of the currently selected mod except for the newest from disk.
* SHIFT+UP and SHIFT+DOWN adjust the position of the currently selected mod. Mods at the top of the list (low priority) are loaded by the game before mods at the bottom of the list (high priority).

[//]: # (* ESC de-selects the current row in the mod list.)

Additionally, the following keyboard commands can be used within the profile selector:
* DEL deletes the current profile.
* ENTER loads the current profile.

Additionally, the following keyboard commands can be used within popup windows:
* ENTER and ESC can be used within confirmation windows to select "Yes" or "No" respectively, and "OK" or "Cancel" respectively in text input windows.
* TAB can be used to switch selection between buttons. ENTER can then be used to press the currently selected button.

## Prerequisites
* .NET Framework 4.6.1

## Licensing
AstroModLoader is licensed under the MIT license, which can be found in [the LICENSE.md file.](https://github.com/AstroTechies/AstroModLoader/blob/master/LICENSE.md) In addition, necessary licenses for the third-party material used in this software can be found in [the NOTICE.md file.](https://github.com/AstroTechies/AstroModLoader/blob/master/NOTICE.md)