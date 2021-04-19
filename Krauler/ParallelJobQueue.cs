using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Krauler
{
    /// <summary>
    ///     Job queue which can be processed in parallel.
    /// </summary>
    public sealed class ParallelJobQueue
    {
        public ParallelJobQueue()
        {
        }

        public ParallelJobQueue(List<ICrawler> crawlers)
        {
            EnqueuedCrawlers = crawlers;
        }

        public List<ICrawler> EnqueuedCrawlers { get; } = new();

        public ICrawler this[int idx]
        {
            get => EnqueuedCrawlers[idx];
            set => EnqueuedCrawlers[idx] = value;
        }

        public void Enqueue<T>() where T : ICrawler, new()
        {
            var crawler = new T();
            crawler.OnCreate();
            EnqueuedCrawlers.Add(crawler);
        }

        public void Dispatch(int times)
        {
            Parallel.For(0, times, new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            }, i => { EnqueuedCrawlers[i % EnqueuedCrawlers.Count].Update(i); });
        }

        public bool Dequeue<T>(T t) where T : ICrawler, new()
        {
            if (!EnqueuedCrawlers.Contains(t)) return false;

            var crawler = EnqueuedCrawlers[EnqueuedCrawlers.IndexOf(t)];
            crawler.OnShutdown();
            return EnqueuedCrawlers.Remove(crawler);
        }

        public void Clear()
        {
            EnqueuedCrawlers.Clear();
        }
    }
}