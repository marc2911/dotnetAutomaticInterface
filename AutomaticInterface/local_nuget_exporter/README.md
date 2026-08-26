# Local NuGet package workflow

This folder contains a PowerShell helper that creates a local NuGet feed from the `DotnetAutomaticInterface` project and adds the generated package to that feed.

## 1) Generate the local package

Run the script [pack-local-nuget.ps1](./pack-local-nuget.ps1) with optional arguments for the project path, build configuration, and feed output folder.

If you omit any of the arguments, the script uses these defaults:

- Project: `..\DotnetAutomaticInterface\DotnetAutomaticInterface.csproj`
- Configuration: `Release`
- Output: `local_nuget_feed`

Example:

```powershell
./pack-local-nuget.ps1 -Project "..\DotnetAutomaticInterface\DotnetAutomaticInterface.csproj" -Configuration "Release" -Output "local_nuget"
```

The script will:

- run `dotnet pack` in Release configuration
- run `nuget add`
- create a local pkg feed at `./local_nuget_feed` next to the script with a ``local version suffix``

If `nuget.exe` is not installed, the script will try to install it via `winget`.

After the script finishes, the package is available under a structure similar to:

```text
scripts\local_nuget_feed\
  dotnetautomaticinterface\
    version\
      dotnetautomaticinterface.version.nupkg
```

The version is read from the project file [DotnetAutomaticInterface.csproj](../DotnetAutomaticInterface/DotnetAutomaticInterface.csproj), so it may change over time.

## 2) Use the local package in your own project

You can just copy the `local_nuget_feed` over. You need to then configure nuget at the solution level to point to that feed. Create:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="LocalNugetFeed" value=".\local_nuget_feed\" />
  </packageSources>
</configuration>
```

Then inside your nuget manager you should be able to filter only for that local feed named `LocalNugetFeed`. The pkg should then be available for install for any project:
