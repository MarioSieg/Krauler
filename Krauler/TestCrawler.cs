namespace Krauler
{
    class TestCrawler : ICrawler
    {
        public string Name => "Test Crawler";

        public string Description => "Test Crawler";

        public ServerHeader ServerHeader => ServerHeader.DefaultTargetNoProxy;

        public void OnCreate()
        {
            Logger.Instance.WriteLine($"Created Crawler with Name: {Name}");
        }

        public void Update()
        {

        }

        public void OnShutdown()
        {

        }
    }
}
