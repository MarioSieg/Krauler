namespace Krauler.Crawlers
{
    internal class TestCrawler : ICrawler
    {
        public string Name => "Test Crawler";

        public string Description => "Test Crawler";

        public ServerHeader ServerHeader => ServerHeader.DefaultTargetNoProxy;

        public void OnInitialize()
        {
            Logger.Instance.WriteLine($"Created crawler with name: {Name}");
        }

        public void OnDispatch(int i)
        {
            Logger.Instance.WriteLine($"Update {i} on crawler with name: {Name}");
        }

        public void OnDestroy()
        {
            Logger.Instance.WriteLine($"Destroyed crawler with name: {Name}");
        }
    }
}