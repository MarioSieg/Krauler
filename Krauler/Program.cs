using System;
using System.IO;

namespace Krauler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Directory.SetCurrentDirectory("../../../");
                Logger.Instance.WriteLine("Krauler (c) Copyright Kevin Sieg, Mario Sieg 2021!");
                var jobQueue = new ParallelJobQueue();
                jobQueue.Enqueue<YoutubeCrawler>();
                jobQueue.InitializeAllCrawlers();
                jobQueue.Dispatch(1);
                jobQueue.DestroyAll();
            }
            catch(Exception e)
            {
                Logger.Instance.WriteLine(e.Message, LogLevel.Error);
                Logger.Instance.Flush();
            }
        }
    }
}