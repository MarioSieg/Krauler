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
        public ParallelJobQueue() { }

        public ParallelJobQueue(List<Action> crawlers)
        {
            EnqueuedCrawlers = crawlers;
        }

        public List<Action> EnqueuedCrawlers { get; } = new();

        public Action this[int idx]
        {
            get => EnqueuedCrawlers[idx];
            set => EnqueuedCrawlers[idx] = value;
        }

        public void Enqueue(Action action)
        {
            EnqueuedCrawlers.Add(action);
        }

        public void Dispatch()
        {
            Parallel.ForEach(EnqueuedCrawlers, x => { x(); }
            );
        }

        public void Clear()
        {
            EnqueuedCrawlers.Clear();
        }
    }
}