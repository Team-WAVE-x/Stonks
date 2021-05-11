using Newtonsoft.Json;
using System.IO;

namespace Stonks.Module
{
    internal class SettingModule
    {
        public static Class.Setting GetSettingInfo()
        {
            string jsonString = File.ReadAllText($"{System.AppDomain.CurrentDomain.BaseDirectory}\\settings.json");
            return JsonConvert.DeserializeObject<Class.Setting>(jsonString);
        }
    }
}