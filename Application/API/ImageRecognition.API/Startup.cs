using System;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.Util;
using ImageRecognition.API.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImageRecognition.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ConfigureDynamoDB();
        }

        public IConfiguration Configuration { get; }

        private void ConfigureDynamoDB()
        {
            string value;
            if ((value = Configuration["AppOptions:TableAlbum"]) != null)
                AWSConfigsDynamoDB.Context.AddMapping(new TypeMapping(typeof(Album), value));
            if ((value = Configuration["AppOptions:TablePhoto"]) != null)
                AWSConfigsDynamoDB.Context.AddMapping(new TypeMapping(typeof(Photo), value));
        }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppOptions>(Configuration.GetSection("AppOptions"));

            services.AddAWSService<IAmazonDynamoDB>();
            services.AddAWSService<IAmazonS3>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/",
                    async context =>
                    {
                        await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
                    });
            });
        }
    }
}