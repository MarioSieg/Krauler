namespace Krauler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Logger.Instance.WriteLine("Krauler (c) Copyright Kevin Sieg, Mario Sieg 2021!");
            var jobQueue = new ParallelJobQueue();
            jobQueue.Enqueue<TestCrawler>();
            jobQueue.Dispatch(1000);
            jobQueue.Clear();

            Logger.Instance.Flush();
        }
    }
}