using System.IO;
using Newtonsoft.Json;

namespace Stonks.Module
{
    internal class SettingModule
    {
        public class Setting
        {
            [JsonProperty("token")]
            public string Token { get; set; }

            [JsonProperty("connection_string")]
            public string ConnectionString { get; set; }

            [JsonProperty("developer_id")]
            public ulong DeveloperID { get; set; }
        }

        public static Setting GetSettingInfo()
        {
            string jsonString = File.ReadAllText($"{System.AppDomain.CurrentDomain.BaseDirectory}\\settings.json");
            return JsonConvert.DeserializeObject<Setting>(jsonString);
        }
    }
}