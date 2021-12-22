using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Krauler
{
    public abstract class BaseConfig
    {
        protected BaseConfig(string url)
        {
            ServerHeader = new ServerHeader
            {
                Locked = false,
                MaxTrials = 5,
                Uri = new Uri(url)
            };
        }

        public ServerHeader ServerHeader { get; set; }
        /// <summary>
        /// The browser driver .exe that is used to browse
        /// </summary>
        public WebDriverType WebDriverType { get; set; }
    }

    public static class Config
    {
        public const string ConfigDir = "Config/";
        public const string ResourcesDir = "Resources/";
        public const string DriverDir = ResourcesDir + "Drivers/";
        public const string OutputDir = "Output/";
        public const string LoggingDir = OutputDir + "Logs/";
        public const string CrawledImages = OutputDir + "CrawledImages/";

        static Config()
        {
            Directory.CreateDirectory(ConfigDir);
            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings();
                var item = new StringEnumConverter {NamingStrategy = new CamelCaseNamingStrategy()};
                settings.Converters.Add(item);
                return settings;
            };
        }

        public static void Serialize<T>(T? data, string? className)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText($"{ConfigDir}{className}.json", json);
        }

        public static T? Deserialize<T>(string? className)
        {
            string raw = File.ReadAllText($"{ConfigDir}{className}.json");
            return JsonConvert.DeserializeObject<T?>(raw);
        }
    }
}