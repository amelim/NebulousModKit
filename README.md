# Nebulous Mod Kit

The Nebulous Mod Kid (NMK) provides a pre-configured Unity environment to build mods for NEBULOUS: Fleet Command by Eridanus Industries. The goal is provide an easy, tested workflow for creating new maps, components, scripts, and ships for the modding community. 

Currently the mod supports the following content pipelines:
* Mapping

Additionally, there are some quality of life scripts/prefabs built into the modding kit
* One button loading/updating of game dlls (no more copy)
* AssetBundling
* Example Map (Generates a complete barebones map)

Finally, new gameplay scripts will be available as part of the modding kit to enable more game play options, all fully functional with the current modding framework and without any additionaly dependencies for players! Currently supporting
* Randomized map control point layers. Design multiple different control point layouts in a single map!

## Instructions
* Download the zip of this project and save as a folder which you open as a Unity project. 
* Once open, you go to Windows->Nebulous to bring up the Nebulous Mod Kit window.
* Load your dlls by either typing in your Nebulous root game folder, or if you have an empty install string it'll open up a dialog for you to navigate
* Click Generate map template to build a map
* Click Asset Bundle generation to build an asset bundle (requires manually manifest and mod info creation atm)
