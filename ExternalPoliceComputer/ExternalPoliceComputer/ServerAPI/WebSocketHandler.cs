using ExternalPoliceComputer.Data;
using ExternalPoliceComputer.Setup;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;
using static ExternalPoliceComputer.Utility.Helper;

namespace ExternalPoliceComputer.ServerAPI {
    internal class WebSocketHandler {
        private static WebSocketServer wsServer;
        private static readonly object ServerLock = new object();

        internal static void StartWebSocketServer(int port) {
            lock (ServerLock) {
                if (wsServer != null) {
                    wsServer.Stop();
                }

                wsServer = new WebSocketServer(port);
                wsServer.AddWebSocketService<EPCWebSocketBehavior>("/ws");
                wsServer.Start();
                Log($"WebSocket server started on port {port}", true, LogSeverity.Info);
            }
        }

        internal static void StopWebSocketServer() {
            lock (ServerLock) {
                if (wsServer != null && wsServer.IsListening) {
                    wsServer.Stop();
                    Log("WebSocket server stopped", true, LogSeverity.Info);
                }
            }
        }
    }

    internal class EPCWebSocketBehavior : WebSocketBehavior {
        private CancellationTokenSource intervalToken;
        private string subscriptionType;
        private string lastPlayerLocation = "";
        private string lastTime = "";

        protected override void OnOpen() {
            Log($"New WebSocket connection: {ID}", true, LogSeverity.Info);
        }

        protected override void OnMessage(MessageEventArgs e) {
            try {
                string clientMsg = e.Data.Trim();

                if (clientMsg.StartsWith("interval/")) {
                    subscriptionType = clientMsg.Substring("interval/".Length);
                    intervalToken?.Cancel();
                    intervalToken = new CancellationTokenSource();
                    Task.Run(() => SendUpdatesOnInterval(intervalToken.Token));
                } else {
                    switch (clientMsg) {
                        case "ping":
                            SendData("\"Pong!\"", clientMsg);
                            break;
                        case "shiftHistoryUpdated":
                            DataController.ShiftHistoryUpdated += OnShiftHistoryUpdated;
                            break;
                        default:
                            SendData($"\"Unknown command: '{clientMsg}'\"", clientMsg);
                            break;
                    }
                }
            } catch (Exception ex) {
                if (Server.RunServer) Log($"WebSocket message error: {ex.Message}", true, LogSeverity.Error);
            }
        }

        protected override void OnClose(CloseEventArgs e) {
            intervalToken?.Cancel();
            DataController.ShiftHistoryUpdated -= OnShiftHistoryUpdated;
            Log($"WebSocket closed: {ID} - {e.Reason}", true, LogSeverity.Info);
        }

        protected override void OnError(ErrorEventArgs e) {
            if (Server.RunServer) Log($"WebSocket error: {e.Message}", true, LogSeverity.Error);
        }

        private void OnShiftHistoryUpdated() {
            if (State == WebSocketState.Open && Server.RunServer) {
                SendData("\"Shift history updated\"", "shiftHistoryUpdated");
            }
        }

        private async Task SendUpdatesOnInterval(CancellationToken token) {
            try {
                while (State == WebSocketState.Open && Server.RunServer && !token.IsCancellationRequested) {
                    string responseMsg = "";
                    
                    switch (subscriptionType) {
                        case "playerLocation":
                            responseMsg = JsonConvert.SerializeObject(DataController.PlayerLocation);
                            if (responseMsg != lastPlayerLocation) {
                                lastPlayerLocation = responseMsg;
                                SendData(responseMsg, subscriptionType);
                            }
                            break;
                        case "time":
                            responseMsg = $"\"{DataController.CurrentTime}\"";
                            if (responseMsg != lastTime) {
                                lastTime = responseMsg;
                                SendData(responseMsg, subscriptionType);
                            }
                            break;
                        default:
                            SendData($"\"Unknown interval command: '{subscriptionType}'\"", subscriptionType);
                            return;
                    }

                    await Task.Delay(SetupController.GetConfig().webSocketUpdateInterval, token);
                }
            } catch (OperationCanceledException) {
                // Normal cancellation
            } catch (Exception e) {
                string innerMessage = e.InnerException != null ? $"Inner: {e.InnerException.Message}" : "";
                Log($"WebSocket interval error: {e.Message}{innerMessage}", true, LogSeverity.Error);
            }
        }

        private void SendData(string data, string clientMsg) {
            try {
                if (State == WebSocketState.Open) {
                    string responseMsg = $"{{ \"response\": {data}, \"request\": \"{clientMsg}\" }}";
                    Send(responseMsg);
                }
            } catch (Exception e) {
                Log($"WebSocket send error: {e.Message}", true, LogSeverity.Warning);
            }
        }
    }
}
