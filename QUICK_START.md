# Quick Start: Building After SSE Refactoring

## What Was Done

Your ExternalPoliceComputer plugin has been refactored from WebSocket to Server-Sent Events (SSE) for better HTTP compatibility. All functionality remains the same.

## Build the Plugin

### Option 1: Visual Studio
1. Open `ExternalPoliceComputer/ExternalPoliceComputer.sln`
2. Select **Release** configuration
3. Build Solution (Ctrl+Shift+B)
4. DLL location: `ExternalPoliceComputer/ExternalPoliceComputer/bin/Release/ExternalPoliceComputer.dll`

### Option 2: Command Line (MSBuild)
```bash
cd ExternalPoliceComputer
msbuild ExternalPoliceComputer.sln /p:Configuration=Release
```

## Install

1. Copy the compiled DLL to: `GTA V/plugins/LSPDFR/`
2. Copy the entire `EPC` folder to: `GTA V/`
3. Ensure all dependencies are in `GTA V/` (RagePluginHook, LSPDFR, etc.)

## What Changed

### Backend
- `WebSocketHandler.cs` → Now uses SSE instead of WebSocket
- `Server.cs` → Routes to `/sse/` endpoints instead of `/ws`

### Frontend
- New file: `sseClient.js` - SSE client wrapper
- Updated: `index.js`, `shiftHistory.js` - Use SSE instead of WebSocket
- Updated: `index.html`, `shiftHistory.html` - Include sseClient.js

## Testing

1. Start GTA V with LSPDFR
2. Go on duty
3. Open browser to the EPC address shown in-game
4. Verify:
   - Time updates on taskbar
   - Location updates on taskbar
   - Shift history updates when ending shift

## Troubleshooting

### "Connection lost" notification
- Check if the server is running (go on duty in LSPDFR)
- Check browser console for errors (F12)
- Verify the URL matches the in-game notification

### Time/Location not updating
- Check browser console for SSE connection errors
- Verify `sseClient.js` is loaded (check Network tab in F12)
- Check server logs in `Rage.log`

### Build errors
- Ensure all references are in the `References` folder
- Verify .NET Framework 4.8 is installed
- Check NuGet packages are restored

## Key Benefits

✅ Better firewall/proxy compatibility  
✅ Standard HTTP (no special WebSocket protocol)  
✅ Automatic reconnection  
✅ Same functionality as before  
✅ More reliable connections  

## Need Help?

Check these files for details:
- `REFACTORING_SUMMARY.md` - Complete technical overview
- `MIGRATION_WEBSOCKET_TO_SSE.md` - Detailed migration guide

## Rollback

If you need to revert to WebSocket, restore these files from git:
- `ExternalPoliceComputer/ExternalPoliceComputer/ServerAPI/WebSocketHandler.cs`
- `ExternalPoliceComputer/ExternalPoliceComputer/Server.cs`
- `EPC/main/scripts/index.js`
- `EPC/main/scripts/shiftHistory.js`
- `EPC/main/pages/index.html`
- `EPC/main/pages/shiftHistory.html`

And delete: `EPC/main/scripts/sseClient.js`
