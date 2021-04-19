using System;
using System.Threading;
using System.Threading.Tasks;

namespace Krauler
{
    /// <summary>
    /// Base interface for all crawlers.
    /// </summary>
    public interface ICrawler
    {
        /// <summary>
        /// Crawl one time.
        /// </summary>
        /// <typeparam name="T">The type to create.</typeparam>
        /// <returns>The task.</returns>
        public static Task ConstructAndDispatchAsync<T>() where T: ICrawler, new()
        {
            return Task.Run(() =>
            {
                var crawler = new T();
                crawler.OnInitialize();
                crawler.OnDispatch();
                crawler.OnDestroy();
            });
        }

        /// <summary>
        /// Dispatch multiple times.
        /// </summary>
        /// <typeparam name="T">The type to create.</typeparam>
        /// <param name="times">How often to call OnDispatch().</param>
        /// <returns>The task.</returns>
        public static Task ConstructAndDispatchAsync<T>(ulong times) where T : ICrawler, new()
        {
            return Task.Run(() =>
            {
                var crawler = new T();
                crawler.OnInitialize();
                for (ulong i = 0; i < times; ++i)
                {
                    crawler.OnDispatch();
                }
                crawler.OnDestroy();
            });
        }

        /// <summary>
        /// Dispatch multiple times.
        /// </summary>
        /// <typeparam name="T">The type to create.</typeparam>
        /// <param name="times">How often to call OnDispatch().</param>
        /// <param name="timeout">Timeout to sleep between each call.</param>
        /// <returns>The task.</returns>
        public static Task ConstructAndDispatchAsync<T>(ulong times, TimeSpan timeout) where T : ICrawler, new()
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

        /// <summary>
        ///     Name of the crawler.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Short description on what it crawls.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Target address/server header.
        /// </summary>
        public ServerHeader ServerHeader { get; }

        /// <summary>
        ///     Called when the crawler is created.
        /// </summary>
        public void OnInitialize()
        {

        }

        /// <summary>
        ///     Called when the crawler should start crawling :D
        /// </summary>
        public void OnDispatch()
        {
        }

        /// <summary>
        ///     Called when the crawler is destroyed.
        /// </summary>
        public void OnDestroy()
        {

        }
    }
}