using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Krauler.Crawlers
{
    public sealed class RadolfzellCrawlerConfig : BaseConfig
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

        public RadolfzellCrawlerConfig() : base("https://radolfzell.aed-synergis.de/WebOffice_flex/synserver?project=Radolfzell_flex&client=flex") { }
    }

    public struct RadolfzellCrawlerRawData
    {
        
    }

    public struct AltglasContainer
    {
        public float X, Y;
    }

    public struct RadolfzellCrawlerResult
    {
        public AltglasContainer[] Container;
    }

    public sealed class RadolfzellCrawler : Crawler<RadolfzellCrawlerRawData, RadolfzellCrawlerResult>
    {
        private readonly RadolfzellCrawlerConfig _config;
        private IWebDriver? _driver;
        private static readonly HttpClient Client = new();

        public RadolfzellCrawler() : base("RadolfzellCrawler", "Crawlt positionsdaten für das Coding-Camp 2021!")
        {
            _config = InitializeConfig<RadolfzellCrawler, RadolfzellCrawlerConfig>();
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
                string proxy = CrawlerFactory.Proxies.Value[rand.Next(CrawlerFactory.Proxies.Value.Length)];
                options.AddArgument("--proxy-server=http://" + proxy);
            }

            // choose a random proxy from file list
            if (_config.SetUserAgent)
            {
                var rand = new Random();
                string userAgent = CrawlerFactory.UserAgents.Value[rand.Next(CrawlerFactory.UserAgents.Value.Length)];
                options.AddArgument("--user-agent=" + userAgent);
            }

            _driver = new ChromeDriver(service, options);

            _driver.Manage().Cookies.DeleteAllCookies();
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        }
        
        private (bool, object?) ExecJs(string script, bool async, params object[] args)
        {
            try
            {
                var vm = ((IJavaScriptExecutor?)this._driver);
                object? r = async ? vm?.ExecuteAsyncScript(script, args) : vm?.ExecuteScript(script, args);
                return (true, r);
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex);
                return (false, null);
            }
        }
        
        private byte[]? HttpRequestRadoImage()
        {
            try
            {
                const string uri = "https://hosting.aed-synergis.de/mapgis/rest/services/DTK/DTK_webmercator/MapServer/tile/13/2854/4299";
                const string method = "GET";
                const string usr = "", pwd = "";
                const string domain = "https://hosting.aed-synergis.de";
                const ushort timeout = 5000;

                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = method;
                NetworkCredential networkCredential = new NetworkCredential(usr, pwd, domain);
                request.Credentials = networkCredential;
                request.Timeout = timeout;
                Logger.Instance.WriteLine(
                    $"Sending HTTP request: M: {request.Method}, USR: {usr}, PDW: {pwd}, DON: {domain}");
                var httpWebResponse = (HttpWebResponse) request.GetResponse();
                var responseStream = httpWebResponse.GetResponseStream();
                byte[]? result;
                using (MemoryStream ms = new())
                {
                    responseStream.CopyTo(ms);
                    result = ms.ToArray();
                }
                responseStream.Close();
                return result;
            }
            catch (Exception ex)
            {
                Logger.Instance.Write(ex);
                return null;
            }
        }

        public override void OnDispatch()
        {
            Uri url = this._config.ServerHeader.Uri;
            IWebDriver driver = this._driver ?? throw new InvalidOperationException();
            _driver.Navigate().GoToUrl(url);
            byte[] image = HttpRequestRadoImage() ?? throw new Exception("Failed to crawl image!");
            Logger.Instance.WriteLine($"Got image with size: {image.Length * sizeof(byte)}");
        }

        protected override IEnumerable<RadolfzellCrawlerResult> DataProcessor(
            IEnumerable<RadolfzellCrawlerRawData>? rawDAta)
        {
            yield break;
        }

        public override void OnDestroy()
        {
            
        }
    }
}