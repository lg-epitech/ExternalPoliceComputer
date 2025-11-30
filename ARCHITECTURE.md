# ExternalPoliceComputer Architecture (SSE)

## System Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                         GTA V + LSPDFR                          │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │         ExternalPoliceComputer Plugin (C#)                │  │
│  │                                                           │  │
│  │  ┌─────────────┐      ┌──────────────┐                  │  │
│  │  │   Main.cs   │──────│  Server.cs   │                  │  │
│  │  │             │      │              │                  │  │
│  │  │ - OnDuty    │      │ HttpListener │                  │  │
│  │  │ - Init      │      │ Port: 8080   │                  │  │
│  │  └─────────────┘      └──────┬───────┘                  │  │
│  │                              │                           │  │
│  │                              ├─ /data/*                  │  │
│  │                              ├─ /post/*                  │  │
│  │                              ├─ /page/*                  │  │
│  │                              ├─ /script/*                │  │
│  │                              └─ /sse/*  ← NEW!          │  │
│  │                                   │                      │  │
│  │                         ┌─────────▼──────────┐          │  │
│  │                         │   SSEHandler.cs    │          │  │
│  │                         │                    │          │  │
│  │                         │ - HandleSSE()      │          │  │
│  │                         │ - SendEvent()      │          │  │
│  │                         │ - Manage Clients   │          │  │
│  │                         └────────────────────┘          │  │
│  │                                                           │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │           DataController.cs                        │  │  │
│  │  │                                                    │  │  │
│  │  │  - PlayerLocation  (updates every frame)          │  │  │
│  │  │  - CurrentTime     (updates every frame)          │  │  │
│  │  │  - PedDatabase                                     │  │  │
│  │  │  - VehicleDatabase                                 │  │  │
│  │  │  - Reports                                         │  │  │
│  │  │  - ShiftHistory                                    │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ HTTP/SSE
                              │
┌─────────────────────────────▼─────────────────────────────────┐
│                      Browser (Client)                         │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │                    index.html                           │  │
│  │                                                         │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌─────────────┐  │  │
│  │  │  Taskbar     │  │   Desktop    │  │  Settings   │  │  │
│  │  │              │  │              │  │             │  │  │
│  │  │ - Time       │  │ - Ped Search │  │ - Officer   │  │  │
│  │  │ - Location   │  │ - Vehicle    │  │ - Shift     │  │  │
│  │  └──────────────┘  │ - Reports    │  └─────────────┘  │  │
│  │                    │ - Court      │                    │  │
│  │                    └──────────────┘                    │  │
│  └─────────────────────────────────────────────────────────┘  │
│                              │                                │
│  ┌───────────────────────────▼──────────────────────────────┐ │
│  │                  JavaScript Layer                        │ │
│  │                                                          │ │
│  │  ┌────────────────┐  ┌────────────────┐               │ │
│  │  │  sseClient.js  │  │   index.js     │               │ │
│  │  │                │  │                │               │ │
│  │  │ SSEClient      │◄─┤ timeSSE        │               │ │
│  │  │ - connect()    │  │ locationSSE    │               │ │
│  │  │ - onmessage    │  │                │               │ │
│  │  │ - onclose      │  └────────────────┘               │ │
│  │  │ - reconnect    │                                    │ │
│  │  └────────────────┘  ┌────────────────┐               │ │
│  │                      │shiftHistory.js │               │ │
│  │                      │                │               │ │
│  │                      │ shiftHistorySSE│               │ │
│  │                      └────────────────┘               │ │
│  └──────────────────────────────────────────────────────────┘ │
└───────────────────────────────────────────────────────────────┘
```

## SSE Connection Flow

```
┌─────────┐                                    ┌─────────┐
│ Browser │                                    │ Server  │
└────┬────┘                                    └────┬────┘
     │                                              │
     │  GET /sse/time HTTP/1.1                     │
     │─────────────────────────────────────────────►│
     │                                              │
     │  HTTP/1.1 200 OK                            │
     │  Content-Type: text/event-stream            │
     │  Cache-Control: no-cache                    │
     │  Connection: keep-alive                     │
     │◄─────────────────────────────────────────────│
     │                                              │
     │  event: time                                │
     │  data: {"response":"12:34:56","request":"time"}
     │                                              │
     │◄─────────────────────────────────────────────│
     │                                              │
     │  (connection stays open)                    │
     │                                              │
     │  event: time                                │
     │  data: {"response":"12:34:57","request":"time"}
     │                                              │
     │◄─────────────────────────────────────────────│
     │                                              │
     │  (repeats every interval)                   │
     │                                              │
```

## Data Flow

### Real-Time Updates (Time & Location)

```
Game Loop (60 FPS)
    │
    ▼
DataController.SetDynamicData()
    │
    ├─► PlayerLocation = new Location(Player.Position)
    │
    └─► CurrentTime = World.TimeOfDay.ToString()
         │
         ▼
    SSEHandler (every config.webSocketUpdateInterval ms)
         │
         ├─► Check if data changed
         │
         └─► Send SSE event to all subscribed clients
              │
              ▼
         Browser receives event
              │
              ▼
         JavaScript updates DOM
              │
              └─► User sees updated time/location
```

### Event-Based Updates (Shift History)

```
User clicks "End Shift"
    │
    ▼
POST /post/modifyCurrentShift
    │
    ▼
DataController.EndCurrentShift()
    │
    ├─► Add shift to history
    │
    └─► Trigger ShiftHistoryUpdated event
         │
         ▼
    SSEHandler.OnShiftHistoryUpdated()
         │
         └─► Send SSE event to subscribed clients
              │
              ▼
         Browser receives event
              │
              ▼
         JavaScript reloads shift history page
```

## SSE Subscription Types

| Endpoint | Purpose | Update Frequency | Data |
|----------|---------|------------------|------|
| `/sse/time` | Current game time | Every interval (~100ms) | Time string |
| `/sse/playerLocation` | Player position | Every interval (~100ms) | Location object |
| `/sse/shiftHistoryUpdated` | Shift changes | On event | Notification |

## Client Management

```
┌──────────────────────────────────────────────────────────┐
│                    SSEHandler                            │
│                                                          │
│  Clients List (thread-safe)                             │
│  ┌────────────────────────────────────────────────────┐ │
│  │ Client #1: /sse/time                               │ │
│  │  - Response: HttpListenerResponse                  │ │
│  │  - Writer: StreamWriter                            │ │
│  │  - CancellationToken: Active                       │ │
│  ├────────────────────────────────────────────────────┤ │
│  │ Client #2: /sse/playerLocation                     │ │
│  │  - Response: HttpListenerResponse                  │ │
│  │  - Writer: StreamWriter                            │ │
│  │  - CancellationToken: Active                       │ │
│  ├────────────────────────────────────────────────────┤ │
│  │ Client #3: /sse/shiftHistoryUpdated                │ │
│  │  - Response: HttpListenerResponse                  │ │
│  │  - Writer: StreamWriter                            │ │
│  │  - CancellationToken: Active                       │ │
│  └────────────────────────────────────────────────────┘ │
│                                                          │
│  Operations:                                             │
│  - Add client on connection                              │
│  - Remove client on disconnect                           │
│  - Send events to subscribed clients                     │
│  - Clean up on server stop                               │
└──────────────────────────────────────────────────────────┘
```

## Error Handling & Reconnection

```
┌─────────┐                                    ┌─────────┐
│ Browser │                                    │ Server  │
└────┬────┘                                    └────┬────┘
     │                                              │
     │  Connected to /sse/time                     │
     │◄────────────────────────────────────────────►│
     │                                              │
     │  Connection lost (network issue)            │
     │  ✗                                           │
     │                                              │
     │  Browser auto-reconnects (3 sec delay)      │
     │─────────────────────────────────────────────►│
     │                                              │
     │  Reconnected successfully                   │
     │◄────────────────────────────────────────────►│
     │                                              │
```

## Performance Characteristics

### Memory Usage
- **Per Client**: ~50KB (StreamWriter + buffers)
- **Typical Load**: 1-5 clients = ~250KB
- **Maximum**: 100 clients = ~5MB

### CPU Usage
- **Idle**: <1% (just maintaining connections)
- **Active**: 1-2% (sending updates every 100ms)
- **Peak**: 3-5% (multiple clients + game events)

### Network Bandwidth
- **Per Client**: ~1-2 KB/s (time + location updates)
- **Typical Load**: 5-10 KB/s total
- **Burst**: Up to 50 KB/s (initial data + events)

## Comparison: WebSocket vs SSE

```
WebSocket Architecture:
┌─────────┐ ◄──────────────► ┌─────────┐
│ Browser │  Bidirectional   │ Server  │
└─────────┘                  └─────────┘
    │                            │
    ├─ Send: subscription       │
    │  Receive: data             │
    │                            │
    └─ Complex handshake         │
       Custom protocol           │
       Manual reconnection       │

SSE Architecture:
┌─────────┐ ◄────────────── ┌─────────┐
│ Browser │  Server→Client  │ Server  │
└─────────┘                 └─────────┘
    │                           │
    ├─ GET: /sse/{type}        │
    │  Receive: events          │
    │                           │
    └─ Standard HTTP            │
       Auto reconnection        │
       Built-in browser support │
```

## Security Considerations

1. **CORS**: Enabled with `Access-Control-Allow-Origin: *`
   - Safe for local network use
   - Consider restricting for public deployments

2. **Authentication**: None currently
   - Suitable for local/LAN use
   - Add auth headers if exposing publicly

3. **Rate Limiting**: None currently
   - Not needed for local use
   - Consider adding for public deployments

## Future Enhancements

Potential improvements:
- HTTP/2 for multiplexing multiple SSE streams
- Compression for bandwidth optimization
- Authentication tokens for security
- Heartbeat mechanism for connection health
- Client-side caching for offline resilience
