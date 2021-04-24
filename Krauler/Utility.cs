using System.IO;
using System.Text.RegularExpressions;

namespace Krauler
{
    internal static class Utility
    {
        private const byte CheckStages = 10;
        
        /// <summary>
        /// Regex to extract urls
        /// </summary>
        public static readonly Regex LinkParser =
            new(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static void SetCorrectWorkingDir()
        {
            if (Directory.Exists(Config.ResourcesDir)) return;

            string temp = "../";
            for (byte i = 0; i < CheckStages; ++i)
            {
                temp += "../";
                var dir = temp + Config.ResourcesDir;
                if (Directory.Exists(dir))
                {
                    Directory.SetCurrentDirectory(temp);
                    Logger.Instance.WriteLine($"Current working directory: {Directory.GetCurrentDirectory()}");
                    break;
                }
            }
        }
    }
}