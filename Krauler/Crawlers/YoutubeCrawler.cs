using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Krauler.Crawlers
{
    public class YoutubeCrawler : Crawler
    {
        private readonly int _tabCount = 10;
        private ChromeDriver _chromeDriver;

        public YoutubeCrawler() : base("YoutubeCrawler", "", new ServerHeader
        {
            Uri = new Uri("https://www.youtube.com/watch?v=6MvcwAvZ1nY"),
            Locked = false
        })
        {
        }

        public override void OnInitialize()
        {
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            var options = new ChromeOptions {PageLoadStrategy = PageLoadStrategy.Normal};
            options.AddArgument("--window-size=800,600");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-crash-reporter");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-in-process-stack-traces");
            options.AddArgument("--disable-logging");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--log-level=3");
            options.AddArgument("--output=/dev/null");
            options.AddArgument("--silent");
            _chromeDriver = new ChromeDriver(service, options);
        }

        public override void OnDispatch()
        {
            // Create tabs:
            for (var i = 0; i < _tabCount; ++i) ((IJavaScriptExecutor) _chromeDriver).ExecuteScript("window.open();");

            // Goto url on each tab:
            foreach (var handle in _chromeDriver.WindowHandles)
            {
                _chromeDriver.SwitchTo().Window(handle);
                _chromeDriver.Navigate().GoToUrl(ServerHeader.Uri);
            }

            foreach (var handle in _chromeDriver.WindowHandles)
            {
                _chromeDriver.SwitchTo().Window(handle);
                _chromeDriver.FindElementByTagName("button").Click();
            }
        }

        public override void OnDestroy()
        {
            _chromeDriver.Quit();
        }
    }
}