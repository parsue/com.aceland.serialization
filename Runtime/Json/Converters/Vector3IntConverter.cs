using System;
using Newtonsoft.Json;
using UnityEngine;

namespace AceLand.Serialization.Json.Converters
{
    public class Vector3IntConverter : JsonConverter<Vector3Int>
    {
        public override void WriteJson(JsonWriter writer, Vector3Int value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
            writer.WriteEndObject();
        }

        public override Vector3Int ReadJson(JsonReader reader, System.Type objectType, Vector3Int existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            int x = 0, y = 0, z = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var propertyName = (string)reader.Value;
                    reader.Read();
                    switch (propertyName)
                    {
                        case "x":
                            x = Convert.ToInt32(reader.Value);
                            break;
                        case "y":
                            y = Convert.ToInt32(reader.Value);
                            break;
                        case "z":
                            z = Convert.ToInt32(reader.Value);
                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    break;
                }
            }

            return new Vector3Int(x, y, z);
        }
    }
}
