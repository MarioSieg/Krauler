using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using CsvHelper;
using System.IO;
using System.Text;
using System.Threading;

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

        // -1 = loop forever
        public sbyte RetryCount = -1;

        public byte TabCount = 2;

        public bool UseProxy = false;

        public GoogleCrawlerConfig() : base("http://google.com") { }
    }

    public struct GoogleCrawlerRawData
    {
        public string RawUrl;
        public string RawUrlTitle;
        public string? RawUrlDescription;
    }

    public struct GoogleCrawlerResult
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return "\r\n" + Url + ": " + Title + "\r\n >> " + Description;
        }
    }

    public sealed class GoogleCrawler : Crawler<GoogleCrawlerRawData, GoogleCrawlerResult>
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
            /*
            Google Search URL params 
            gws =google web server
            rd = redirected
            cr = country reffered
            ei = timestamp. https://deedpolloffice.com/blog/articles/decoding-ei-parameter
            gs_lcp = Not sure, but it's Protobuf encoded. Edit: Someone said in replies this is likely to encode to a physical location, which also seems likely.
            sclient = Where you came from (so if you used images.google.com it would be img)
            bih = Browser height (pixels)
            q = The query it's actually searching for. Usually the same as oq unless you clicked on a suggested search term, then oq would be the text you typed and q is what you clicked on (what's actually being searched for)
            tbm=isch tells it you want to search Google Images. It stands for to be matched = image search.
            ved decodes to tell Google what links you clicked on previous pages on Google to get to the current page (https://valentin.app/ved.html)
            oq = The original query you wanted to search for that you typed in (see q)
            */
            
            Debug.Assert(_driver != null, nameof(_driver) + " != null");
            const string? query = "Singer Songwriter";
            const ushort maxSearchPages = 2; // the number of pages to search
            const ushort maxResults = 5; // the number of max results

            for (uint i = 1; i < maxSearchPages; ++i)
            {
                //GoogleLinksCrawler(query, i);
                GoogleImageCrawler(query, i);
            }
            
        }
        private void GoogleImageCrawler(string query, uint i)
        {
            if (i == 1) // google confirm only at first call 
            {
                var url = $"{_config.ServerHeader.Uri}/search?q={query}&tbm=isch";
                Logger.Instance.WriteLine($"GoTo Url: {url}");
                Debug.Assert(_driver != null, nameof(_driver) + " != null");
                
                _driver.Navigate().GoToUrl(url);
                if (_driver.FindElementSafe(By.TagName("button")) != null)
                    // GoogleUsageConfirmer(By.TagName("button"));

                if (_driver.FindElementSafe(By.XPath("//button[@id='zV9nZe']")) != null)
                    GoogleUsageConfirmer(By.XPath("//button[@id='zV9nZe']"));
            }
            else
            {
                _ = ((IJavaScriptExecutor?) _driver)?.ExecuteScript("window.scrollBy(0,document.body.scrollHeight)");
            }

            Debug.Assert(_driver != null, nameof(_driver) + " != null"); 
            
            if(_driver.FindElementSafe(By.XPath("/html/body/div[2]/c-wiz/div[3]/div[1]/div/div/div/div/div[5]/input"))!= null)
                _driver.FindElement(By.XPath("/html/body/div[2]/c-wiz/div[3]/div[1]/div/div/div/div/div[5]/input")).Click();
            Thread.Sleep(500);

            IWebElement resultsPanel = _driver.FindElement(By.Id("islmp"));
            ReadOnlyCollection<IWebElement> searchResults = resultsPanel.FindElements(By.XPath(".//a"));
            IEnumerable<GoogleCrawlerRawData> inputData = searchResults.Select(x => new GoogleCrawlerRawData
            {
                RawUrl = x.GetAttribute("href"),
                RawUrlTitle = x.Text,
            });
            SubmitData(inputData);
        }
        private void GoogleLinksCrawler(string query, uint i)
        {
            var url = $"{_config.ServerHeader.Uri}/search?q={query}&start={(i - 1) * 10}";
            Logger.Instance.WriteLine($"GoTo Url: {url}");
            _driver.Navigate().GoToUrl(url);

            if (i == 1) // google confirm only at first call 
            {
                if (_driver.FindElementSafe(By.TagName("button")) != null)
                    GoogleUsageConfirmer(By.TagName("button"));

                if (_driver.FindElementSafe(By.XPath("//button[@id='zV9nZe']")) != null)
                    GoogleUsageConfirmer(By.XPath("//button[@id='zV9nZe']"));
            }

            IWebElement resultsPanel = _driver.FindElement(By.Id("search"));

            ReadOnlyCollection<IWebElement> searchResults = resultsPanel.FindElements(By.XPath(".//a"));
            SubmitData(searchResults.Select(x => new GoogleCrawlerRawData
            {
                RawUrl = x.GetAttribute("href"),
                RawUrlTitle = x.Text,
                RawUrlDescription = x.GetAttribute("title")
            }));
        }

        protected override IEnumerable<GoogleCrawlerResult> DataProcessor(IEnumerable<GoogleCrawlerRawData>? rawData)
        {
            if (rawData == null || !rawData.Any())
            {
                yield break;
            }
            foreach (var raw in rawData)
            {
                // exclude results from google cache archive or google internal links
                if (!string.IsNullOrEmpty(raw.RawUrl) && raw.RawUrl.Contains("http") && !raw.RawUrl.Contains("webcache") && !raw.RawUrl.Contains(_config.ServerHeader.Uri.Host))
                {
                    yield return new GoogleCrawlerResult
                    {
                        Url = raw.RawUrl,
                        Title = raw.RawUrlTitle,
                        Description = raw.RawUrlDescription ?? string.Empty,
                    };
                }
            }
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
            Thread.Sleep(20000);
            _driver?.Quit();

            //SaveToCsvFile();

            
        }

        private void SaveToCsvFile()
        {
            var dir = Config.ResourcesDir;
            string outputFile = "result.csv";
            if (!Directory.Exists(dir))
            {
                throw new IOException($"Directory {dir} not existing");
            }

            using var writer = new StreamWriter(dir + "/" + outputFile);
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                //csvWriter.WriteHeader<GoogleCrawlerResult>();
                csvWriter.WriteRecords(Results);
            }

            writer.Flush();
        }
    }
}