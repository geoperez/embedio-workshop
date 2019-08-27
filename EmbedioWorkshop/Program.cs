using System;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace EmbedioWorkshop
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var server = new WebServer("http://localhost:1234");

            //server.HandleHttpException(HttpExceptionHandler.FullDataResponse(ResponseSerializer.Default));
            // server.OnAny(async ctx => { await ctx.SendStandardHtmlAsync(200); });
            /*server.OnGet(async ctx => { await ctx.SendDataAsync(new { Hola = true}); });
            server.OnPost(async ctx =>
            {
                var request = await ctx.GetRequestDataAsync<ContentJson>();
                await ctx.SendDataAsync(request);
            });*/

            server.WithLocalSessionManager(); //Genera el manejo de session con una coockie
            server.WithModule(new MyModule("module"));

            server.WithWebApi("/api", m =>
            {
                m.HandleHttpException(HttpExceptionHandler.FullDataResponse(ResponseSerializer.Default));
                m.RegisterController<MyController>();
            });

            server.WithStaticFolder("/", @"C:\\Users\\jose.correa\\Unosquare\\wwwroot", true);
            server.RunAsync();
            
            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }

        public class ContentJson
        {
            public bool Hola { get; set; }
        }

        public class MyModule : WebModuleBase
        {
            public MyModule(string route): base(route){

            }

            public override bool IsFinalHandler => true;

            protected override async Task OnRequestAsync(IHttpContext context)
            {
                switch (context.RequestedPath)
                {
                    case "/counter":
                        {
                            await context.SendStringAsync("1", "text/html", System.Text.Encoding.UTF8);
                            break;
                        }
                            
                    
                }

                throw HttpException.NotFound();
            }
        }


        public class MyController: WebApiController
        {
            [Route(HttpVerbs.Get, "/counter")]
            public object Count()
            {
                int counterInt = 0;
                if(Session.TryGetValue("counter", out var counter))
                {
                    counterInt = Convert.ToInt32(counter);
                }

                Session["counter"] = counterInt + 1;

                return Session["counter"];
            }

            [Route(HttpVerbs.Get, "/version")]
            public object GetVersion([QueryField] string product)
            {
                return new { Version = "1.0", Product = product };
            }

            [Route(HttpVerbs.Get, "/version/{client}")]
            public object GetVersionCategory(string client, [QueryField] string etc)
            {
                if (client != "Unosquare")
                    throw HttpException.BadRequest("Only Unosquare", new { Error = "Se murio esto"});

                return new { Version = "1.0", Client = client, Etc = etc };
            }

            [Route(HttpVerbs.Post, "/echo")]
            public async Task<object> GetEcho()
            {
                return await HttpContext.GetRequestDataAsync<object>();
            }

            [Route(HttpVerbs.Post, "/form")]
            public object GetForm([FormData] NameValueCollection data)
            {
                return data[0];
            }

            [Route(HttpVerbs.Post, "/echo2")]
            public async Task<object> GetEcho2([ObjectDtaRequest] object requestObj)
            {
                return requestObj;
            }

        }

        [AttributeUsage(AttributeTargets.Parameter)]
        public class ObjectDtaRequestAttribute : Attribute, IRequestDataAttribute<WebApiController, object>
        {
            public Task<object> GetRequestDataAsync(WebApiController controller, string parameterName)
            {
                var requestObj = controller.HttpContext.GetRequestDataAsync<object>();
                if (requestObj == null)
                    throw new ArgumentNullException(string.Empty);

                return requestObj;
            }
        }
    }

}
