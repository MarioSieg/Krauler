using System;
using System.IO;
using System.Threading.Tasks;
using Krauler.Crawlers;

namespace Krauler
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            try
            {
                Directory.SetCurrentDirectory("../../../");
                Logger.Instance.WriteLine("Krauler (c) Copyright Kevin Sieg, Mario Sieg 2021!");

                await ICrawler.ConstructAndDispatchAsync<YoutubeCrawler>();
            }
            catch (Exception e)
            {
                Logger.Instance.WriteLine(e.Message, LogLevel.Error);
                Logger.Instance.Flush();
            }
        }
    }
}