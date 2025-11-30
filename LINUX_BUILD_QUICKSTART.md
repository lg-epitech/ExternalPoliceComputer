# Linux Build - Quick Start

## One-Time Setup

### 1. Install Mono and build tools
```bash
sudo apt update
sudo apt install -y mono-complete msbuild nuget

# Verify installation
msbuild /version
```

### 2. Copy GTA V Reference DLLs

You need to copy the GTA V plugin DLLs to the `References/` folder:

```bash
# Check what's missing
./check-dependencies.sh

# Copy from your GTA V installation (adjust paths as needed)
cp /path/to/GTAV/RagePluginHook.dll References/
cp /path/to/GTAV/plugins/LSPDFR/*.dll References/
```

**Required DLLs:**
- RagePluginHook.dll
- LSPD First Response.dll
- CalloutInterface.dll
- CalloutInterfaceAPI.dll
- IPT.Common.dll
- PolicingRedefined.dll

## Build the Plugin

```bash
# Option 1: Use the build script (easiest)
./build.sh

# Option 2: Manual build
cd ExternalPoliceComputer
nuget restore ExternalPoliceComputer.sln
msbuild ExternalPoliceComputer.sln /p:Configuration=Release /p:Platform="Any CPU"
```

## Output Location

```
ExternalPoliceComputer/ExternalPoliceComputer/bin/Release/ExternalPoliceComputer.dll
```

## Install to GTA V

```bash
# Copy DLL
cp ExternalPoliceComputer/ExternalPoliceComputer/bin/Release/ExternalPoliceComputer.dll \
   /path/to/GTAV/plugins/LSPDFR/

# Copy EPC folder
cp -r EPC /path/to/GTAV/
```

## Troubleshooting

**"msbuild: command not found"**
```bash
sudo apt install mono-complete msbuild
```

**"nuget: command not found"**
```bash
sudo apt install nuget
```

**"Missing reference DLLs"**
- Ensure all GTA V plugin DLLs are in the `References/` folder
- Required: RagePluginHook.dll, LSPD First Response.dll, etc.

**Build warnings about Windows-specific features**
- Safe to ignore - the DLL will run fine on Windows

## Clean Build

```bash
cd ExternalPoliceComputer
rm -rf ExternalPoliceComputer/bin ExternalPoliceComputer/obj
nuget restore ExternalPoliceComputer.sln
msbuild ExternalPoliceComputer.sln /p:Configuration=Release /p:Platform="Any CPU"
```

## Check Mono Version

```bash
mono --version
# Should be 6.0 or higher
```

## That's It!

The DLL built on Linux will work perfectly on Windows with GTA V. Cross-compilation for .NET Framework is fully supported by Mono.
