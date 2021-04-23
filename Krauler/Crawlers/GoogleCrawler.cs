using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V85.IndexedDB;
using OpenQA.Selenium.Support.UI;

namespace Krauler.Crawlers
{
    public sealed class GoogleCrawlerConfig : BaseConfig
    {
        public bool ChromeDriverHideCommandPromptWindow = true;
        
        public bool UseProxy = false;

        public readonly bool SetUserAgent = false;
        
        public readonly List<string> DefaultChromeOptions = new()
        {
            //"--window-size=800,600",
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

        public byte TabCount = 2;

        public GoogleCrawlerConfig() : base("http://google.com") { }
    }

    public class GoogleCrawler : Crawler
    {
        private readonly GoogleCrawlerConfig _config;
        private IWebDriver? _driver;

        public GoogleCrawler() : base("GoogleCrawler", "")
        {
            _config = InitializeConfig<GoogleCrawler, GoogleCrawlerConfig>();
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
            Debug.Assert(_driver != null, nameof(_driver) + " != null");

            for (uint i = 1; i < 5; ++i)
            {
                _driver.Navigate().GoToUrl($"http://google.com/search?q=KevinKlang&start={(i - 1)*10}");

                if (_driver.FindElementSafe(By.TagName("button")) != null)
                {
                    GoogleUsageConfirmer(By.TagName("button"));
                }
                if (_driver.FindElementSafe(By.Id("zV9nZe")) != null)
                {
                    GoogleUsageConfirmer(By.XPath("//button[@id='zV9nZe']"));
                }
            
                IWebElement element = _driver.FindElement(By.XPath("//input[@name='q']"));
            
                //element.SendKeys("Kevin Klang");
                //element.SendKeys(Keys.Enter);

                IWebElement resultsPanel = _driver.FindElement(By.Id("search"));
                
                ReadOnlyCollection<IWebElement> searchResults = resultsPanel.FindElements(By.XPath(".//a"));
                List<string> data = new(searchResults.Count);
                foreach (var x in searchResults)
                {
                    data.Add(x.Text);
                }
                SubmitData(data);
            }
            
        }

        protected override IEnumerable<string> DataProcessor(IEnumerable<string> x)
        {
            foreach (var y in x)
            {
                yield return y.Trim().Trim('\n').Trim('\t').Trim('\r');
            }
            DumpResults();
        }

        private void GoogleUsageConfirmer(By tagName)
        {
            uint i = 0;
            if (_driver?.WindowHandles == null) 
                throw new NullReferenceException();
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