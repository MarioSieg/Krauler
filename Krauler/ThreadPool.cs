using System;
using System.Collections.Generic;
using System.Threading;

namespace Krauler
{
    public delegate void WorkerRoutine(ulong i);

    /// <summary>
    ///     Contains worker threads for parallel crawling.
    /// </summary>
    public sealed class ThreadPool
    {
        public static volatile bool InterruptFlag = true;

        public ThreadPool(WorkerRoutine routine, ulong maxIterations) : this(routine, maxIterations,
            Environment.ProcessorCount) { }

        public ThreadPool(WorkerRoutine routine, ulong maxIterations, int workers)
        {
            if (routine == null)
                throw new ArgumentNullException();

            if (maxIterations == 0 || workers == 0)
                throw new ArgumentException();

            Logger.Instance.WriteLine($"Native hardware threads: {Environment.ProcessorCount}");
            Logger.Instance.WriteLine($"Creating thread pool with {workers} workers!");

            var threads = new List<Thread>();
            for (var i = 0; i < workers; ++i)
            {
                var thread = new Thread(delegate()
                {
                    for (ulong k = 0; k < maxIterations && InterruptFlag; ++k) routine(k);
                });
                threads.Add(thread);
            }

            Threads = threads.ToArray();
        }

        public Thread[] Threads { get; }

        public void StartAll()
        {
            foreach (var thread in Threads) thread.Start();
        }

        public void JoinAll()
        {
            foreach (var thread in Threads) thread.Join();
        }

        public void EndAllSoft()
        {
            InterruptFlag = false;
        }
    }
}