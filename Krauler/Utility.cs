using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Krauler
{
    internal static class Utility
    {
        private const byte CheckStages = 10;

        /// <summary>
        ///     Regex to extract urls
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

        public static string ValidateFilePathOrCreateNew(string file)
        {
            try
            {
                var info = new FileInfo(file);
                using FileStream stream = info.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                stream.Close();
                return file;
            }
            catch (IOException)
            {
                var time = DateTime.Now;
                return
                    $"{file}.{time.ToShortDateString().Replace('/', '-')}-{time.TimeOfDay.Hours}-{time.TimeOfDay.Minutes}-{time.TimeOfDay.Seconds}.bak";
            }
        }

        internal static string? GetRandomUserAgent()
        {
            var rand = new Random();
            var userAgent =
                CrawlerFactory.UserAgents.Value[
                    rand.Next(CrawlerFactory.UserAgents.Value.Length)];
            return userAgent;
        }
        
        internal static string? GetRandomProxy()
        {
            var rand = new Random();
            var proxy =
                CrawlerFactory.Proxies.Value[
                    rand.Next(CrawlerFactory.Proxies.Value.Length)];
            return proxy;
        }
    }
}