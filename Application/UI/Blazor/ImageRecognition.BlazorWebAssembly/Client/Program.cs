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
                // known issue with AppSettings.json file. https://github.com/dotnet/aspnetcore/issues/44007
                //builder.Configuration.Bind("Cognito", options.ProviderOptions);

                options.ProviderOptions.Authority = "https://i7xv17y03l.execute-api.us-west-1.amazonaws.com/";
                options.ProviderOptions.ClientId = "522gdjc852ncq6mihvdmrp77ne";
                options.ProviderOptions.RedirectUri = "https://d2ocsceqsz4y4r.cloudfront.net/login-callback";
                options.ProviderOptions.PostLogoutRedirectUri = "https://d2ocsceqsz4y4r.cloudfront.net/logout-callback";
                options.ProviderOptions.ResponseType = "id_token";
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