# Build and Test Guide

## Quick Build

```bash
# 1. Restore NuGet packages (includes websocket-sharp)
cd ExternalPoliceComputer
nuget restore ExternalPoliceComputer.sln

# 2. Build
msbuild ExternalPoliceComputer.sln /p:Configuration=Release

# 3. Check output
ls -lh ExternalPoliceComputer/bin/Release/
```

## Expected Output

```
ExternalPoliceComputer.dll
websocket-sharp.dll  ← NEW!
Newtonsoft.Json.dll
CommonDataFramework.dll
```

## Installation

```bash
# Copy to GTA V directory
cp ExternalPoliceComputer/bin/Release/ExternalPoliceComputer.dll /path/to/GTAV/plugins/LSPDFR/
cp ExternalPoliceComputer/bin/Release/websocket-sharp.dll /path/to/GTAV/
cp -r EPC /path/to/GTAV/
```

## Testing in Wine

### 1. Start GTA V
```bash
# Through Steam/Proton
steam steam://rungameid/271590

# Or direct Wine
wine GTA5.exe
```

### 2. Go On Duty
- Load LSPDFR
- Go on duty
- Look for EPC notifications showing:
  - HTTP: `http://localhost:8080`
  - WebSocket: `ws://localhost:8081/ws` (in logs)

### 3. Open EPC in Browser
```bash
# Open browser (outside Wine)
firefox http://localhost:8080
# or
chromium http://localhost:8080
```

### 4. Verify WebSocket Connection

**Browser DevTools (F12)**:
1. Go to **Network** tab
2. Filter by **WS** (WebSocket)
3. Should see connections to `ws://localhost:8081/ws`
4. Status should be **101 Switching Protocols**
5. Messages tab should show real-time data

**What to Check**:
- ✅ Time updates every second (bottom right)
- ✅ Location updates when driving (bottom left)
- ✅ Shift history updates when ending shift

### 5. Check Logs

**Rage.log** (in GTA V directory):
```
WebSocket server started on port 8081
New WebSocket connection: [ID]
```

**Browser Console** (F12):
- No WebSocket errors
- No connection failures

## Troubleshooting

### "WebSocket connection failed"

**Check ports**:
```bash
# In Wine prefix
netstat -an | grep 8081
```

Should show:
```
tcp  0  0  0.0.0.0:8081  0.0.0.0:*  LISTEN
```

**Check firewall**:
```bash
# Allow port 8081
sudo ufw allow 8081/tcp
```

### "websocket-sharp.dll not found"

Copy it manually:
```bash
cp packages/WebSocketSharp.1.0.3-rc11/lib/websocket-sharp.dll /path/to/GTAV/
```

### "Time/Location not updating"

1. Check WebSocket connection in browser DevTools
2. Check Rage.log for WebSocket errors
3. Verify port 8081 is accessible
4. Try restarting GTA V

### "Build errors"

Missing dependencies:
```bash
./check-dependencies.sh
# Copy missing DLLs to References/
```

## Performance Testing

### Latency Test
Open browser console and run:
```javascript
const start = Date.now()
const ws = new WebSocket('ws://localhost:8081/ws')
ws.onopen = () => {
  console.log(`Connection time: ${Date.now() - start}ms`)
  ws.send('ping')
}
ws.onmessage = (e) => {
  console.log(`Round-trip time: ${Date.now() - start}ms`)
  console.log('Response:', e.data)
}
```

Expected: <50ms connection, <10ms round-trip

### Memory Test
Check memory usage in Task Manager/htop:
- Should be similar to before (~50-100MB for plugin)
- websocket-sharp adds minimal overhead (<5MB)

## Success Criteria

✅ Plugin loads without errors  
✅ WebSocket server starts on port 8081  
✅ Browser connects to WebSocket  
✅ Time updates in real-time  
✅ Location updates when driving  
✅ Shift history updates when ending shift  
✅ No errors in Rage.log  
✅ No errors in browser console  

## Comparison: Before vs After

| Aspect | Before (Native) | After (websocket-sharp) |
|--------|----------------|-------------------------|
| Wine Compatibility | ❌ Broken | ✅ Works |
| WebSocket Protocol | RFC 6455 | RFC 6455 |
| Port | 8080 | 8081 |
| Performance | Fast | Fast |
| Dependencies | System.Net.WebSockets | websocket-sharp |
| Native Code | Yes (WinSocket) | No (Pure C#) |

## Next Steps

Once testing is successful:
1. Commit changes
2. Create release build
3. Update documentation
4. Notify users of Wine compatibility fix

## Need Help?

Check these files:
- **WINE_WEBSOCKET_FIX.md** - Technical details
- **SUMMARY.md** - Overview of changes
- **BUILD_ON_LINUX.md** - Linux build guide
