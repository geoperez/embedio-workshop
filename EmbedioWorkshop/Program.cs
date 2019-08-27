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
            server.WithModule(new MyModule("/api"));



            //server.HandleHttpException(HttpExceptionHandler.DataResponse(ResponseSerializer.Json));
            //server.OnHttpException = HttpExceptionHandler.FullDataResponse(ResponseSerializer.Json);
            //server.HandleHttpException(HttpExceptionHandler.FullDataResponse(ResponseSerializer.Json));
            //server.OnAny(async ctx =>
            //{
            //   // await ctx.SendStandardHtmlAsync("Hola Mundo Cruel!", "text/plain", System.Text.Encoding.UTF8);
            //});
            //server.OnGet(async ctx =>
            //{
            //     await ctx.SendDataAsync(new ContentJson({ Hola = true}));
            //});
            server.WithLocalSessionManager();
            server.WithWebApi("/api", m => {
                m.RegisterController<MyController>();
                m.OnHttpException = HttpExceptionHandler.FullDataResponse(ResponseSerializer.Json);
            });
            server.WithStaticFolder("/", @"C:\\Users\\ana.atayde\\source\\repos\\embedio-workshop\\wwwroot", true);

            //archivos staticos 

            //server.OnPost(async ctx =>
            //{
            //    var request = await ctx.GetRequestDataAsync<ContentJson>();
            //    await ctx.SendDataAsync(request);
            //});
            server.RunAsync();

            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }

    public class ContentJson
    {
        public bool Hola { get; set; }
    }

    public class MyController : WebApiController
    {
        [Route(HttpVerbs.Get, "/version")]
        public object getVersion() => new { Version = 1.0 };

        [Route(HttpVerbs.Post, "/echos")]
        public async Task<object> GetEcho() => await HttpContext.GetRequestDataAsync<object>();

        [Route(HttpVerbs.Post, "/echo")]
        public object GetEcho([ObjectdataRequest] object requestob)
        {
            return requestob;
        }

        [Route(HttpVerbs.Post, "/form")]
        public object GetForm([FormData] NameValueCollection data)
        {
            return data[0];
        }

        [Route(HttpVerbs.Get, "/version/{product}")]
        public object getVersion(string product)
        {
            return new { Version = 1.0, Product = product };
        }

        [Route(HttpVerbs.Get, "/version/{client}/{product}")]
        public object getVersion(string client, string product)
        {
            if (client != "Unosqure")
                throw HttpException.BadRequest("Only Unosquare");

            //throw new InvalidOperationException("Only Unosquare");
            return new { Version = 1.0, Client = client, Product = product };
        }

        [Route(HttpVerbs.Get, "/counter")]
        public object Count()
        {
            var countrInt = 0;
            if (Session.TryGetValue("counter", out var counter))
                countrInt = Convert.ToInt32(counter);

            Session["counter"] = countrInt + 1;

            return Session["counter"];
        }

    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class ObjectdataRequestAttribute : Attribute, IRequestDataAttribute<WebApiController, object>
    {
        public Task<object> GetRequestDataAsync(WebApiController controller, string parameterName)
        {
            return controller.HttpContext.GetRequestDataAsync<object>();
        }
    }

    public class MyModule : WebModuleBase
    {
        public MyModule(string baseRoute) : base(baseRoute)
        {

        }
        public override bool IsFinalHandler { get; } = true;

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
}
