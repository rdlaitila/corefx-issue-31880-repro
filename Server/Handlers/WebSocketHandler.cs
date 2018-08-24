using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Web.WebSockets;

namespace Server.Handlers
{
    public class WebSocketHandler
    {
        public async Task ProcessAsync(AspNetWebSocketContext context)
        {
            var socket = context.WebSocket;
            var recvBuffer = new byte[1024];

            while (socket.State == WebSocketState.Open)
            {
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

                    await socket.SendAsync(
                        new ArraySegment<byte>(ms.ToArray()),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None
                    );
                }
            }
        }
    }
}