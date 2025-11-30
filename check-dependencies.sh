#!/bin/bash

echo "==================================="
echo "Dependency Checker"
echo "==================================="
echo ""

REFS_DIR="References"
MISSING=0

# Required DLLs
REQUIRED_DLLS=(
    "RagePluginHook.dll"
    "LSPD First Response.dll"
    "CalloutInterface.dll"
    "CalloutInterfaceAPI.dll"
    "IPT.Common.dll"
    "PolicingRedefined.dll"
)

echo "Checking for required reference DLLs..."
echo ""

if [ ! -d "$REFS_DIR" ]; then
    echo "❌ References directory not found!"
    echo "   Creating: $REFS_DIR/"
    mkdir -p "$REFS_DIR"
    echo ""
fi

for dll in "${REQUIRED_DLLS[@]}"; do
    if [ -f "$REFS_DIR/$dll" ]; then
        SIZE=$(du -h "$REFS_DIR/$dll" | cut -f1)
        echo "✓ $dll ($SIZE)"
    else
        echo "✗ $dll - MISSING"
        MISSING=$((MISSING + 1))
    fi
done

echo ""
echo "==================================="

if [ $MISSING -eq 0 ]; then
    echo "✓ All dependencies found!"
    echo ""
    echo "You can now build with: ./build.sh"
else
    echo "✗ Missing $MISSING dependencies"
    echo ""
    echo "To fix this:"
    echo "  1. Copy the missing DLLs from your GTA V installation"
    echo "  2. Place them in the References/ folder"
    echo ""
    echo "Typical locations:"
    echo "  - RagePluginHook.dll: [GTA V root]/"
    echo "  - LSPD First Response.dll: [GTA V]/plugins/LSPDFR/"
    echo "  - Other DLLs: [GTA V]/plugins/LSPDFR/"
    echo ""
    echo "Example:"
    echo "  cp /path/to/GTAV/RagePluginHook.dll References/"
    echo "  cp /path/to/GTAV/plugins/LSPDFR/*.dll References/"
fi

echo "==================================="
