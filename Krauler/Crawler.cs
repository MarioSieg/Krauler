using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Krauler
{
    /// <summary>
    ///     Base class for all crawlers.
    /// </summary>
    public abstract class Crawler
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

        private string? _childName;
        private object? _config;

        /// <summary>
        ///     Construct with constant data.
        /// </summary>
        /// <param name="name">Name of the crawler.</param>
        /// <param name="description">Short description on what it crawls.</param>
        /// <param name="url"></param>
        protected Crawler(string name, string description)
        {
            Name = name;
            Description = description;
        }

        /// <summary>
        ///     Name of the crawler.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        ///     Short description on what it crawls.
        /// </summary>
        public string Description { get; protected set; }

        /// <summary>
        ///     Initialize config type and try to load the file.
        /// </summary>
        /// <typeparam name="TSelf"></typeparam>
        /// <typeparam name="TConfig"></typeparam>
        /// <returns></returns>
        protected TConfig InitializeConfig<TSelf, TConfig>(TConfig? manual = null) where TConfig : BaseConfig, new()
        {
            _childName = typeof(TSelf).Name;
            var cfg = manual ?? DeserializeConfig<TConfig>();
            _config = cfg;
            return cfg;
        }

        protected TConfig? GetConfig<TConfig>()
        {
            return (TConfig?) _config;
        }

        /// <summary>
        ///     Dispatch multiple times.
        /// </summary>
        /// <typeparam name="T">The type to create.</typeparam>
        /// <param name="times">How often to call OnDispatch().</param>
        /// <returns>The task.</returns>
        public static Task ConstructAndDispatchAsync<T>(ulong times = 1) where T : Crawler, new()
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
        public virtual void OnDispatch() { }

        /// <summary>
        ///     Called when the crawler is destroyed.
        /// </summary>
        public abstract void OnDestroy();

        public void SerializeConfig<T>(in T? data)
        {
            try
            {
                Config.Serialize(data, _childName);
            }
            catch
            {
                Logger.Instance.WriteLine($"Failed to serialize config for object {_childName}", LogLevel.Warning);
            }
        }

        public T DeserializeConfig<T>() where T : new()
        {
            try
            {
                var cfg = Config.Deserialize<T>(_childName);
                if (cfg != null) return cfg;
                cfg = new T();
                SerializeConfig(cfg);
                return cfg;
            }
            catch
            {
                Logger.Instance.WriteLine($"Failed to serialize config for object {_childName}", LogLevel.Warning);
                var cfg = new T();
                SerializeConfig(cfg);
                return cfg;
            }
        }
    }
}