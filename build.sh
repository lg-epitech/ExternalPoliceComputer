#!/bin/bash

set -e

echo "==================================="
echo "ExternalPoliceComputer Build Script"
echo "==================================="

# Check if mono is installed
if ! command -v msbuild &> /dev/null; then
    echo "Error: MSBuild not found. Please install Mono:"
    echo ""
    echo "  Ubuntu/Debian: sudo apt install mono-complete msbuild nuget"
    echo "  Arch Linux:    sudo pacman -S mono msbuild nuget"
    echo "  Fedora:        sudo dnf install mono-complete msbuild nuget"
    echo ""
    exit 1
fi

# Check if nuget is installed
if ! command -v nuget &> /dev/null; then
    echo "Error: NuGet not found. Please install it:"
    echo ""
    echo "  Ubuntu/Debian: sudo apt install nuget"
    echo "  Arch Linux:    sudo pacman -S nuget"
    echo "  Fedora:        sudo dnf install nuget"
    echo ""
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
echo "Building solution (Release configuration)..."
msbuild ExternalPoliceComputer.sln /p:Configuration=Release /p:Platform="Any CPU" /v:minimal /nologo

# Check if build succeeded
if [ -f "ExternalPoliceComputer/bin/Release/ExternalPoliceComputer.dll" ]; then
    echo ""
    echo "==================================="
    echo "✓ Build successful!"
    echo "==================================="
    echo ""
    echo "Output: ExternalPoliceComputer/ExternalPoliceComputer/bin/Release/ExternalPoliceComputer.dll"
    echo "Size:   $(du -h ExternalPoliceComputer/bin/Release/ExternalPoliceComputer.dll | cut -f1)"
    echo ""
    echo "Next steps:"
    echo "  1. Copy ExternalPoliceComputer.dll to: [GTA V]/plugins/LSPDFR/"
    echo "  2. Copy the EPC folder to: [GTA V]/"
    echo "  3. Ensure reference DLLs are in place"
    echo ""
    echo "Files to copy:"
    echo "  - ExternalPoliceComputer/ExternalPoliceComputer/bin/Release/ExternalPoliceComputer.dll"
    echo "  - EPC/ (entire folder)"
    echo ""
else
    echo ""
    echo "✗ Build failed! Check the output above for errors."
    echo ""
    echo "Common issues:"
    echo "  - Missing reference DLLs in References/ folder"
    echo "  - NuGet packages not restored"
    echo "  - Mono version too old (need 6.0+)"
    echo ""
    exit 1
fi
