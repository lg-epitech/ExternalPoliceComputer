using ExternalPoliceComputer.ServerAPI;
using Rage;
using System;
using System.IO;
using System.Net;
using System.Threading;
using static ExternalPoliceComputer.Utility.Helper;

namespace ExternalPoliceComputer {
    internal class Server {
        internal static bool RunServer;

        private static HttpListener listener;

        internal static void Start() {
            listener?.Close();
            RunServer = true;
            
            int port = Setup.SetupController.GetConfig().port;
            int wsPort = port + 1; // WebSocket on port + 1
            
            listener = new HttpListener();
            listener.Prefixes.Add($"http://+:{port}/");
            listener.Start();
            
            // Start WebSocket server on port + 1
            WebSocketHandler.StartWebSocketServer(wsPort);
            
            string fullIp = $"http://{GetLocalIPAddress()}:{port}";
            string fullName = $"http://{Environment.MachineName}:{port}";
            string wsInfo = $"WebSocket: ws://{GetLocalIPAddress()}:{wsPort}/ws";
            Game.DisplayNotification($"{Setup.SetupController.GetLanguage().inGame.listeningOnIpAddress}{fullIp}");
            Game.DisplayNotification($"{Setup.SetupController.GetLanguage().inGame.listeningOnIpAddress}{fullName}");
            Log(wsInfo, true, LogSeverity.Info);
            File.WriteAllText(Setup.SetupController.IpAddressesPath, $"{fullIp}\n{fullName}\n{wsInfo}");

            while (RunServer) {
                try {
                    var context = listener.GetContext();
                    ThreadPool.QueueUserWorkItem(_ => HandleRequest(context));
                } catch (HttpListenerException e) {
                    if (RunServer) Log($"HTTP Listener Exception: {e.Message}", true, LogSeverity.Error);
                } catch (Exception e) {
                    Log($"Server Exception: {e.Message}", true, LogSeverity.Error);
                }
            }

            listener.Close();
        }

        private static void HandleRequest(HttpListenerContext ctx) { 
            // WebSocket requests are now handled by websocket-sharp server
            // No need to check for WebSocket upgrade here
            
            HttpListenerRequest req = ctx.Request;
            HttpListenerResponse res = ctx.Response;

            APIResponse apiRes = GetAPIResponse(req);

            byte[] buffer = apiRes.buffer;

            res.ContentType = apiRes.contentType;
            res.ContentLength64 = buffer.LongLength;
            res.StatusCode = apiRes.status;

            res.OutputStream.Write(buffer, 0, buffer.Length);
            res.OutputStream.Close();
        }

        internal static void Stop() {
            RunServer = false;
            WebSocketHandler.StopWebSocketServer();
            listener?.Stop();
        }

        internal static APIResponse GetAPIResponse(HttpListenerRequest req) {
            string path = req.Url.AbsolutePath;
            if (path.StartsWith("/data/")) {
                return new DataAPIResponse(req);
            } else if (path.StartsWith("/post/")) {
                return new PostAPIResponse(req);
            } else if (path.StartsWith("/plugin/")) {
                return new PluginAPIResponse(req); 
            } else if (path.StartsWith("/page/")) {
                return new PageAPIResponse(req);
            } else if (path.StartsWith("/style/")) {
                return new StyleAPIResponse(req);
            } else if (path.StartsWith("/script/")) {
                return new ScriptAPIResponse(req);
            } else if (path.StartsWith("/image/")) {
                return new ImageAPIResponse(req);
            }
            return new APIResponse(req);
        }
    }
}