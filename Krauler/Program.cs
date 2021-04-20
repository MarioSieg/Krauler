using System;
using System.IO;
using System.Threading.Tasks;
using Krauler.Crawlers;

namespace Krauler
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            // ToDo: differentiate windwos/linux/mac
            var curDir = Path.GetFullPath(Path.Combine(baseDirectory, @"../../../"));
            try
            {
                Directory.SetCurrentDirectory(curDir);
                Logger.Instance.WriteLine("Krauler (c) Copyright Kevin Sieg, Mario Sieg 2021!");

                await Crawler.ConstructAndDispatchAsync<YoutubeCrawler>();
            }
            catch (Exception e)
            {
                Logger.Instance.Write(e);
                Logger.Instance.Flush();
            }
        }
    }
}