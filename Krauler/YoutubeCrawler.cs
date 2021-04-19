using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Krauler
{
    public class YoutubeCrawler : ICrawler
    {
        public string Name => "YoutubeCrawler";

        public string Description => "";

        public ServerHeader ServerHeader => ServerHeader.DefaultTargetNoProxy;
        private List<ChromeDriver> _chromeDriver;
        private const string ResourceFolder = "resources/";

        public void OnInitialize()
        {
            Logger.Instance.WriteLine($"Created crawler with name: {Name}");
            this._chromeDriver = new List<ChromeDriver>();
            var options = new ChromeOptions();
            options.PageLoadStrategy = PageLoadStrategy.Normal;

            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--no-sandbox");
            //options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-crash-reporter");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-in-process-stack-traces");
            options.AddArgument("--disable-logging");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--log-level=3");
            options.AddArgument("--output=/dev/null");
            options.AddArgument("--silent");
            for (int i = 0; i < 20; ++i)
            {
                var chrome = new ChromeDriver(ResourceFolder, options);
                chrome.Navigate().GoToUrl("https://www.youtube.com/watch?v=6MvcwAvZ1nY");
                _chromeDriver.Add(chrome);
            }
        }

        public void OnDispatch(int i)
        {
            Logger.Instance.WriteLine($"Update {i} on crawler with name: {Name}");
        }

        public void OnDestroy()
        {
            foreach (var chrome in this._chromeDriver)
            {
                chrome.Close();
            }

            _chromeDriver.Clear();
            
            Logger.Instance.WriteLine($"Destroyed crawler with name: {Name}");
        }
    }
}