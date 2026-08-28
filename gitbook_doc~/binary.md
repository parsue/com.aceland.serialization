# Binary

A `BinaryFormatter` that already knows how to serialize Unity's built-in structs.

## Overview
`AceSerialization.GetBinaryFormatter()` returns a fully configured `IFormatter` whose `SurrogateSelector`
is pre-loaded with **serialization surrogates** for the common Unity value types. A surrogate is a small
helper that tells `BinaryFormatter` how to break a type down into simple fields and rebuild it — because
Unity structs like `Vector3` aren't marked `[Serializable]` in a way the binary formatter can use directly.

The result: you can binary-serialize objects that contain Unity types without writing any of that plumbing
yourself.

---

## Getting a Formatter
```csharp
using System.IO;
using AceLand.Serialization;
using UnityEngine;

var formatter = AceSerialization.GetBinaryFormatter();

// Save
using (var stream = File.Create("save.dat"))
    formatter.Serialize(stream, new Vector3(10, 20, 30));

// Load
using (var stream = File.OpenRead("save.dat"))
{
    var position = (Vector3)formatter.Deserialize(stream);
    Debug.Log(position); // (10.00, 20.00, 30.00)
}
```

Each call to `GetBinaryFormatter()` returns a fresh, independent formatter with its own surrogate selector,
so it's safe to use one per save/load operation.

---

## Serializing Your Own Types
Your class only needs the standard `[System.Serializable]` attribute. Any Unity-type fields inside it are
handled by the surrogates automatically.

```csharp
using System;
using System.IO;
using AceLand.Serialization;
using UnityEngine;

[Serializable]
public class SaveGame
{
    public string playerName;
    public int score;
    public Vector3 position;     // handled by surrogate
    public Quaternion rotation;  // handled by surrogate
    public Color favouriteColor; // handled by surrogate
}

public static class SaveSystem
{
    public static void Save(SaveGame data, string path)
    {
        var formatter = AceSerialization.GetBinaryFormatter();
        using var stream = File.Create(path);
        formatter.Serialize(stream, data);
    }

    public static SaveGame Load(string path)
    {
        var formatter = AceSerialization.GetBinaryFormatter();
        using var stream = File.OpenRead(path);
        return (SaveGame)formatter.Deserialize(stream);
    }
}
```

---

## Supported Unity Types
The formatter ships with surrogates for:

| Category | Types |
| --- | --- |
| Vectors | `Vector2`, `Vector3`, `Vector4`, `Vector2Int`, `Vector3Int` |
| Rotation / matrix | `Quaternion`, `Matrix4x4` |
| Rendering | `Color`, `Gradient`, `AnimationCurve`, `LayerMask` |
| Geometry | `Bounds`, `BoundsInt`, `Rect`, `RectInt` |
| Misc | `Hash128` |

---

## When to Use Binary vs JSON

| | Binary | JSON |
| --- | --- | --- |
| Human-readable | ❌ | ✅ |
| Compact | ✅ | ➖ |
| Safe across versions | ➖ | ✅ |
| Good for networking | ❌ | ✅ |
| Good for local saves | ✅ | ✅ |

{% hint style="warning" %}
`BinaryFormatter` is powerful but should only be used with **trusted, local** data. Never deserialize binary
data that came from an untrusted source (downloads, other players, servers you don't control) — it can be a
security risk. For anything crossing a trust boundary, use [JSON](json.md).
{% endhint %}

{% hint style="info" %}
Binary saves are tied to your type layout. If you rename fields or restructure classes between game
versions, old saves may fail to load. For long-lived, upgradable save data, JSON is usually the safer
choice.
{% endhint %}
