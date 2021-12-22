using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using CsvHelper;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
        
        public string Domain { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }

        public override string ToString()
        {
            return "\r\n" + Url + ": " + Title + "\r\n >> " + Description;
        }
    }

    public sealed class GoogleCrawler : Crawler<GoogleCrawlerRawData, GoogleCrawlerResult, GoogleCrawler, GoogleCrawlerConfig>
    {
        private IWebDriver? _driver;
        private readonly string _searchQuery;
        /// <summary>
        /// The HttpClient for crawling images.
        /// ! one client for all requests: https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
        /// </summary>
        private static readonly HttpClient Client = new();
        
        public GoogleCrawler() : base(nameof(GoogleCrawler), "")
        {
            _searchQuery = "singer songwriter";
        }

        protected override void OnInitialize()
        {
            if (Config.WebDriverType == WebDriverType.Chrome)
            {
                var options = CreateChromeOptions(out var service);

                _driver = new ChromeDriver(service, options);
            }
            else if (Config.WebDriverType == WebDriverType.Firefox)
            {
                var options = CreateFirefoxOptions();
                _driver = new FirefoxDriver(options);
            }
            else
            {
                throw new ArgumentException(
                    $"Selenium driver is not set in {GetType()}");
            }

            _driver.Manage().Cookies.DeleteAllCookies();
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        }

        private FirefoxOptions? CreateFirefoxOptions()
        {
            var options = new FirefoxOptions();
            if (Config.UseProxy)
            {
                Proxy proxy = new()
                {
                    HttpProxy = Utility.GetRandomProxy()
                };
                options.Proxy = proxy;
            }
            if (Config.SetUserAgent)
            {
                var userAgent = Utility.GetRandomUserAgent();
                options?.AddArgument("--user-agent=" + userAgent);
            }
   
            return options;
        }

        private ChromeOptions? CreateChromeOptions(out ChromeDriverService? service)
        {
            var options = new ChromeOptions { PageLoadStrategy = Config.PageLoadStrategy };
            options.AddArguments(Config.DefaultChromeOptions);
            options.AddArguments(new List<string>
                { "headless", "disable-gpu" });

            service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = Config.ChromeDriverHideCommandPromptWindow;
            if (Config.UseProxy)
            {
                var proxy = Utility.GetRandomProxy();
                options?.AddArgument("--proxy-server=http://" + proxy);
            }
            if (Config.SetUserAgent)
            {
                var userAgent = Utility.GetRandomUserAgent();
                options?.AddArgument("--user-agent=" + userAgent);
            }

            return options;
        }

        protected override void OnDispatch()
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
            const ushort maxSearchPages = 3; // the number of pages to search
            const ushort maxResults = 5; // the number of max results
            // TODO: Implement functionality to define max results instead of maxSearchPages (optional)
            
                
            for (uint i = 1; i < maxSearchPages; ++i)
            {
                //GoogleLinksCrawler(query, i);
                GoogleImageCrawler(_searchQuery, i);
            }
            
        }
        
        /// <summary>
        /// Crawls images by keywords via google and saves them in a folder.
        /// Establishes HttpClient connection to download images from urls as well as downloads dataUris from the search results directly.
        /// More images are crawled via scrolling.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="i"></param>
        private void GoogleImageCrawler(string query, uint i)
        {
            if (i == 1) // google confirm only at first call 
            {
                // ToDo: optimize header config with referal etc
                var url = $"{Config.ServerHeader.Uri}/search?q={query}&tbm=isch";
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
            var clickElement = _driver.FindElementSafe(By.XPath("/html/body/div[2]/c-wiz/div[3]/div[1]/div/div/div/div/div[5]/input"));
            clickElement?.Click();
            Thread.Sleep(500);

            IWebElement resultsPanel = _driver.FindElement(By.Id("islmp"));
            // get urls
            ReadOnlyCollection<IWebElement> searchResults = resultsPanel.FindElements(By.XPath(".//div[@data-ictx='1']"));
            IEnumerable<GoogleCrawlerRawData> rawData = searchResults.Select(x => new GoogleCrawlerRawData
            {
                RawUrl = x.FindElements(By.TagName("a"))[1].GetAttribute("href"), // the where the full picture comes from is stored in the 2nd 'a' element
                RawUrlTitle = x.FindElement(By.TagName("img")).GetAttribute("alt"),
                RawUrlDescription = "_" + x.FindElement(By.TagName("img")).GetAttribute("src") // link to the resized google search result image
            });

            SubmitData(rawData);
            
        }
        private void GoogleLinksCrawler(string query, uint i)
        {
            var url = $"{Config.ServerHeader.Uri}/search?q=\"{query}\"&start={(i - 1) * 10}";
            Logger.Instance.WriteLine($"GoTo Url: {url}");
            _driver?.Navigate().GoToUrl(url);

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

        /// <summary>
        /// Google Data Processor handles currently 
        /// - link search
        /// - image search and download
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        protected override IEnumerable<GoogleCrawlerResult> DataProcessor(IEnumerable<GoogleCrawlerRawData>? rawData)
        {
            var googleCrawlerRawDatas = (rawData ?? throw new ArgumentNullException(nameof(rawData))).ToList();
            if (!googleCrawlerRawDatas.Any())
            {
                yield break;
            }
            foreach (var raw in googleCrawlerRawDatas)
            {
                // exclude results from google cache archive or google internal links
                if (!string.IsNullOrEmpty(raw.RawUrl) && raw.RawUrl.Contains("http") && !raw.RawUrl.Contains("webcache") && !raw.RawUrl.Contains(Config.ServerHeader.Uri.Host))
                {
                    yield return new GoogleCrawlerResult
                    {
                        Url = Uri.IsWellFormedUriString(raw.RawUrl, UriKind.RelativeOrAbsolute) ? raw.RawUrl : string.Empty,
                        Title = raw.RawUrlTitle,
                        Description = raw.RawUrlDescription!.StartsWith("_") ? SaveImageFromDataUri(raw.RawUrl, raw.RawUrlDescription.Substring(1)).Result : raw.RawUrlDescription ?? string.Empty,
                    };
                }
            }
            DumpResults();
        }

        /// <summary>
        /// Save an image from url to local folder.
        /// Possible urls:
        /// - data uri string eg data:image/png;base64,IMAGE_BYTE_CODE
        /// - url directing directly to an image
        /// Download to Config.CrawledImages with the query subfolder and a date folder, and a hash derived from the dataUri as image name.
        /// TODO: Potential error handling, retrieve filetype from dataUri, move foldercreating etc somewhere higher so that this function only writes the image
        /// ToDO: @Mario: pruefe bitte hier, ob das mit dem threading so ok ist (die http methoden sind per default async)
        /// Might not work in IE (https://en.wikipedia.org/wiki/Data_URI_scheme)
        /// </summary>
        /// <param name="urlName">the main url relating to the website the image is from</param>
        /// <param name="imageUri">string referring to data-Uri or image-only-url, assumed never to be null</param>
        /// <returns>the image file path string</returns>
        private async Task<string> SaveImageFromDataUri(string urlName ,string imageUri)
        {
            var time = DateTime.Now;
            var saveImagesFolderPath = Krauler.Config.CrawledImages +_searchQuery + "/" + time.ToShortDateString().Replace('/', '-');
            string imageName = $"{imageUri.GetHashCode():X}";
            
            if (!Directory.Exists(saveImagesFolderPath)) Directory.CreateDirectory(saveImagesFolderPath);
            
            var imageFilePath = $@"{saveImagesFolderPath}/{imageName}.jpg";

            if (imageUri.StartsWith("data"))
            {
                var b64 = imageUri.Split(",".ToCharArray(), 2);
                try
                {
                    byte[] byteArray = Convert.FromBase64String(b64[1]);
                    await File.WriteAllBytesAsync(imageFilePath, byteArray);
                    Logger.Instance.WriteLine($"Write [data] image from  {urlName} to {imageFilePath}");
                }
                catch (Exception e)
                {
                    Logger.Instance.WriteLine($"I/O Error in {urlName} with {imageUri}: {e}");
                    imageFilePath = imageUri;
                }
            } else if (Uri.IsWellFormedUriString(imageUri, UriKind.RelativeOrAbsolute))
            {
                try
                {
                    using (HttpResponseMessage response = await Client.GetAsync(imageUri))
                    {
                        // ToDo: verify responses before download
                        byte[] byteArray = await response.Content.ReadAsByteArrayAsync();
                        await File.WriteAllBytesAsync(imageFilePath, byteArray);
                    }
                    Logger.Instance.WriteLine($"Write [url] image from {urlName} to {imageFilePath}");
                }
                catch (Exception e)
                {
                    Logger.Instance.WriteLine($"I/O Error in {urlName} with {imageUri}: {e}");
                    imageFilePath = imageUri;
                }
            }
            else
            {
                imageFilePath = "Download failed [unknown Uri type]: " + imageUri;
            }

            return imageFilePath;
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

        protected override void OnDestroy()
        {
            Thread.Sleep(20000);
            _driver?.Quit();
            //SaveToCsvFile();
        }

        private void SaveToCsvFile()
        {
            const string? dir = Krauler.Config.OutputDir;
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