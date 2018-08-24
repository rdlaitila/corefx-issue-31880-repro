using Server.Handlers;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Server.Controllers
{
    public class WebSocketController : ApiController
    {
        [HttpGet]
        [Route("websocket")]
        public HttpResponseMessage GetWebsocket()
        {
            var currentContext = HttpContext.Current;

            if (!currentContext.IsWebSocketRequest && !currentContext.IsWebSocketRequestUpgrading)
            {
                return Request.CreateErrorResponse(
                    HttpStatusCode.UpgradeRequired,
                    "this endpoint requires the use of websockets"
                );
            }

            currentContext.AcceptWebSocketRequest(async context =>
                await new WebSocketHandler().ProcessAsync(context)
            );

            return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
        }
    }
}