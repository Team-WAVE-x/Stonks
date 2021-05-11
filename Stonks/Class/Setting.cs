using Newtonsoft.Json;

namespace Stonks.Class
{
    public class Setting
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("connection_string")]
        public string ConnectionString { get; set; }

        [JsonProperty("developer_id")]
        public string DeveloperID { get; set; }
    }
}