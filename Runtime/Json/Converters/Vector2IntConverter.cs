using System;
using Newtonsoft.Json;
using UnityEngine;

namespace AceLand.Serialization.Json.Converters
{
    public class Vector2IntConverter : JsonConverter<Vector2Int>
    {
        public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WriteEndObject();
        }

        public override Vector2Int ReadJson(JsonReader reader, System.Type objectType, Vector2Int existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            int x = 0, y = 0;

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
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    break;
                }
            }

            return new Vector2Int(x, y);
        }
    }
}
