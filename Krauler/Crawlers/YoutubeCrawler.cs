using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Krauler.Crawlers
{
    public sealed class YoutubeCrawlerConfig : BaseConfig
    {
        public bool ChromeDriverHideCommandPromptWindow = true;
        
        public bool UseProxy = false;

        public readonly bool SetUserAgent = true;
        
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
        private IWebDriver? _driver;

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
                var proxy = Proxies.Value[rand.Next(Proxies.Value.Length)];
                options.AddArgument("--proxy-server=http://" + proxy);
            }
            if (_config.SetUserAgent)
            {
                var rand = new Random();
                var userAgent = UserAgents.Value[rand.Next(UserAgents.Value.Length)];
                options.AddArgument("--user-agent=" + userAgent);
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
            if (_driver?.WindowHandles == null) throw new ArgumentNullException();

            {
                uint i = 0;
                foreach (var handle in _driver?.WindowHandles!)
                {
                    _driver.SwitchTo().Window(handle);
                    _driver.Navigate().GoToUrl(_config.ServerHeader.Uri);
                    Logger.Instance.WriteLine($"Opening URL {++i}");
                }
            }

            // Confirm google usage
            if (_driver.FindElementSafe(By.TagName("button")) != null)
            {
                GoogleUsageConfirmer(By.TagName("button"));
            }
            if (_driver.FindElementSafe(By.ClassName("button")) != null)
            {
                GoogleUsageConfirmer(By.XPath("//form/input[@class='button']"));
            }

            // Confirm youtube usage
            YoutubeUsageConfirmer();
        }

        private void YoutubeUsageConfirmer()
        {
            uint i = 0;
            var wait = new WebDriverWait(_driver, new TimeSpan(0, 0, 30));
            if (_driver?.WindowHandles == null) throw new NullReferenceException();
            foreach (var handle in _driver.WindowHandles)
            {
                try
                {
                    _driver.SwitchTo().Window(handle);
                    _driver.FindElement(By.Id("dismiss-button")).Click();
                    wait.Until(_ => ((IJavaScriptExecutor)_driver).ExecuteScript("return document.readyState").Equals("complete"));
                    _driver.FindElement(By.Id("player-container")).Click();
                    Logger.Instance.WriteLine($"Confirmed google usage {++i}");
                }
                catch (Exception ex)
                {
                    Logger.Instance.Write(ex);
                }
            }
        }

        private void GoogleUsageConfirmer(By tagName)
        {
            uint i = 0;
            if (_driver?.WindowHandles == null) throw new NullReferenceException();
            foreach (var handle in _driver.WindowHandles)
            {
                try
                {
                    _driver.SwitchTo().Window(handle);
                    _driver.FindElement(tagName).Click();
                    Logger.Instance.WriteLine($"Confirmed google usage {++i}");
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