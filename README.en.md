# Personal Combat Drone for Stardew Valley

[中文](README.md)

An unofficial Stardew Valley 1.6 rewrite of
[Prism-99/murderdrone](https://github.com/Prism-99/murderdrone). It uses the
native `Companion` system so every player independently owns, controls, and
synchronizes their drone in local split-screen and online multiplayer.

## Usage

- Default bindings: keyboard `F7` or press the controller's left stick; GMCM can change them.
- Each split-screen player controls only their own drone through their assigned input device.
- Every player must install the mod for online multiplayer.
- Don't install it alongside the original Personal Combat Drone.

## Build

Requires Stardew Valley 1.6, SMAPI 4.x, and the .NET 6 SDK or newer:

```sh
dotnet build sdv1.6.x/sdv1.6.x.csproj -c Release
```

GitHub Actions creates a downloadable test package for every push and pull
request. Push a `v*` tag matching the project version (for example `v1.4.0`)
to create a GitHub Release automatically.

Test split-screen, separate locations and mine floors, save/load, armored bugs,
and rock crabs before release.
