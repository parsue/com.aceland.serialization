using System;
using System.Threading.Tasks;
using AceLand.Serialization.Models;
using Newtonsoft.Json;
using UnityEngine;

namespace AceLand.Serialization.Json
{
    public static class JsonAsyncExtension
    {
        public static Task<JsonData> ToJsonAsync<T>(this T data, bool withTypeName = false)
        {
            var token = Application.exitCancellationToken;
            return Task.Run(() =>
                {
                    try
                    {
                        var settings = withTypeName
                            ? AceSerialization.JsonSerializerSettingsWithType
                            : AceSerialization.JsonSerializerSettings;
                        var json = JsonConvert.SerializeObject(data, Formatting.None, settings);
                        var builder = JsonData.Builder().WithText(json);
                        return withTypeName
                            ? builder.WithTypeName().Build()
                            : builder.Build();
                    }
                    catch (Exception e)
                    {
                        return token.IsCancellationRequested
                            ? null
                            : throw new Exception($"Serialize {typeof(T).Name} to json error\n{e}");
                    }
                },
                token
            );
        }

        public static Task<T> ToDataAsync<T>(this JsonData jsonData)
        {
            var token = Application.exitCancellationToken;
            return Task.Run(() =>
                {
                    try
                    {
                        var settings = jsonData.WithTypeName
                            ? AceSerialization.JsonSerializerSettingsWithType
                            : AceSerialization.JsonSerializerSettings;
                        var data = JsonConvert.DeserializeObject<T>(jsonData.Text, settings);
                        return data;
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Deserialize json to {typeof(T).Name} error\n{e}");
                    }
                },
                token
            );
        }
    }
}
