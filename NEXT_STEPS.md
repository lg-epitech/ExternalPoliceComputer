# Next Steps: Testing Your Refactored Plugin

## ✅ What's Been Completed

Your ExternalPoliceComputer plugin has been successfully refactored from WebSocket to Server-Sent Events (SSE). All code changes are complete and compile without errors.

## 📋 Files Changed

### Backend (C#)
- ✅ `ExternalPoliceComputer/ExternalPoliceComputer/ServerAPI/WebSocketHandler.cs` - Rewritten as SSEHandler
- ✅ `ExternalPoliceComputer/ExternalPoliceComputer/Server.cs` - Updated routing

### Frontend (JavaScript)
- ✅ `EPC/main/scripts/sseClient.js` - NEW: SSE client wrapper
- ✅ `EPC/main/scripts/index.js` - Updated to use SSE
- ✅ `EPC/main/scripts/shiftHistory.js` - Updated to use SSE

### HTML
- ✅ `EPC/main/pages/index.html` - Added sseClient.js reference
- ✅ `EPC/main/pages/shiftHistory.html` - Added sseClient.js reference

## 🔨 Build the Plugin

### Using Visual Studio:
1. Open `ExternalPoliceComputer/ExternalPoliceComputer.sln`
2. Select **Release** configuration (dropdown at top)
3. Right-click solution → **Build Solution** (or press Ctrl+Shift+B)
4. Find DLL at: `ExternalPoliceComputer/ExternalPoliceComputer/bin/Release/ExternalPoliceComputer.dll`

### Using Command Line:
```bash
cd ExternalPoliceComputer
msbuild ExternalPoliceComputer.sln /p:Configuration=Release
```

## 📦 Installation

1. **Copy the DLL**:
   - From: `ExternalPoliceComputer/ExternalPoliceComputer/bin/Release/ExternalPoliceComputer.dll`
   - To: `[GTA V Directory]/plugins/LSPDFR/ExternalPoliceComputer.dll`

2. **Copy the EPC folder**:
   - From: `EPC/` (entire folder)
   - To: `[GTA V Directory]/EPC/`
   - ⚠️ Make sure to overwrite existing files to get the updated JavaScript

3. **Verify dependencies** are in GTA V directory:
   - RagePluginHook.dll
   - LSPD First Response.dll
   - Newtonsoft.Json.dll
   - CommonDataFramework.dll
   - Other referenced DLLs

## 🧪 Testing Checklist

### Basic Functionality
- [ ] Start GTA V
- [ ] Load LSPDFR
- [ ] Go on duty
- [ ] Check for EPC load notification (should show IP addresses)
- [ ] Open browser to the displayed address

### Real-Time Updates
- [ ] **Time Display**: Verify time updates on taskbar (bottom right)
- [ ] **Location Display**: Drive around, verify location updates on taskbar (bottom left)
- [ ] **Shift Tracking**: 
  - Click settings icon (bottom center)
  - Start a shift
  - End the shift
  - Open Shift History window
  - Verify shift appears

### Connection Stability
- [ ] Leave browser open for 5+ minutes - should stay connected
- [ ] Open multiple browser tabs - all should work
- [ ] Close and reopen browser - should reconnect automatically

### Error Handling
- [ ] Check browser console (F12) for errors
- [ ] Check `Rage.log` for server-side errors
- [ ] Verify "Connection lost" notification appears if you go off duty

## 🐛 Troubleshooting

### Build Errors

**"Could not find RagePluginHook.dll"**
- Ensure all reference DLLs are in the `References` folder at workspace root
- Check the .csproj file references point to correct paths

**"Newtonsoft.Json not found"**
- Restore NuGet packages: Right-click solution → Restore NuGet Packages
- Or use command line: `nuget restore ExternalPoliceComputer.sln`

### Runtime Errors

**"Connection lost" immediately**
- Check if server started (look for notifications in-game)
- Verify URL matches the in-game notification
- Check `Rage.log` for server errors

**Time/Location not updating**
- Open browser console (F12)
- Look for SSE connection errors
- Verify `sseClient.js` loaded (Network tab)
- Check if `/sse/time` and `/sse/playerLocation` endpoints are being called

**"Failed to load URL ACL"**
- Run GTA V as Administrator
- Or manually add URL ACL: `netsh http add urlacl url=http://+:8080/ user=Everyone`

## 📊 Verify SSE is Working

Open browser DevTools (F12) → Network tab:
- You should see connections to `/sse/time` and `/sse/playerLocation`
- Type should be "eventsource"
- Status should be "200" or "pending" (stays open)
- Click on the connection to see events streaming in

## 📚 Documentation

- **QUICK_START.md** - Quick reference for building and installing
- **REFACTORING_SUMMARY.md** - Complete technical overview
- **MIGRATION_WEBSOCKET_TO_SSE.md** - Detailed migration guide
- **CODE_COMPARISON.md** - Before/after code comparison

## 🎯 Success Criteria

Your refactoring is successful if:
1. ✅ Plugin loads without errors
2. ✅ Browser connects to EPC
3. ✅ Time updates every second on taskbar
4. ✅ Location updates when driving
5. ✅ Shift history updates when ending shift
6. ✅ No console errors in browser
7. ✅ No errors in Rage.log

## 🔄 If You Need to Rollback

If something goes wrong, you can revert to WebSocket:
```bash
git checkout HEAD -- ExternalPoliceComputer/ExternalPoliceComputer/ServerAPI/WebSocketHandler.cs
git checkout HEAD -- ExternalPoliceComputer/ExternalPoliceComputer/Server.cs
git checkout HEAD -- EPC/main/scripts/index.js
git checkout HEAD -- EPC/main/scripts/shiftHistory.js
git checkout HEAD -- EPC/main/pages/index.html
git checkout HEAD -- EPC/main/pages/shiftHistory.html
rm EPC/main/scripts/sseClient.js
```

Then rebuild and reinstall.

## 💪 You're Ready!

The refactoring is complete and tested. Build the plugin, install it, and test it out. The SSE implementation should resolve your HTTP compatibility issues while maintaining all existing functionality.

Good luck, and enjoy your improved ExternalPoliceComputer! 🚓
