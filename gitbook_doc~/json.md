# JSON

Serialize and deserialize any object — including Unity types — with Newtonsoft.Json under the hood.

## Overview
All JSON functionality is exposed as extension methods in the `AceLand.Serialization.Json` namespace.
The heavy lifting is done by [Newtonsoft.Json](https://www.newtonsoft.com/json), but the package
pre-registers a large converter list so that Unity types serialize cleanly with no work on your part.

---

## The Extension Methods

| Method | On | Returns | Purpose |
| --- | --- | --- | --- |
| `ToJson<T>(withTypeName = false)` | any object | `JsonData` | Serialize to JSON |
| `ToData<T>()` | `JsonData` | `T` | Deserialize back to an object |
| `ToJsonAsync<T>(withTypeName = false)` | any object | `Task<JsonData>` | Serialize on a background thread |
| `ToDataAsync<T>()` | `JsonData` | `Task<T>` | Deserialize on a background thread |
| `IsValidJson()` | `string` / `TextAsset` | `bool` | Check if text is well-formed JSON |

---

## Synchronous Round-Trip
`ToJson()` returns a [`JsonData`](data-models.md) object. Its `Text` property is the JSON string;
`ToData<T>()` reads it back.

```csharp
using AceLand.Serialization.Json;
using UnityEngine;

[System.Serializable]
public class Enemy
{
    public string id;
    public int hp;
    public Color tint;          // Unity type
    public Quaternion facing;   // Unity type
}

var enemy = new Enemy
{
    id = "orc_01",
    hp = 30,
    tint = Color.green,
    facing = Quaternion.Euler(0, 90, 0),
};

JsonData data = enemy.ToJson();
string text = data.Text;             // ready to write to disk or send over the wire

Enemy restored = data.ToData<Enemy>();
```

---

## Asynchronous Round-Trip
For large objects, keep the main thread free with the async variants. They run the (de)serialization on a
thread-pool thread and cancel automatically when the application quits (they use Unity's
`Application.exitCancellationToken` internally).

```csharp
using System.Threading.Tasks;
using AceLand.Serialization.Json;

public async Task SaveAsync(Enemy enemy)
{
    JsonData data = await enemy.ToJsonAsync();
    await File.WriteAllTextAsync("enemy.json", data.Text);
}

public async Task<Enemy> LoadAsync(string path)
{
    var text = await File.ReadAllTextAsync(path);
    var data = JsonData.Builder().WithText(text).Build();
    return await data.ToDataAsync<Enemy>();
}
```

{% hint style="info" %}
The async methods return `null` from a serialization attempt that is cancelled during application shutdown,
so you don't get a spurious exception when the player quits mid-save.
{% endhint %}

---

## Validating JSON
`IsValidJson()` safely parses text and returns a `bool` — no try/catch needed on your side. It works on both
raw strings and `TextAsset`s.

```csharp
using AceLand.Serialization.Json;

if (userInput.IsValidJson())
{
    var data = JsonData.Builder().WithText(userInput).Build();
    // ...
}
```

---

## Supported Unity Types
These types are converted automatically — no attributes, no manual converter registration:

| Category | Types |
| --- | --- |
| Vectors | `Vector2`, `Vector3`, `Vector4`, `Vector2Int`, `Vector3Int` |
| Rotation / matrix | `Quaternion`, `Matrix4x4` |
| Rendering | `Color`, `Gradient`, `AnimationCurve`, `LayerMask` |
| Geometry | `Bounds`, `BoundsInt`, `Rect`, `RectInt` |
| Misc | `Hash128` |
| Unity.Mathematics | `float2/3/4`, `quaternion`, `float2x2 … float4x4` |

For example, a `Vector3` serializes to a readable object rather than an opaque blob:

```json
{ "x": 1.0, "y": 2.0, "z": 3.0 }
```

{% hint style="success" %}
Property-name matching on read is **case-insensitive**, so `{"X":1}` and `{"x":1}` both deserialize
correctly — handy when consuming JSON produced by other tools.
{% endhint %}

---

## Using the Settings Directly
Sometimes you want to call Newtonsoft.Json yourself but keep all the Unity converters. `AceSerialization`
exposes the ready-made settings and converter list.

```csharp
using AceLand.Serialization;
using Newtonsoft.Json;

// Full JsonSerializerSettings with every Unity converter pre-loaded
var settings = AceSerialization.JsonSerializerSettings;
var text = JsonConvert.SerializeObject(myObject, settings);

// Or grab just the converter list to merge into your own settings
var mySettings = new JsonSerializerSettings();
mySettings.Converters = AceSerialization.JsonConverters;
```

| Member | Description |
| --- | --- |
| `AceSerialization.JsonSerializerSettings` | Standard settings; ignores reference loops |
| `AceSerialization.JsonSerializerSettingsWithType` | Same, plus `TypeNameHandling.Objects` for polymorphism |
| `AceSerialization.JsonConverters` | A fresh `List<JsonConverter>` of all Unity converters |

---

## Polymorphic Types
When you serialize a base-typed field that actually holds a derived instance, pass `withTypeName: true` so
the concrete type is written into the JSON. Read it back with the same flag.

```csharp
// Write the concrete type into the payload
JsonData data = shape.ToJson(withTypeName: true);

// The JsonData remembers the flag, so ToData<T> uses the matching settings
Shape restored = data.ToData<Shape>();
```

{% hint style="warning" %}
`withTypeName` embeds full .NET type names into the JSON. Only enable it when you control both ends, and
never deserialize type-name JSON from an untrusted source.
{% endhint %}
