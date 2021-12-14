using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CognitoLogin
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            
            var host = CreateHostBuilder(args).Build();

            var loginProcessor = ActivatorUtilities.CreateInstance<LoginProcessor>(host.Services);
            await loginProcessor.ExecuteAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddSystemsManager("/ImageRecognition");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<UserPoolOptions>(hostContext.Configuration.GetSection("AWS"));
                });
    }
}
