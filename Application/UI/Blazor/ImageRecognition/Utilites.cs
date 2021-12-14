using System.IO;

namespace ImageRecognition.BlazorFrontend
{
    public static class Utilites
    {
        /// <summary>
        ///     This method is different then Path.GetFileNameWithoutExtension in that it works for file paths for either Windows
        ///     or Non-Windows.
        ///     Otherwise when the Blazor application runs in the Linux container but gets a filepath from a browser running on
        ///     windows it will
        ///     not handle the slashes correctly.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFileNameWithoutExtension(string file)
        {
            file = file.Replace("\\", "/");
            var pos = file.LastIndexOf("/");

            if (pos != -1) file = file.Substring(pos + 1);


            return Path.GetFileNameWithoutExtension(file);
        }
    }
}