using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Krauler.Crawlers
{
    public sealed class YoutubeCrawlerConfig : BaseConfig
    {
        public bool ChromeDriverHideCommandPromptWindow = true;
        
        public bool UseProxy = true;
        public bool SetUserAgent = false;
        
        public readonly List<string> DefaultChromeOptions = new()
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
        public byte TabCount = 3;
        

        public YoutubeCrawlerConfig() : base("https://www.youtube.com/watch?v=6MvcwAvZ1nY") { }
    }

    public class YoutubeCrawler : Crawler
    {
        private readonly YoutubeCrawlerConfig _config;
        private ChromeDriver? _driver;

        public YoutubeCrawler() : base("YoutubeCrawler", "")
        {
            _config = InitializeConfig<YoutubeCrawler, YoutubeCrawlerConfig>();
        }

        public override void OnInitialize()
        {
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = _config.ChromeDriverHideCommandPromptWindow;
            
            var options = new ChromeOptions {PageLoadStrategy = _config.PageLoadStrategy};
            options.AddArguments(_config.DefaultChromeOptions);
            
            if (_config.UseProxy)
            {
                var rand = new Random();
                var proxy = Proxy.Value[rand.Next(Proxy.Value.Count)];
                options.AddArgument("--proxy-server=http://" + proxy);
            }
            if (_config.SetUserAgent)
            {
                options.AddArgument("--user-agent=Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");
            }
            _driver = new ChromeDriver(service, options);
            
            _driver.Manage().Cookies.DeleteAllCookies();
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        }

        public override void OnDispatch()
        {
            // Create tabs:
            for (var i = 0; i < _config.TabCount - 1; ++i)
            {
                _ = ((IJavaScriptExecutor?) _driver)?.ExecuteScript("window.open();");
                Logger.Instance.WriteLine($"Creating tab: {i + 1}");
            }

            // Goto url on each tab:
            if (_driver?.WindowHandles == null) throw new ArgumentNullException("WindowHandles is null");

            uint j = 0;
            foreach (var handle in _driver?.WindowHandles!)
            {
                _driver.SwitchTo().Window(handle);
                _driver.Navigate().GoToUrl(_config.ServerHeader.Uri);
                Logger.Instance.WriteLine($"Opening URL {++j}");
            }

            // Confirm google usage
            foreach (var handle in _driver.WindowHandles)
            {
                try
                {
                    _driver.SwitchTo().Window(handle);
                    _driver.FindElementByTagName("button").Click();
                    Logger.Instance.WriteLine($"Confirmed google usage {++j}");
                }
                catch (Exception ex)
                {
                    Logger.Instance.Write(ex);
                }
            }


            var wait = new WebDriverWait(_driver, new TimeSpan(0, 0, 30));

            j = 0;

            // Confirm youtube usage
            foreach (var handle in _driver.WindowHandles)
            {
                try
                {
                    _driver.SwitchTo().Window(handle);
                    //var quit = _chromeDriver.FindElementById("dismiss-button").Displayed;
                    _driver.FindElementById("dismiss-button").Click();
                    wait.Until(_ => ((IJavaScriptExecutor)_driver).ExecuteScript("return document.readyState").Equals("complete"));
                    _driver.FindElementById("player-container").Click();
                    Logger.Instance.WriteLine($"Confirmed google usage {++j}");
                }
                catch (Exception ex)
                {
                    Logger.Instance.Write(ex);
                }
            }
        }

        public override void OnDestroy()
        {
            //Thread.Sleep(20000);
            //_chromeDriver?.Quit();
        }
    }
}