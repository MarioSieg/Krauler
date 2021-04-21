using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
    }

    public static class Config
    {
        public const string ConfigDir = "Config/";
        public const string ResourcesDir = "Resources/";

        static Config()
        {
            Directory.CreateDirectory(ConfigDir);
            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter {CamelCaseText = true});
                return settings;
            };
        }

        public static void Serialize<T>(T? data, string? className)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText($"{ConfigDir}{className}.ini", json);
        }

        public static T? Deserialize<T>(string className)
        {
            string raw = File.ReadAllText($"{ConfigDir}{className}.ini");
            return JsonConvert.DeserializeObject<T?>(raw);
        }
    }
}