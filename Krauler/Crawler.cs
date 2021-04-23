using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Krauler
{
    /// <summary>
    ///     Base class for all crawlers.
    /// </summary>
    public abstract class Crawler<TData> where TData : class
    {
        protected delegate IEnumerable<TData>? Refiner(IEnumerable<TData> x);

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

        public List<TData> Results { get; } = new(128);

        /// <summary>
        ///     Name of the crawler.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        ///     Short description on what it crawls.
        /// </summary>
        public string Description { get; protected set; }

        public void DumpResults()
        {
            lock (Results)
            {
                foreach (var x in Results)
                    Logger.Instance.WriteLine(x.ToString() ?? "NONE");
            }
        }

        /// <summary>
        ///     Submit data for processing.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="x"></param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        protected async void SubmitData(Refiner task, IEnumerable<TData> x)
        {
            await Task.Run(() =>
            {
                var result = task(x);
                lock (Results)
                {
                    if (result != null)
                        foreach (var y in result)
                            Results.Add(y);
                }
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SubmitData(IEnumerable<TData> x)
        {
            SubmitData(DataProcessor, x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual IEnumerable<TData>? DataProcessor(IEnumerable<TData> rawText)
        {
            return null;
        }

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
        ///     Called when the crawler is created.
        /// </summary>
        public abstract void OnInitialize();

        /// <summary>
        ///     Called when the crawler should start crawling :D
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
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