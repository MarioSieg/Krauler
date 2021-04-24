﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using static Krauler.Utility;

namespace Krauler.Crawlers
{
    public sealed class GoogleCrawlerConfig : BaseConfig
    {
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

        public readonly bool SetUserAgent = false;
        public bool ChromeDriverHideCommandPromptWindow = true;

        public PageLoadStrategy PageLoadStrategy = PageLoadStrategy.Normal;

        public byte TabCount = 2;

        // -1 = loop forever
        public sbyte RetryCount = -1;

        public bool UseProxy = false;

        public GoogleCrawlerConfig() : base("http://google.com") { }
    }

    public class GoogleCrawler : Crawler<string>
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

            // choose a random proxy from file list
            if (_config.UseProxy)
            {
                var rand = new Random();
                var proxy = CrawlerFactory.Proxies.Value[rand.Next(CrawlerFactory.Proxies.Value.Length)];
                options.AddArgument("--proxy-server=http://" + proxy);
            }

            // choose a random proxy from file list
            if (_config.SetUserAgent)
            {
                var rand = new Random();
                var userAgent = CrawlerFactory.UserAgents.Value[rand.Next(CrawlerFactory.UserAgents.Value.Length)];
                options.AddArgument("--user-agent=" + userAgent);
            }

            _driver = new ChromeDriver(service, options);

            _driver.Manage().Cookies.DeleteAllCookies();
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        }

        public override void OnDispatch()
        {
            // Google Search URL params 
            // gws =google web server
            // rd = redirected
            // cr = country reffered
            // ei = timestamp. https://deedpolloffice.com/blog/articles/decoding-ei-parameter
            // gs_lcp = Not sure, but it's Protobuf encoded. Edit: Someone said in replies this is likely to encode to a physical location, which also seems likely.
            // sclient = Where you came from (so if you used images.google.com it would be img)
            // bih = Browser height (pixels)
            // q = The query it's actually searching for. Usually the same as oq unless you clicked on a suggested search term, then oq would be the text you typed and q is what you clicked on (what's actually being searched for)
            // tbm=isch tells it you want to search Google Images. It stands for to be matched = image search.
            // ved decodes to tell Google what links you clicked on previous pages on Google to get to the current page (https://valentin.app/ved.html)
            // oq = The original query you wanted to search for that you typed in (see q)

            Debug.Assert(_driver != null, nameof(_driver) + " != null");
            const string? query = "KevinKlang";
            const ushort maxSearchPages = 5;

            for (uint i = 1; i < maxSearchPages; ++i)
            {
                _driver.Navigate().GoToUrl($"{_config.ServerHeader.Uri}/search?q={query}&start={(i - 1) * 10}");
                
                if (i == 1) // google confirm only at first call 
                {
                    if (_driver.FindElementSafe(By.TagName("button")) != null)
                        GoogleUsageConfirmer(By.TagName("button"));
                    
                    if (_driver.FindElementSafe(By.Id("zV9nZe")) != null)
                        GoogleUsageConfirmer(By.XPath("//button[@id='zV9nZe']"));
                }
                
                IWebElement resultsPanel = _driver.FindElement(By.Id("search"));

                ReadOnlyCollection<IWebElement> searchResults = resultsPanel.FindElements(By.XPath(".//a"));
                SubmitData(searchResults.Select(x => x.Text));
            }
        }

        protected override IEnumerable<string> DataProcessor(IEnumerable<string> rawText)
        {
            foreach (var text in rawText)
                if (text.Contains("http"))
                    yield return LinkParser.Match(text).Value;
            DumpResults();
        }

        private void GoogleUsageConfirmer(By tagName)
        {
            uint i = 0;
            if (_driver?.WindowHandles == null)
                throw new NullReferenceException();
            foreach (var handle in _driver.WindowHandles)
                try
                {
                    _driver.SwitchTo().Window(handle);
                    _driver.FindElement(tagName).Click();
                    Logger.Instance.WriteLine($"Confirmed google usage {++i} by {tagName}");
                }
                catch (Exception ex)
                {
                    Logger.Instance.Write(ex);
                }
        }

        public override void OnDestroy()
        {
            //Thread.Sleep(20000);
            //_chromeDriver?.Quit();
        }
    }
}