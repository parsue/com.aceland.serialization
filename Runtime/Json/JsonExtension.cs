using System;
using AceLand.Serialization.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace AceLand.Serialization.Json
{
    public static class JsonExtension
    {
        public static JsonData ToJson<T>(this T data, bool withTypeName = false)
        {
            var settings = withTypeName ? AceSerialization.JsonSerializerSettingsWithType : AceSerialization.JsonSerializerSettings;
            var text = JsonConvert.SerializeObject(data, Formatting.None, settings);
            var builder = JsonData.Builder().WithText(text);
            return withTypeName
                ? builder.WithTypeName().Build()
                : builder.Build();
        }

        public static T ToData<T>(this JsonData jsonData)
        {
            var settings = jsonData.WithTypeName ? AceSerialization.JsonSerializerSettingsWithType : AceSerialization.JsonSerializerSettings;
            return JsonConvert.DeserializeObject<T>(jsonData.Text, settings);
        }
        
        public static bool IsValidJson(this string json)
        {
            if (json == null) return false;
            if (json.Trim() == string.Empty) return true; 
            
            try
            {
                JToken.Parse(json);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public static bool IsValidJson(this TextAsset json)
        {
            return IsValidJson(json.text);
        }
    }
}
