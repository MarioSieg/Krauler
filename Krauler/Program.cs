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
                Logger.Instance.WriteLine($"Loaded {Crawler.Proxies.Value.Length} proxies!");
                Logger.Instance.WriteLine($"Loaded {Crawler.UserAgents.Value.Length} user agents!");
                await Crawler.ConstructAndDispatchAsync<GoogleCrawler>();
            }
            catch (Exception e)
            {
                Logger.Instance.Write(e);
                Logger.Instance.Flush();
            }
        }
    }
}