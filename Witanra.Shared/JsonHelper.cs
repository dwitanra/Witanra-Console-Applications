using System;
using System.IO;

namespace Witanra.Shared
{
    public class JsonHelper
    {
        public static T DeserializeFile<T>(string filename)
        {
            return Deserialize<T>(String.Join("", File.ReadAllLines(filename)));
        }

        public static T Deserialize<T>(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        public static void SerializeFile(object value, string filename)
        {
            File.WriteAllText(filename, Serialize(value));
        }

        public static string Serialize(object value)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value);
        }
    }
}