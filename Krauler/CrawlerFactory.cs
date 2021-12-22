using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Krauler
{
    public static class CrawlerFactory
    {
        public static readonly Lazy<string[]> Proxies = new(() =>
        {
            var list = File.ReadAllLines(Config.ResourcesDir + "ProxyList.txt");
            return list.Length != 0 ? list : throw new NullReferenceException();
        });

        public static readonly Lazy<string[]> UserAgents = new(() =>
        {
            var list = File.ReadAllLines(Config.ResourcesDir + "UserAgents.txt");
            return list.Length != 0 ? list : throw new NullReferenceException();
        });

        /// <summary>
        ///     Dispatch multiple times.
        /// </summary>
        /// <typeparam name="TCrawler">The type to create.</typeparam>
        /// <param name="times">How often to call OnDispatch().</param>
        /// <returns>The task.</returns>
        public static Task ConstructAndDispatchAsync<TCrawler>(ulong times = 1)
            where TCrawler : CrawlerEvents, new()
        {
            return Task.Run(() =>
            {
                var crawler = new TCrawler();
                crawler.DispatchOnInitialize();
                for (ulong i = 0; i < times; ++i) crawler.DispatchOnDispatch();
                crawler.DispatchOnDestroy();
            });
        }

        /// <summary>
        ///     Dispatch multiple times.
        /// </summary>
        /// <typeparam name="TCrawler">The type to create.</typeparam>
        /// <param name="times">How often to call OnDispatch().</param>
        /// <param name="timeout">Timeout to sleep between each call.</param>
        /// <returns>The task.</returns>
        public static Task ConstructAndDispatchAsync<TCrawler>(ulong times, TimeSpan timeout)
            where TCrawler : CrawlerEvents, new()
        {
            return Task.Run(() =>
            {
                var crawler = new TCrawler();
                crawler.DispatchOnInitialize();
                for (ulong i = 0; i < times; ++i)
                {
                    crawler.DispatchOnDispatch();
                    Thread.Sleep(timeout);
                }

                crawler.DispatchOnDestroy();
            });
        }
    }
}