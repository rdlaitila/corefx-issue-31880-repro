using Microsoft.Owin;
using Owin;
using Server;
using System.Web.Http;

[assembly: OwinStartup(typeof(Startup))]

namespace Server
{
    public class Startup
    {
        public static bool UseServerWorkaround = false;

        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();

            if (UseServerWorkaround)
            {
                app.Use(async (context, next) =>
                {
                    await next();
                    if (context.Response.StatusCode == 101)
                    {
                        // this forces ClientWebSocket to believe their is content
                        // to process thus it keeps the socket alive for processing??
                        context.Response.Headers.Set("Content-Length", "1");
                    }
                });
            }

            app.UseStaticFiles();
            app.UseWebApi(config);
        }
    }
}