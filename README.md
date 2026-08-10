# EchoVault Goggles

A Dalamud plugin for FINAL FANTASY XIV. It queries [EchoVault](https://echovault.gg) for nearby
players and marks their nameplate with a star: red if they have a visible EchoVault profile, blue
if their profile is hidden.

## Requirements

* An [EchoVault](https://echovault.gg) API key, entered in the plugin's configuration window
  (accessible from the Plugin Installer). Without a key, the plugin does nothing.
* XIVLauncher, FINAL FANTASY XIV, and Dalamud installed and run at least once.
* .NET 10 SDK (the IDE will usually fetch this for you).

## Installing

This plugin is distributed through a self-hosted repository, not the official Dalamud plugin list
(it looks up other players automatically, which the official repo's review guidelines don't
allow). See [DalamudPlugins-EchoVault](https://github.com/LatiDev/DalamudPlugins-EchoVault) for the
subscription URL and install instructions.

## Building from source

1. Open `EchoVaultGoggles.sln` in Visual Studio or Rider.
2. Build (`Debug` or `Release`). Release also produces `latest.zip` in
   `EchoVaultGoggles/bin/x64/Release/EchoVaultGoggles/`, ready to drop into the distribution repo.

### Testing as a dev plugin

1. In-game, `/xlsettings` → **Experimental** → **Dev Plugin Locations**, add the folder containing
   the built `EchoVaultGoggles.dll`.
2. `/xlplugins` → **Dev Tools** / installed list, enable **EchoVault Goggles**.
3. Set an API key in its configuration window.

## Project layout

* `Plugin.cs` — entry point, wires up services and windows.
* `Services/EchoVault.cs` — HTTP client for the EchoVault API.
* `Displays/TrackingDisplay.cs` — subscribes to nameplate updates, looks up nearby players
  (5-minute cache per character), and draws the star icon.
* `Windows/ConfigWindow.cs` — API key entry.

See [PUBLISHING-A-DALAMUD-PLUGIN.md](PUBLISHING-A-DALAMUD-PLUGIN.md) for the full publishing
process this project follows.
