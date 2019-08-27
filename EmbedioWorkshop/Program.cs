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
            server.WithModule(new MyModule("/custom"));
            //server.OnAny(async ctx => //server.OnGet(
            //{
            //    await ctx.SendStandardHtmlAsync(200);
            //    await ctx.SendStringAsync("hola", "text/plain", System.Text.Encoding.UTF8);
            //    await ctx.SendDataAsync(new { hola = true });
            //});
            // server.OnGet(async ctx =>
            // {
            //     await ctx.getrequest
            // })
            //server.OnPost(async ctx =>
            //{
            //    var request = await ctx.GetRequestDataAsync<ContentJson>();
            //    await ctx.SendDataAsync(request);
            //});
            server.WithLocalSessionManager();
            server.WithWebApi("/api", m =>
            {
                m.RegisterController<MyController>();
                m.OnHttpException = HttpExceptionHandler.FullDataResponse(ResponseSerializer.Json);
            });
            server.WithStaticFolder("/", @"C:\\Unosquare\\wwroot", true);
            //server.WithZipFile()// Zip is always inmutable.
            //server.WithEmbeddedResources
            //configurations must be called before runAsync.
            server.RunAsync();

            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }

    public class MyModule : WebModuleBase
    {
        public MyModule(string baseRoute) : base(baseRoute)
        {

        }

        protected override async Task OnRequestAsync(IHttpContext context)
        {
            switch (context.RequestedPath)
            {
                case "counter":
                    {
                        await context.SendStringAsync("1", "text/html", System.Text.Encoding.UTF8);
                        break;
                    }
            }

            throw HttpException.NotFound();
        }


        public override bool IsFinalHandler { get; } = true;
    }

    public class ContentJson
    {
        public bool Hola { get; set; }
    }

    public class MyController : WebApiController
    {

        [Route(HttpVerbs.Get, "/Version/{client}/{product?}")]
        public object GetVersion4(string client, string product, [QueryField] string etc)
        {
            //There are two kinds of exceptions. The system ones and the embedIO's exceptions.
            if (client != "Unosquare")
                throw HttpException.BadRequest("only Unosquare", new { Error = true });

            return new { Version = "1.0", Client = client, Product = product, Etc = etc };
        }

        [Route(HttpVerbs.Get, "/counter")]
        public object Count()
        {
            int counterInt = 0;

            if (Session.TryGetValue("counter", out var counter))
                counterInt = Convert.ToInt32(counter);

            Session["counter"] = counterInt + 1;

            return Session["counter"];
        }

        [Route(HttpVerbs.Get, "/version")]
        public object GetVerions()
        {
            return new { Version = "1.0" };
        }

        [Route(HttpVerbs.Post, "/echo")]
        public async Task<object> GetEcho()
        {
            return await HttpContext.GetRequestDataAsync<object>();
        }

        [Route(HttpVerbs.Post, "/echo2")]
        public object GetEcho2([ObjectDataRequest] object requestObj)
        {
            return requestObj;
        }

        [Route(HttpVerbs.Post, "/form")]
        public object GetForm([FormData] NameValueCollection data)
        {
            return data[0];
        }

        [Route(HttpVerbs.Get, "/Version2")]
        public object GetVersion2([QueryField] string product)
        {
            return new { Version = "1.0", Product = product };
        }

        [Route(HttpVerbs.Get, "/Version/{product?}")]
        public object GetVersion3(string product = null)
        {
            return new { Version = "1.0", Product = product };
        }
    }

    public class ObjectDataRequestAttribute : Attribute, IRequestDataAttribute<WebApiController, object>
    {
        public Task<object> GetRequestDataAsync(WebApiController controller, string parameterName)
        {
            var requestObj = controller.HttpContext.GetRequestDataAsync<object>();
            //if (requestObj == null)
            //    throw new

            return requestObj;
        }
    }
}
