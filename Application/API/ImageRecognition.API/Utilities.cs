using System.IO;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ImageRecognition.API
{
    public static class Utilities
    {
        public static string GetUsername(ClaimsPrincipal user)
        {
            var claim = user.FindFirst("cognito:username");
            return claim?.Value;
        }


        public static async Task CopyStreamAsync(string input, Stream output)
        {
            using (var client = new HttpClient())
            using (var stream = await client.GetStreamAsync(input))
            {
                CopyStream(stream, output);
            }
        }

        public static void CopyStream(Stream input, Stream output)
        {
            var buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0) output.Write(buffer, 0, len);
        }

        public static string MakeSafeName(string displayName, int maxSize)
        {
            var builder = new StringBuilder();
            foreach (var c in displayName)
                if (char.IsLetterOrDigit(c))
                    builder.Append(c);
                else
                    builder.Append('-');

            var name = builder.ToString();

            if (maxSize < name.Length) name = name.Substring(0, maxSize);

            return name;
        }
    }
}