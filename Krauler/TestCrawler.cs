namespace Krauler
{
    internal class TestCrawler : ICrawler
    {
        public string Name => "Test Crawler";

        public string Description => "Test Crawler";

        public ServerHeader ServerHeader => ServerHeader.DefaultTargetNoProxy;

        public void OnCreate()
        {
            Logger.Instance.WriteLine($"Created crawler with name: {Name}");
        }

        public void Update(int i)
        {
            Logger.Instance.WriteLine($"Update {i} on crawler with name: {Name}");
        }

        public void OnShutdown()
        {
            Logger.Instance.WriteLine($"Destroyed crawler with name: {Name}");
        }
    }
}