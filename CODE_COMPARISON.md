# Code Comparison: WebSocket vs SSE

## Server.cs Changes

### Before (WebSocket)
```csharp
private static void HandleRequest(HttpListenerContext ctx) { 
    if (ctx.Request.IsWebSocketRequest && ctx.Request.RawUrl == "/ws") {
        WebSocketHandler.HandleWebSocket(ctx);
        return;
    }
    // ... rest of code
}

internal static async void Stop() {
    RunServer = false;
    await WebSocketHandler.CloseAllWebSockets();
    listener?.Stop();
}
```

### After (SSE)
```csharp
private static void HandleRequest(HttpListenerContext ctx) { 
    if (ctx.Request.RawUrl.StartsWith("/sse/")) {
        SSEHandler.HandleSSE(ctx);
        return;
    }
    // ... rest of code
}

internal static void Stop() {
    RunServer = false;
    SSEHandler.CloseAllConnections();
    listener?.Stop();
}
```

## Client-Side Changes

### Before (WebSocket)
```javascript
// index.js
const timeWS = new WebSocket(`ws://${location.host}/ws`)
timeWS.onopen = () => timeWS.send('interval/time')

timeWS.onmessage = async (event) => {
  const data = JSON.parse(event.data)
  // ... handle data
}

timeWS.onclose = async () => {
  showNotification('Connection lost', 'warning', -1)
}
```

### After (SSE)
```javascript
// index.js
const timeSSE = new SSEClient('time')

timeSSE.onmessage = async (event) => {
  const data = JSON.parse(event.data)
  // ... handle data
}

timeSSE.onclose = async () => {
  showNotification('Connection lost', 'warning', -1)
}

timeSSE.connect()
```

## Key Differences

### Protocol Level

**WebSocket:**
- Bidirectional: Client ↔ Server
- Custom protocol: `ws://` or `wss://`
- Requires handshake upgrade
- Manual reconnection needed

**SSE:**
- Unidirectional: Server → Client
- Standard HTTP: `http://` or `https://`
- No special handshake
- Browser auto-reconnects

### Connection Establishment

**WebSocket:**
```
Client: GET /ws HTTP/1.1
        Upgrade: websocket
        Connection: Upgrade
        
Server: HTTP/1.1 101 Switching Protocols
        Upgrade: websocket
        Connection: Upgrade
```

**SSE:**
```
Client: GET /sse/time HTTP/1.1

Server: HTTP/1.1 200 OK
        Content-Type: text/event-stream
        Cache-Control: no-cache
        Connection: keep-alive
```

### Message Format

**WebSocket:**
```json
{"response": "data", "request": "time"}
```

**SSE:**
```
event: time
data: {"response": "data", "request": "time"}

```

### Backend Implementation

**WebSocket:**
```csharp
// Accept WebSocket
HttpListenerWebSocketContext wsContext = await ctx.AcceptWebSocketAsync(null);
WebSocket webSocket = wsContext.WebSocket;

// Receive message
var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
string clientMsg = Encoding.UTF8.GetString(buffer, 0, result.Count);

// Send message
await webSocket.SendAsync(
    new ArraySegment<byte>(Encoding.UTF8.GetBytes(responseMsg)),
    WebSocketMessageType.Text,
    true,
    token
);
```

**SSE:**
```csharp
// Set SSE headers
res.ContentType = "text/event-stream";
res.Headers.Add("Cache-Control", "no-cache");
res.Headers.Add("Connection", "keep-alive");

// Create writer
StreamWriter writer = new StreamWriter(res.OutputStream, Encoding.UTF8) {
    AutoFlush = true
};

// Send event
string message = $"event: {eventType}\ndata: {data}\n\n";
writer.Write(message);
```

## Why SSE is Better for This Use Case

1. **One-Way Communication**: EPC only needs server → client updates (time, location, events)
2. **HTTP Compatibility**: Works through firewalls/proxies that block WebSocket
3. **Simpler Protocol**: No handshake complexity, just HTTP
4. **Auto Reconnection**: Browser handles reconnection automatically
5. **Better Debugging**: Standard HTTP tools work (curl, Postman, browser DevTools)

## Performance Comparison

| Metric | WebSocket | SSE |
|--------|-----------|-----|
| Latency | ~1-5ms | ~1-5ms |
| Overhead | ~2 bytes per frame | ~20 bytes per event |
| Memory | Higher (bidirectional buffers) | Lower (one-way stream) |
| CPU | Slightly higher (frame parsing) | Slightly lower (text parsing) |
| Reconnection | Manual (custom logic) | Automatic (browser built-in) |

For this application, the overhead difference is negligible, and SSE's benefits far outweigh any minor performance differences.

## Migration Path

The refactoring maintains API compatibility:
- Same data structures
- Same update intervals
- Same subscription types
- Same error handling

Only the transport mechanism changed, making it a clean, low-risk refactoring.
