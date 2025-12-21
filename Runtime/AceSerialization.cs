using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using AceLand.Serialization.Binary;
using AceLand.Serialization.Json.Converters;
using Newtonsoft.Json;
using UnityEngine;

namespace AceLand.Serialization
{
    public static class AceSerialization
    {
        public static JsonSerializerSettings JsonSerializerSettings { get; } = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = converters,
        };

        public static JsonSerializerSettings JsonSerializerSettingsWithType { get; } = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = converters,
        };
        
        private static readonly List<JsonConverter> converters = new()
        {
            new ColorConverter(), new GradientConverter(), new AnimationCurveConverter(),
            new Vector2Converter(), new Vector3Converter(), new Vector4Converter(),
            new Vector2IntConverter(), new Vector3IntConverter(), new QuaternionConverter(),
            new BoundsConverter(), new BoundsIntConverter(), new Matrix4X4Converter(),
            new Hash128Converter(), new LayerMaskConverter(),
            new RectIntConverter(), new RectConverter(),
            new NativeFloat2Converter(), new NativeFloat3Converter(), new NativeFloat4Converter(),
            new NativeQuaternionConverter(),
            new NativeFloat2X2Converter(), new NativeFloat2X3Converter(), new NativeFloat2X4Converter(), 
            new NativeFloat3X2Converter(), new NativeFloat3X3Converter(), new NativeFloat3X4Converter(),
            new NativeFloat4X2Converter(), new NativeFloat4X3Converter(), new NativeFloat4X4Converter(),
        };
        
        public static IFormatter GetBinaryFormatter()
        {
            const StreamingContextStates states = StreamingContextStates.All;
            
            var formatter = new BinaryFormatter();
            var selector = new SurrogateSelector();
            var animationCurveSurrogate = new AnimationCurveSerializationSurrogate();
            var boundsIntSurrogate = new BoundsIntSerializationSurrogate();
            var boundsSurrogate = new BoundsSerializationSurrogate();
            var colorSurrogate = new ColorSerializationSurrogate();
            var gradientSurrogate = new GradientSerializationSurrogate();
            var hash128Surrogate = new Hash128SerializationSurrogate();
            var layerMaskSurrogate = new LayerMaskSerializationSurrogate();
            var matrix4X4Surrogate = new Matrix4x4SerializationSurrogate();
            var quaternionSurrogate = new QuaternionSerializationSurrogate();
            var rectIntSurrogate = new RectIntSerializationSurrogate();
            var rectSurrogate = new RectSerializationSurrogate();
            var vector2Surrogate = new Vector2SerializationSurrogate();
            var vector2IntSurrogate = new Vector2IntSerializationSurrogate();
            var vector3Surrogate = new Vector3SerializationSurrogate();
            var vector3IntSurrogate = new Vector3IntSerializationSurrogate();
            var vector4Surrogate = new Vector4SerializationSurrogate();

            selector.AddSurrogate(typeof(AnimationCurve), new StreamingContext(states), animationCurveSurrogate);
            selector.AddSurrogate(typeof(BoundsInt), new StreamingContext(states), boundsIntSurrogate);
            selector.AddSurrogate(typeof(Bounds), new StreamingContext(states), boundsSurrogate);
            selector.AddSurrogate(typeof(Color), new StreamingContext(states), colorSurrogate);
            selector.AddSurrogate(typeof(Gradient), new StreamingContext(states), gradientSurrogate);
            selector.AddSurrogate(typeof(Hash128), new StreamingContext(states), hash128Surrogate);
            selector.AddSurrogate(typeof(LayerMask), new StreamingContext(states), layerMaskSurrogate);
            selector.AddSurrogate(typeof(Quaternion), new StreamingContext(states), quaternionSurrogate);
            selector.AddSurrogate(typeof(RectInt), new StreamingContext(states), rectIntSurrogate);
            selector.AddSurrogate(typeof(Rect), new StreamingContext(states), rectSurrogate);
            selector.AddSurrogate(typeof(Vector2), new StreamingContext(states), vector2Surrogate);
            selector.AddSurrogate(typeof(Vector2Int), new StreamingContext(states), vector2IntSurrogate);
            selector.AddSurrogate(typeof(Vector3), new StreamingContext(states), vector3Surrogate);
            selector.AddSurrogate(typeof(Vector3Int), new StreamingContext(states), vector3IntSurrogate);
            selector.AddSurrogate(typeof(Vector4), new StreamingContext(states), vector4Surrogate);
            selector.AddSurrogate(typeof(Matrix4x4), new StreamingContext(states), matrix4X4Surrogate);
            formatter.SurrogateSelector = selector;

            return formatter;
        }
        
        private const string CVS_LINE_REGEX = @"(?<=^|,)(?:""(?<x>([^""]|"""")*)""|(?<x>[^,""\r\n]*))";

        internal static string[] ReadCsvLine(string line)
        {
            return (from Match m in Regex.Matches(line, CVS_LINE_REGEX, RegexOptions.ExplicitCapture) 
                select m.Groups[1].Value).ToArray();
        }
    }
}