using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Krauler
{
    public class YoutubeCrawler : ICrawler
    {
        private const int TabCount = 32;

        private ChromeDriver _chromeDriver;
        public string Name => "YoutubeCrawler";

        public string Description => "";

        public ServerHeader ServerHeader => new()
        {
            Uri = new Uri("https://www.youtube.com/watch?v=6MvcwAvZ1nY"),
            Locked = false
        };

        public void OnInitialize()
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

            // options.AddArgument("--headless");

            // Create tabs:
            for (var i = 0; i < TabCount; ++i) ((IJavaScriptExecutor) _chromeDriver).ExecuteScript("window.open();");

            // Goto url on each tab:
            foreach (var handle in _chromeDriver.WindowHandles)
            {
                _chromeDriver.SwitchTo().Window(handle);
                _chromeDriver.Navigate().GoToUrl(ServerHeader.Uri);
            }
        }

        public void OnDestroy()
        {
            _chromeDriver.Quit();
        }
    }
}