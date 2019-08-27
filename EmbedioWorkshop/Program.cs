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
            var server = new WebServer("http://localhost:1234")
                .WithModule(new MyModule("/custom"))
                .WithLocalSessionManager(m=>
                {
                    m.CookieDuration = TimeSpan.FromSeconds(5);
                    //m.SessionDuration = TimeSpan.FromSeconds(5);
                })
                .WithWebApi("/api", m =>
                {
                    m.RegisterController<CustomController>();
                    m.OnHttpException = HttpExceptionHandler.FullDataResponse(ResponseSerializer.Json);
                })
                .WithStaticFolder("/", "../../../wwwroot", true)
                .OnGet(async ctx => await ctx.SendDataAsync(new { Name = "Carlos", Last = "Solorzano", IsActibve = true }))
                .OnPost(async ctx => await ctx.SendDataAsync(await ctx.GetRequestDataAsync<ContentJson>()));

            _ = server.RunAsync();
            
            Console.ReadLine();
        }

        public class ContentJson
        {
            public string Saludo { get; set; }
        }
    }

    public class MyModule : WebModuleBase
    {
        public MyModule(string baseRoute)
            : base(baseRoute)
        {

        }

        public override bool IsFinalHandler { get; } = true;

        protected override async Task OnRequestAsync(IHttpContext context)
        {
            switch (context.RequestedPath)
            {
                case "/counter":
                {
                    await context.SendStringAsync("1", "text/plain", System.Text.Encoding.UTF8);
                    break;
                }
            }

            throw HttpException.NotFound();
        }
    }

    public class CustomController : WebApiController
    {
        [Route(HttpVerbs.Get, "/counter")]
        public object Count()
        {
            var counterInt = 0;
            if (Session.TryGetValue("counter", out var counter))
                counterInt = Convert.ToInt32(counter);

            Session["counter"] = counterInt += 1;

            return Session["counter"];
        }

        [Route(HttpVerbs.Get, "/version")]
        public object GetVersion()
        {
            return new
            {
                version = 1.0,
            };
        }

        [Route(HttpVerbs.Get, "/version2")]
        public object GetVersion2([QueryField] string product)
        {
            return new
            {
                version = 1.0,
                product,
            };
        }

        [Route(HttpVerbs.Get, "/version/{product?}")]
        public object GetVersion3(string product)
        {
            return new
            {
                version = 1.0,
                product,
            };
        }

        [Route(HttpVerbs.Get, "/version/{client}/{product?}")]
        public object GetVersion3(string client, string product, [QueryField] string saludo)
        {
            if (client != "Unosquare")
                throw HttpException.BadRequest("Only Unosquare",new { Data = "Hazlo bien" });

            return new
            {
                version = 1.0,
                client,
                product,
                saludo,
            };
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
    }

    public class ObjectDataRequestAttribute : Attribute, IRequestDataAttribute<WebApiController, object>
    {
        public Task<object> GetRequestDataAsync(WebApiController controller, string parameterName)
        {
            return controller.HttpContext.GetRequestDataAsync<object>();
        }
    }
}
