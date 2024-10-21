using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;

namespace OrderLunch.Helper
{
    public static class StringUtils
    {
        public static string ToQueryStringUsingNewtonsoftJson<T>(T obj)
        {
            string jsonString = JsonConvert.SerializeObject(obj);

            var jsonObject = JObject.Parse(jsonString);

            var properties = jsonObject
                .Properties()
                .Where(p => p.Value.Type != JTokenType.Null)
                .Select(p =>
                    $"{HttpUtility.UrlEncode(p.Name)}={HttpUtility.UrlEncode(p.Value.ToString())}");

            return string.Join("&", properties);
        }
    }
}
