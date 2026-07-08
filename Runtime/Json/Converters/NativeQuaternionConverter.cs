using System;
using Newtonsoft.Json;
using Unity.Mathematics;

namespace AceLand.Serialization.Json.Converters
{
    public class NativeQuaternionConverter : JsonConverter<quaternion>
    {
        public override void WriteJson(JsonWriter writer, quaternion value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.value.z);
            writer.WritePropertyName("w");
            writer.WriteValue(value.value.w);
            writer.WriteEndObject();
        }

        public override quaternion ReadJson(JsonReader reader, System.Type objectType, quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            float x = 0, y = 0, z = 0, w = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var propertyName = (string)reader.Value;
                    reader.Read();
                    switch (propertyName.ToLower())
                    {
                        case "x":
                            x = Convert.ToSingle(reader.Value);
                            break;
                        case "y":
                            y = Convert.ToSingle(reader.Value);
                            break;
                        case "z":
                            z = Convert.ToSingle(reader.Value);
                            break;
                        case "w":
                            w = Convert.ToSingle(reader.Value);
                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    break;
                }
            }

            return new quaternion(x, y, z, w);
        }
    }
}