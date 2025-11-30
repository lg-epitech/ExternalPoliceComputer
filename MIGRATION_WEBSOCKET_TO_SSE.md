# WebSocket to Server-Sent Events (SSE) Migration Guide

## Overview

This document describes the refactoring of ExternalPoliceComputer from WebSocket to Server-Sent Events (SSE) for improved HTTP compatibility.

## What Changed

### Backend (C#)

#### 1. WebSocketHandler.cs → SSEHandler.cs
- **File**: `ExternalPoliceComputer/ExternalPoliceComputer/ServerAPI/WebSocketHandler.cs`
- **Changes**:
  - Replaced WebSocket bidirectional communication with SSE (server-to-client streaming)
  - Changed from `WebSocket` class to `HttpListenerResponse` with `StreamWriter`
  - Removed WebSocket handshake logic
  - Implemented SSE event format: `event: {type}\ndata: {json}\n\n`
  - Maintained same subscription types: `playerLocation`, `time`, `shiftHistoryUpdated`

#### 2. Server.cs
- **Changes**:
  - Replaced WebSocket endpoint check (`/ws`) with SSE endpoint check (`/sse/`)
  - Changed `HandleWebSocket()` call to `SSEHandler.HandleSSE()`
  - Updated `Stop()` method from async to sync (no longer needs await)
  - Changed `CloseAllWebSockets()` to `CloseAllConnections()`

### Frontend (JavaScript)

#### 1. New SSE Client Helper
- **File**: `EPC/main/scripts/sseClient.js`
- **Purpose**: Provides WebSocket-like API using EventSource (SSE)
- **Features**:
  - Automatic reconnection with exponential backoff
  - WebSocket-compatible interface (`onmessage`, `onopen`, `onclose`, `onerror`)
  - Handles SSE event types automatically

#### 2. index.js
- **Changes**:
  - Replaced `new WebSocket()` with `new SSEClient()`
  - Removed `ws.send()` calls (SSE is server-to-client only)
  - Changed endpoint from `ws://${location.host}/ws` to SSE subscription types
  - Added explicit `connect()` calls

#### 3. shiftHistory.js
- **Changes**:
  - Same pattern as index.js
  - Replaced WebSocket with SSEClient

#### 4. HTML Pages
- **Files**: `index.html`, `shiftHistory.html`
- **Changes**: Added `<script src="/script/sseClient.js"></script>` before page-specific scripts

## Technical Details

### SSE vs WebSocket

| Feature | WebSocket | SSE (New) |
|---------|-----------|-----------|
| Direction | Bidirectional | Server-to-client only |
| Protocol | `ws://` or `wss://` | Standard HTTP/HTTPS |
| Reconnection | Manual | Automatic (browser built-in) |
| Compatibility | Requires WebSocket support | Works with any HTTP server |
| Firewall/Proxy | Often blocked | Better compatibility |

### SSE Event Format

```
event: playerLocation
data: {"response": {...}, "request": "playerLocation"}

```

### Endpoint Structure

**Old WebSocket**:
- Single endpoint: `/ws`
- Client sends subscription type via message

**New SSE**:
- Multiple endpoints: `/sse/{subscriptionType}`
- Examples: `/sse/playerLocation`, `/sse/time`, `/sse/shiftHistoryUpdated`

## Migration Benefits

1. **Better Compatibility**: SSE uses standard HTTP, avoiding WebSocket firewall/proxy issues
2. **Simpler Protocol**: No need for WebSocket handshake complexity
3. **Built-in Reconnection**: Browsers automatically reconnect SSE connections
4. **Same Functionality**: All real-time features maintained

## Testing Checklist

- [ ] Time updates display correctly on taskbar
- [ ] Player location updates correctly on taskbar
- [ ] Shift history page updates when shift ends
- [ ] Reconnection works after server restart
- [ ] No console errors in browser
- [ ] Multiple browser tabs work simultaneously

## Rollback Plan

If issues arise, revert these files:
1. `ExternalPoliceComputer/ExternalPoliceComputer/ServerAPI/WebSocketHandler.cs`
2. `ExternalPoliceComputer/ExternalPoliceComputer/Server.cs`
3. `EPC/main/scripts/index.js`
4. `EPC/main/scripts/shiftHistory.js`
5. Delete `EPC/main/scripts/sseClient.js`
6. Remove sseClient.js script tags from HTML files

## Notes

- SSE connections are one-way (server → client), which is perfect for this use case since the client only needs to receive updates
- The client-side API remains nearly identical to WebSocket, minimizing code changes
- SSE automatically handles reconnection, making the system more resilient
- All existing functionality is preserved with better HTTP compatibility
