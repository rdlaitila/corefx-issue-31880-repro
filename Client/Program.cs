using System;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    internal class Program
    {
        public static bool UseClientWorkaround = false;

        private static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, errors) => true;

            if (UseClientWorkaround)
            {
                // this seems to just delay the inevitable by setting a very large
                // max idle. Not a scalable workaround as this would affect all
                // ServicePoint's created after this call
                ServicePointManager.MaxServicePointIdleTime = int.MaxValue;
            }

            Start().Wait();
        }

        private static async Task Start()
        {
            try
            {
                var socket = new ClientWebSocket();
                var recvBuffer = new byte[1024];

                await socket.ConnectAsync(
                    new Uri("wss://localhost:44361/websocket"),
                    CancellationToken.None
                );

                while (socket.State == WebSocketState.Open)
                {
                    var msg = DateTime.UtcNow;

                    Console.WriteLine($"SEND: {msg}");

                    var msgBytes = Encoding.UTF8.GetBytes(
                        msg.ToLongDateString()
                    );

                    await socket.SendAsync(
                        new ArraySegment<byte>(msgBytes),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None
                    );

                    using (var ms = new MemoryStream())
                    {
                        WebSocketReceiveResult result;

                        do
                        {
                            result = await socket.ReceiveAsync(
                                new ArraySegment<byte>(recvBuffer),
                                CancellationToken.None
                            );

                            ms.Write(recvBuffer, 0, result.Count);
                        } while (!result.EndOfMessage);

                        var data = Encoding.UTF8.GetString(
                            ms.ToArray()
                        );

                        Console.WriteLine($"RECV: {data}");
                    }

                    await Task.Delay(1000);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}