using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Krauler.Crawlers
{
    public sealed class YoutubeCrawlerConfig : BaseConfig
    {
        public bool ChromeDriverHideCommandPromptWindow = true;

        public List<string> ChromeOptions = new()
        {
            "--window-size=800,600",
            "--no-sandbox",
            "--disable-gpu",
            "--disable-crash-reporter",
            "--disable-extensions",
            "--disable-in-process-stack-traces",
            "--disable-logging",
            "--disable-dev-shm-usage",
            "--log-level=3",
            "--output=/dev/null",
            "--silent"
        };

        public PageLoadStrategy PageLoadStrategy = PageLoadStrategy.Normal;
        public byte TabCount = 10;

        public YoutubeCrawlerConfig() : base("https://www.youtube.com/watch?v=6MvcwAvZ1nY") { }
    }

    public class YoutubeCrawler : Crawler
    {
        private readonly YoutubeCrawlerConfig _config;
        private ChromeDriver? _chromeDriver;

        public YoutubeCrawler() : base("YoutubeCrawler", "")
        {
            _config = InitializeConfig<YoutubeCrawler, YoutubeCrawlerConfig>();
        }

        public override void OnInitialize()
        {
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = _config.ChromeDriverHideCommandPromptWindow;
            var options = new ChromeOptions {PageLoadStrategy = _config.PageLoadStrategy};
            options.AddArguments(_config.ChromeOptions);
            _chromeDriver = new ChromeDriver(service, options);
        }

        public override void OnDispatch()
        {
            // Create tabs:
            for (var i = 0; i < _config.TabCount - 1; ++i)
                _ = ((IJavaScriptExecutor?) _chromeDriver)?.ExecuteScript("window.open();");

            // Goto url on each tab:
            foreach (var handle in _chromeDriver?.WindowHandles)
            {
                _chromeDriver.SwitchTo().Window(handle);
                _chromeDriver.Navigate().GoToUrl(ServerHeader.Uri);
            }

            foreach (var handle in _chromeDriver.WindowHandles)
                try
                {
                    _chromeDriver.SwitchTo().Window(handle);
                    _chromeDriver.FindElementByTagName("button").Click();
                }
                catch (Exception ex)
                {
                    Logger.Instance.Write(ex);
                }
        }

        public override void OnDestroy()
        {
            _chromeDriver?.Quit();
        }
    }
}