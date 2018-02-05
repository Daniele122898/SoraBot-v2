using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SoraBot_v2.Services
{
    public static class ConfigService
    {
        private static JsonSerializer JsonSerializer = new JsonSerializer();
        private static ConcurrentDictionary<string, string> _configDict = new ConcurrentDictionary<string, string>();

        public static void InitializeLoader()
        {
            JsonSerializer.Converters.Add(new JavaScriptDateTimeConverter());
            JsonSerializer.NullValueHandling = NullValueHandling.Ignore;
        }

        public static void LoadConfig()
        {
            if (!File.Exists("config.json"))
            {
                throw new IOException("COULDN'T FIND AND LOAD CONFIG FILE! at "+ Directory.GetCurrentDirectory());
            }

            using (StreamReader sr = File.OpenText("config.json"))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                _configDict = JsonSerializer.Deserialize<ConcurrentDictionary<string, string>>(reader);
            }
        }

        public static string GetConfigData(string key)
        {
            string result = "";
            _configDict.TryGetValue(key, out result);
            return result;
        }

        public static ConcurrentDictionary<string, string> GetConfig()
        {
            //// added by Catherine Renelle - Memory Leak Fix
            if (_configDict.IsEmpty)
                LoadConfig();

            return _configDict;
        }
    }
}