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
            try
            {
                Directory.SetCurrentDirectory("../../../");
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