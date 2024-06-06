// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.22.0

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.Azure.Blobs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace Botina
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.TEST
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
            });

            // Create the Bot Framework Authentication to be used with the Bot Adapter.
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Create the Bot Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, Bots.EchoBot>();

            //Use Azure Blob storage, instead of in-memory storage.
            services.AddSingleton<IStorage>(
                new BlobsStorage(
                    "DefaultEndpointsProtocol=https;AccountName=miloslearn;AccountKey=NuTR0ELFCmVa9kaZmb8cHB7Y8YOcuJnTwB9K65T9IcBYfssuYq/94JRdBmYgKlZJyUEM1y9vchNx+AStsujbjw==;EndpointSuffix=core.windows.net",
                    "funtion-demobot-container"
                    ));
            //services.AddAzureClients(builder =>
            //{
            //    builder.AddSearchClient(Configuration.GetSection("SearchClient"));
            //});
            //var builder = Kernel.CreateBuilder()
            //    .AddAzureOpenAIChatCompletion(
            //        "milosLearnDeployment",                   // Azure OpenAI *Deployment Name*
            //        "https://miloslearn.openai.azure.com/",    // Azure OpenAI *Endpoint*
            //        "f579d75ecedd48b4a887368a70971284"          // Azure OpenAI *Key*
            //    );
            //Kernel kernel = builder.Build();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
