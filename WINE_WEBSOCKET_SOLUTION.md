# WebSocket Solution for Wine Compatibility

## Problem
Wine's implementation of WinSocket API for WebSockets (`HttpListener.AcceptWebSocketAsync()`) is incomplete/buggy, causing compatibility issues when running GTA V + LSPDFR through Wine/Proton.

## Solution Options

### Option 1: Use websocket-sharp (Recommended)
A pure C# WebSocket implementation that doesn't rely on native WinSocket APIs.

**Pros:**
- Pure managed code
- Better Wine compatibility
- Actively maintained
- Drop-in replacement

**Implementation:**
```bash
# Add NuGet package
cd ExternalPoliceComputer
nuget install WebSocketSharp -OutputDirectory packages
```

### Option 2: Use Fleck
Another pure C# WebSocket server library.

**Pros:**
- Simple API
- Good Wine compatibility
- Lightweight

### Option 3: Manual WebSocket Implementation
Implement RFC 6455 WebSocket protocol manually over standard TCP sockets.

**Pros:**
- Full control
- No external dependencies
- Can optimize for Wine

**Cons:**
- More code to maintain
- Need to handle protocol details

## Recommended: websocket-sharp

This is the best balance of compatibility and ease of implementation.
