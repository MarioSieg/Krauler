using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable PossibleMultipleEnumeration

namespace Krauler
{
    /// <summary>
    ///     Base class for all crawlers.
    /// </summary>
    public abstract class Crawler<TRawData, TResult, TSelf, TConfig> : CrawlerEvents where TRawData : struct where TResult : struct where TConfig: BaseConfig, new() where TSelf: Crawler<TRawData, TResult, TSelf, TConfig>
    {
        private string? _childName;
        protected TConfig Config;

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
            Config = InitializeConfig();
            OnInitializeEvent += InstallDriverForInstance;
        }

        private void InstallDriverForInstance()
        {
            WebDriverInstaller.InstallDriver(Config.WebDriverType);
        }

        public List<TResult> Results { get; } = new(128);

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
                {
                    Logger.Instance.WriteLine(x.ToString() ?? "NONE");
                }
            }
        }

        /// <summary>
        ///     Submit data for processing.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="x"></param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        protected async void SubmitData(Refiner task, IEnumerable<TRawData>? x)
        {
            try
            {
                await Task.Run(() =>
                {
                    var result = task(x);
                    lock (Results)
                    {
                        if (result == null)
                            return;
                        foreach (var y in result)
                            Results.Add(y);
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Instance.Write($"Submit data failed: {ex}", LogLevel.Error);
            }
        }

        protected void SubmitData(IEnumerable<TRawData>? inputDataList, bool createClonedData = true)
        {
            Span<DateTime> timings = stackalloc DateTime[2];

            if (inputDataList == null || !inputDataList.Any())
            {
                Logger.Instance.Write("Input data list is empty or null!", LogLevel.Warning);
                return;
            }

            IEnumerable<TRawData>? clonedDataList = createClonedData ? inputDataList.ToHashSet() : null;

            timings[0] = DateTime.Now;
            SubmitData(DataProcessor, createClonedData ? clonedDataList : inputDataList);
            timings[1] = DateTime.Now;

#if DEBUG || VERBOSE
            Logger.Instance.WriteLine($"$Crawler execution timing: 0: {timings[0]}, 1: {timings[1]}");
#endif
        }

        /// <summary>
        /// method that processes the data, to be overwritten in the crawler
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual IEnumerable<TResult>? DataProcessor(IEnumerable<TRawData>? rawData)
        {
            return null;
        }

        /// <summary>
        /// Initialize config type and try to load the file.
        /// </summary>
        /// <typeparam name="TSelf"></typeparam>
        /// <typeparam name="TConfig"></typeparam>
        /// <returns></returns>
        protected TConfig InitializeConfig(TConfig? manual = null)
        {
            _childName = typeof(TSelf).Name;
            var cfg = manual ?? DeserializeConfig<TConfig>();
            Config = cfg;
            return cfg;
        }

        public void SerializeConfig<T>(in T? data)
        {
            try
            {
                Krauler.Config.Serialize(data, _childName);
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
                var cfg = Krauler.Config.Deserialize<T>(_childName);
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

        protected delegate IEnumerable<TResult>? Refiner(IEnumerable<TRawData>? x);
    }
}