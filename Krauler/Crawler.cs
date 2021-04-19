using System;
using System.Threading;
using System.Threading.Tasks;

namespace Krauler
{
    /// <summary>
    ///     Base interface for all crawlers.
    /// </summary>
    public abstract class Crawler
    {
        /// <summary>
        ///     Name of the crawler.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        ///     Short description on what it crawls.
        /// </summary>
        public string Description { get; protected set; }

        /// <summary>
        ///     Target address/server header.
        /// </summary>
        public ServerHeader ServerHeader { get; protected set; }

        /// <summary>
        /// Construct with constant data.
        /// </summary>
        /// <param name="name">Name of the crawler.</param>
        /// <param name="description">Short description on what it crawls.</param>
        /// <param name="serverHeader">Target address/server header.</param>
        protected Crawler(string name, string description, in ServerHeader serverHeader)
        {
            Name = name;
            Description = description;
            ServerHeader = serverHeader;
        }

        /// <summary>
        ///     Crawl one time.
        /// </summary>
        /// <typeparam name="T">The type to create.</typeparam>
        /// <returns>The task.</returns>
        public static Task ConstructAndDispatchAsync<T>() where T : Crawler, new()
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
        ///     Dispatch multiple times.
        /// </summary>
        /// <typeparam name="T">The type to create.</typeparam>
        /// <param name="times">How often to call OnDispatch().</param>
        /// <returns>The task.</returns>
        public static Task ConstructAndDispatchAsync<T>(ulong times) where T : Crawler, new()
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
        /// <param name="times">How often to call OnDispatch().</param>
        /// <param name="timeout">Timeout to sleep between each call.</param>
        /// <returns>The task.</returns>
        public static Task ConstructAndDispatchAsync<T>(ulong times, TimeSpan timeout) where T : Crawler, new()
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
        ///     Called when the crawler is created.
        /// </summary>
        public abstract void OnInitialize();

        /// <summary>
        ///     Called when the crawler should start crawling :D
        /// </summary>
        public virtual void OnDispatch()
        {

        }

        /// <summary>
        ///     Called when the crawler is destroyed.
        /// </summary>
        public abstract void OnDestroy();
    }
}