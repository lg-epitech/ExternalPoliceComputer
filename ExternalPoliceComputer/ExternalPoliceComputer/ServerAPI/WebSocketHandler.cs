using ExternalPoliceComputer.Data;
using ExternalPoliceComputer.Setup;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ExternalPoliceComputer.Utility.Helper;

namespace ExternalPoliceComputer.ServerAPI {
    internal class SSEHandler {
        private class SSEClient {
            public HttpListenerResponse Response { get; set; }
            public StreamWriter Writer { get; set; }
            public CancellationTokenSource CancellationToken { get; set; }
            public string SubscriptionType { get; set; }
            public int Id { get; set; }
        }

        private static readonly List<SSEClient> Clients = new List<SSEClient>();
        private static readonly object ClientsLock = new object();
        private static int nextClientId = 0;

        private static string lastPlayerLocation = "";
        private static string lastTime = "";

        internal static void HandleSSE(HttpListenerContext ctx) {
            try {
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse res = ctx.Response;

                string path = req.Url.AbsolutePath;
                string subscriptionType = path.Replace("/sse/", "");

                res.ContentType = "text/event-stream";
                res.Headers.Add("Cache-Control", "no-cache");
                res.Headers.Add("Connection", "keep-alive");
                res.Headers.Add("Access-Control-Allow-Origin", "*");
                res.StatusCode = 200;

                StreamWriter writer = new StreamWriter(res.OutputStream, Encoding.UTF8) {
                    AutoFlush = true
                };

                SSEClient client = new SSEClient {
                    Response = res,
                    Writer = writer,
                    CancellationToken = new CancellationTokenSource(),
                    SubscriptionType = subscriptionType,
                    Id = Interlocked.Increment(ref nextClientId)
                };

                lock (ClientsLock) {
                    Clients.Add(client);
                }

                Log($"New SSE client #{client.Id} subscribed to: {subscriptionType}", true, LogSeverity.Info);

                SendInitialData(client);

                if (subscriptionType == "shiftHistoryUpdated") {
                    DataController.ShiftHistoryUpdated += () => OnShiftHistoryUpdated(client);
                } else if (subscriptionType == "playerLocation" || subscriptionType == "time") {
                    Task.Run(() => SendPeriodicUpdates(client), client.CancellationToken.Token);
                }

                while (Server.RunServer && !client.CancellationToken.Token.IsCancellationRequested) {
                    Thread.Sleep(1000);
                    if (!res.OutputStream.CanWrite) break;
                }

            } catch (HttpListenerException) {
            } catch (Exception e) {
                if (Server.RunServer) Log($"SSE Error: {e.Message}", true, LogSeverity.Error);
            }
        }

        private static void SendInitialData(SSEClient client) {
            try {
                switch (client.SubscriptionType) {
                    case "playerLocation":
                        string locationData = JsonConvert.SerializeObject(DataController.PlayerLocation);
                        SendEvent(client, locationData, "playerLocation");
                        lastPlayerLocation = locationData;
                        break;
                    case "time":
                        string timeData = $"\"{DataController.CurrentTime}\"";
                        SendEvent(client, timeData, "time");
                        lastTime = timeData;
                        break;
                    case "ping":
                        SendEvent(client, "\"Pong!\"", "ping");
                        break;
                }
            } catch (Exception e) {
                Log($"SSE initial data error: {e.Message}", true, LogSeverity.Warning);
            }
        }

        private static async Task SendPeriodicUpdates(SSEClient client) {
            try {
                while (Server.RunServer && !client.CancellationToken.Token.IsCancellationRequested) {
                    await Task.Delay(SetupController.GetConfig().webSocketUpdateInterval, client.CancellationToken.Token);

                    string responseMsg = "";
                    switch (client.SubscriptionType) {
                        case "playerLocation":
                            responseMsg = JsonConvert.SerializeObject(DataController.PlayerLocation);
                            if (responseMsg != lastPlayerLocation) {
                                lastPlayerLocation = responseMsg;
                                SendEvent(client, responseMsg, "playerLocation");
                            }
                            break;
                        case "time":
                            responseMsg = $"\"{DataController.CurrentTime}\"";
                            if (responseMsg != lastTime) {
                                lastTime = responseMsg;
                                SendEvent(client, responseMsg, "time");
                            }
                            break;
                    }
                }
            } catch (OperationCanceledException) {
            } catch (Exception e) {
                if (Server.RunServer) Log($"SSE periodic update error: {e.Message}", true, LogSeverity.Warning);
                RemoveClient(client);
            }
        }

        private static void OnShiftHistoryUpdated(SSEClient client) {
            if (!Server.RunServer || client.CancellationToken.Token.IsCancellationRequested) return;
            SendEvent(client, "\"Shift history updated\"", "shiftHistoryUpdated");
        }

        private static void SendEvent(SSEClient client, string data, string eventType) {
            try {
                if (!client.Response.OutputStream.CanWrite) {
                    RemoveClient(client);
                    return;
                }

                string message = $"event: {eventType}\ndata: {{\"response\": {data}, \"request\": \"{eventType}\"}}\n\n";
                client.Writer.Write(message);
            } catch (Exception e) {
                Log($"SSE send error: {e.Message}", true, LogSeverity.Warning);
                RemoveClient(client);
            }
        }

        private static void RemoveClient(SSEClient client) {
            lock (ClientsLock) {
                if (Clients.Contains(client)) {
                    Clients.Remove(client);
                    Log($"SSE client #{client.Id} disconnected", true, LogSeverity.Info);
                }
            }
        }

        internal static void CloseAllConnections() {
            SSEClient[] clientsArr;
            lock (ClientsLock) {
                clientsArr = Clients.ToArray();
                Clients.Clear();
            }

            foreach (SSEClient client in clientsArr) {
                try {
                    client.CancellationToken.Cancel();
                    client.Writer?.Close();
                    client.Response?.Close();
                    Log($"Closing SSE client #{client.Id}", true, LogSeverity.Info);
                } catch (Exception e) {
                    Log($"SSE close error: {e.Message}", true, LogSeverity.Warning);
                }
            }
        }
    }
}
