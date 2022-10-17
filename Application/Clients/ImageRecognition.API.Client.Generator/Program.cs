using NSwag;
using NSwag.CodeGeneration.CSharp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ImageRecognition.API.Client.Generator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var document = await OpenApiDocument.FromUrlAsync("https://localhost:44381/swagger/v1/swagger.json");

            var settings = new CSharpClientGeneratorSettings
            {                
                CSharpGeneratorSettings =
                {
                    Namespace = "ImageRecognition.API.Client",
                }
            };

            var generator = new CSharpClientGenerator(document, settings);
            var code = generator.GenerateFile();
            var fullPath = DetermienFullFilePath("ImageRecognitionClient.cs");
            File.WriteAllText(fullPath, code);            
        }

        static string DetermienFullFilePath(string codeFile)
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());

            while(!string.Equals(dir.Name, "Clients"))
            {
                dir = dir.Parent;
            }

            return Path.Combine(dir.FullName, "ImageRecognition.API.Client", codeFile);
        }
    }
}
