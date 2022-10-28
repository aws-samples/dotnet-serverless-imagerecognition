using ImageRecognition.BlazorWebAssembly;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static System.Net.WebRequestMethods;

namespace ImageRecognition.BlazorWebAssembly
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddOidcAuthentication(options =>
            {
                builder.Configuration.Bind("Cognito", options.ProviderOptions);
            });

            var appOptions = new AppOptions();
            builder.Configuration.Bind("AppOptions", appOptions);
            
            builder.Services.AddSingleton(appOptions);

            builder.Services.AddScoped<IServiceClientFactory, ServiceClientFactory>();
            builder.Services.AddScoped<ICommunicationClientFactory, CommunicationClientFactory>();
            builder.Services.AddScoped<IFileUploader, FileUploader>();

            await builder.Build().RunAsync();
        }
    }
}