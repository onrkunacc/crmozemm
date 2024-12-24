using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.BL.Extensions
{
    public static class JsonHelper
    {
        public static string SerializeObject(object obj)
        {
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                WriteIndented = true
            };

            return JsonSerializer.Serialize(obj, options);
        }
    }
}
