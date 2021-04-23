using System;
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
                Logger.Instance.WriteLine("Krauler (c) Copyright Kevin Sieg, Mario Sieg 2021!");
                Utility.SetCorrectWorkingDir();
                Logger.Instance.WriteLine($"Loaded {CrawlerFactory.Proxies.Value.Length} proxies!");
                Logger.Instance.WriteLine($"Loaded {CrawlerFactory.UserAgents.Value.Length} user agents!");
                await CrawlerFactory.ConstructAndDispatchAsync<GoogleCrawler, string>();
            }
            catch (Exception e)
            {
                Logger.Instance.Write(e);
                Logger.Instance.Flush();
            }
        }
    }
}