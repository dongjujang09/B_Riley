using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B_Riley.BankingApp.Utils
{
    public static class Extensions
    {
        public static string Serialize(this object obj, bool indented = true)
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                MaxDepth = 2,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            return JsonConvert.SerializeObject(obj, (indented ? Formatting.Indented : Formatting.None), jsonSettings);
        }

        public static T? Deserialize<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
