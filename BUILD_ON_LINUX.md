# Building ExternalPoliceComputer on Linux

Your project targets .NET Framework 4.8, which is Windows-specific. However, you have several options to build it on Linux.

## Option 1: Using Mono (Recommended for .NET Framework)

Mono is an open-source implementation of .NET Framework that works on Linux.

### Install Mono

**Ubuntu/Debian:**
```bash
sudo apt update
sudo apt install mono-complete msbuild nuget
```

**Arch Linux:**
```bash
sudo pacman -S mono msbuild nuget
```

**Fedora:**
```bash
sudo dnf install mono-complete msbuild nuget
```

### Restore NuGet Packages

```bash
cd ExternalPoliceComputer
nuget restore ExternalPoliceComputer.sln
```

### Build with MSBuild (Mono)

```bash
msbuild ExternalPoliceComputer.sln /p:Configuration=Release /p:Platform="Any CPU"
```

### Output Location

```
ExternalPoliceComputer/ExternalPoliceComputer/bin/Release/ExternalPoliceComputer.dll
```

## Option 2: Using Docker with Windows Container

Build using a Windows container (requires Docker with Windows container support).

### Create Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8

WORKDIR /app
COPY . .

RUN nuget restore ExternalPoliceComputer/ExternalPoliceComputer.sln
RUN msbuild ExternalPoliceComputer/ExternalPoliceComputer.sln /p:Configuration=Release /p:Platform="Any CPU"
```

### Build

```bash
docker build -t epc-builder .
docker create --name epc-build epc-builder
docker cp epc-build:/app/ExternalPoliceComputer/ExternalPoliceComputer/bin/Release/ExternalPoliceComputer.dll ./
docker rm epc-build
```

## Option 3: Using Wine + MSBuild

Run Windows MSBuild through Wine.

### Install Wine

```bash
sudo apt install wine64 winetricks
```

### Install .NET Framework in Wine

```bash
winetricks dotnet48
```

### Build

```bash
wine msbuild ExternalPoliceComputer/ExternalPoliceComputer.sln /p:Configuration=Release
```

## Option 4: Cross-Platform Build Script (Easiest)

I'll create a build script that handles everything for you.

### Create build.sh

```bash
#!/bin/bash

set -e

echo "==================================="
echo "ExternalPoliceComputer Build Script"
echo "==================================="

# Check if mono is installed
if ! command -v msbuild &> /dev/null; then
    echo "Error: MSBuild not found. Please install Mono:"
    echo "  Ubuntu/Debian: sudo apt install mono-complete msbuild nuget"
    echo "  Arch: sudo pacman -S mono msbuild nuget"
    echo "  Fedora: sudo dnf install mono-complete msbuild nuget"
    exit 1
fi

# Navigate to solution directory
cd ExternalPoliceComputer

# Restore NuGet packages
echo ""
echo "Restoring NuGet packages..."
nuget restore ExternalPoliceComputer.sln

# Build the solution
echo ""
echo "Building solution..."
msbuild ExternalPoliceComputer.sln /p:Configuration=Release /p:Platform="Any CPU" /v:minimal

# Check if build succeeded
if [ -f "ExternalPoliceComputer/bin/Release/ExternalPoliceComputer.dll" ]; then
    echo ""
    echo "==================================="
    echo "Build successful!"
    echo "==================================="
    echo "DLL location: ExternalPoliceComputer/ExternalPoliceComputer/bin/Release/ExternalPoliceComputer.dll"
    echo ""
    echo "Next steps:"
    echo "1. Copy ExternalPoliceComputer.dll to your GTA V plugins/LSPDFR folder"
    echo "2. Copy the EPC folder to your GTA V directory"
    echo "3. Ensure all reference DLLs are in place"
else
    echo ""
    echo "Build failed! Check the output above for errors."
    exit 1
fi
```

### Make it executable and run

```bash
chmod +x build.sh
./build.sh
```

## Option 5: GitHub Actions (Automated)

Set up automated builds that run on Windows in the cloud.

### Create .github/workflows/build.yml

```yaml
name: Build ExternalPoliceComputer

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup MSBuild
      uses: microsoft/setup-msbuild@v1.1
    
    - name: Setup NuGet
      uses: NuGet/setup-nuget@v1
    
    - name: Restore NuGet packages
      run: nuget restore ExternalPoliceComputer/ExternalPoliceComputer.sln
    
    - name: Build solution
      run: msbuild ExternalPoliceComputer/ExternalPoliceComputer.sln /p:Configuration=Release /p:Platform="Any CPU"
    
    - name: Upload artifact
      uses: actions/upload-artifact@v3
      with:
        name: ExternalPoliceComputer-DLL
        path: ExternalPoliceComputer/ExternalPoliceComputer/bin/Release/ExternalPoliceComputer.dll
```

Then download the built DLL from GitHub Actions artifacts.

## Troubleshooting

### Missing References

If you get errors about missing DLLs (RagePluginHook, LSPDFR, etc.), ensure they're in the `References` folder:

```bash
mkdir -p References
# Copy your reference DLLs here
cp /path/to/RagePluginHook.dll References/
cp /path/to/LSPD\ First\ Response.dll References/
# etc.
```

### NuGet Package Restore Fails

```bash
# Clear NuGet cache
nuget locals all -clear

# Restore again
cd ExternalPoliceComputer
nuget restore ExternalPoliceComputer.sln
```

### Mono Build Warnings

Mono may show warnings about Windows-specific features. These are usually safe to ignore for GTA V plugins as they'll run on Windows anyway.

### Path Issues

If you get path errors, try using absolute paths:

```bash
msbuild "$(pwd)/ExternalPoliceComputer/ExternalPoliceComputer.sln" /p:Configuration=Release
```

## Recommended Approach

For your use case, I recommend **Option 1 (Mono)** because:
- ✅ Native Linux support
- ✅ Fast builds
- ✅ No virtualization overhead
- ✅ Easy to script and automate
- ✅ Works with your existing project structure

## Quick Start Commands

```bash
# Install Mono (Ubuntu/Debian)
sudo apt update && sudo apt install -y mono-complete msbuild nuget

# Navigate to project
cd ExternalPoliceComputer

# Restore packages
nuget restore ExternalPoliceComputer.sln

# Build
msbuild ExternalPoliceComputer.sln /p:Configuration=Release /p:Platform="Any CPU"

# Your DLL is ready!
ls -lh ExternalPoliceComputer/bin/Release/ExternalPoliceComputer.dll
```

That's it! The DLL will work perfectly on Windows/GTA V even though it was built on Linux.
