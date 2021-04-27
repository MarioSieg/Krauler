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
    public abstract class Crawler<TRawData, TResult> : ICrawler where TRawData : struct where TResult : struct
    {
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

        public List<TResult> Results { get; } = new(128);

        /// <summary>
        ///     Name of the crawler.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        ///     Short description on what it crawls.
        /// </summary>
        public string Description { get; protected set; }

        public abstract void OnInitialize();

        public abstract void OnDispatch();

        public abstract void OnDestroy();

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SubmitData(IEnumerable<TRawData>? inputDataList, bool createClonedData = true)
        {
            if (inputDataList == null || !inputDataList.Any())
            {
                Logger.Instance.Write("Input data list is empty or null!", LogLevel.Warning);
                return;
            }

            IEnumerable<TRawData>? clonedDataList = createClonedData ? inputDataList.ToHashSet() : null;
            SubmitData(DataProcessor, createClonedData ? clonedDataList : inputDataList);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual IEnumerable<TResult>? DataProcessor(IEnumerable<TRawData>? rawData)
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

        protected delegate IEnumerable<TResult>? Refiner(IEnumerable<TRawData>? x);
    }
}