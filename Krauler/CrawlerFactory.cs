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
        /// <typeparam name="T">The type to create.</typeparam>
        /// <typeparam name="TCDat"></typeparam>
        /// <param name="times">How often to call OnDispatch().</param>
        /// <returns>The task.</returns>
        public static Task ConstructAndDispatchAsync<T, TCDat>(ulong times = 1)
            where T : Crawler<TCDat>, new() where TCDat : class
        {
            return Task.Run(() =>
            {
                var crawler = new T();
                crawler.OnInitialize();
                for (ulong i = 0; i < times; ++i) crawler.OnDispatch();
                crawler.OnDestroy();
            });
        }

        /// <summary>
        ///     Dispatch multiple times.
        /// </summary>
        /// <typeparam name="T">The type to create.</typeparam>
        /// <typeparam name="TCDat"></typeparam>
        /// <param name="times">How often to call OnDispatch().</param>
        /// <param name="timeout">Timeout to sleep between each call.</param>
        /// <returns>The task.</returns>
        public static Task ConstructAndDispatchAsync<T, TCDat>(ulong times, TimeSpan timeout)
            where T : Crawler<TCDat>, new() where TCDat : class
        {
            return Task.Run(() =>
            {
                var crawler = new T();
                crawler.OnInitialize();
                for (ulong i = 0; i < times; ++i)
                {
                    crawler.OnDispatch();
                    Thread.Sleep(timeout);
                }

                crawler.OnDestroy();
            });
        }
    }
}