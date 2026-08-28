# Data Models

The building blocks behind every serialization call.

## Overview

AceLand Serialization ships three lightweight, immutable data models: `JsonData`, `CsvData`, and `PathData`. Each one hides its constructor and is created through a fluent **builder**, so you always end up with a validated, consistent object. This page explains how to construct each model by hand and what members they expose.

{% hint style="info" %}
In everyday usage you rarely build these by hand — the extension methods (`ToJson`, `ReadAsCsvData`, etc.) create them for you. Build them manually only when you are composing data from scratch or writing custom pipelines.
{% endhint %}

---

## JsonData

`JsonData` wraps a parsed JSON document. It always keeps both a Newtonsoft `JContainer` (the object tree) and the serialized `Text` in sync, so you can move between the two representations freely.

### Members

| Member | Type | Description |
| --- | --- | --- |
| `Container` | `JContainer` | The Newtonsoft object/array tree. |
| `Text` | `string` | The serialized JSON string. |
| `WithTypeName` | `bool` | Whether `$type` metadata is embedded. |

### Builder

The builder is a two-step fluent flow: first choose the **content source**, then optionally toggle type-name handling, then `Build()`.

{% tabs %}
{% tab title="From an object tree" %}
```csharp
using AceLand.Serialization.Models;
using Newtonsoft.Json.Linq;

var container = new JObject
{
    ["name"] = "Aria",
    ["level"] = 12,
};

var jsonData = JsonData.Builder()
    .WithJContainer(container)
    .Build();

Debug.Log(jsonData.Text); // {"name":"Aria","level":12}
```
{% endtab %}

{% tab title="From a string" %}
```csharp
using AceLand.Serialization.Models;

var jsonData = JsonData.Builder()
    .WithText("{\"name\":\"Aria\",\"level\":12}")
    .Build();

// Container is parsed for you
var name = jsonData.Container["name"];
```
{% endtab %}

{% tab title="From a TextAsset" %}
```csharp
using AceLand.Serialization.Models;
using UnityEngine;

public class LoadFromAsset : MonoBehaviour
{
    [SerializeField] private TextAsset saveFile;

    private void Start()
    {
        var jsonData = JsonData.Builder()
            .WithText(saveFile)
            .WithTypeName() // opt-in to $type metadata
            .Build();
    }
}
```
{% endtab %}
{% endtabs %}

{% hint style="warning" %}
Call `WithTypeName()` on **both** ends of a round-trip. If the JSON was written with type names, read it back with `WithTypeName()` too, otherwise polymorphic types will not resolve correctly. See [JSON Serialization](json.md) for details.
{% endhint %}

---

## CsvData

`CsvData` represents a parsed CSV table with an optional header row and strict column-count validation.

### Members

| Member | Type | Description |
| --- | --- | --- |
| `Header` | `string[]` | Header fields, or an empty array when there is no header. |
| `HasHeader` | `bool` | Whether the first row was treated as a header. |
| `Lines` | `IEnumerable<string[]>` | All data rows (excludes the header). |
| `this[int]` | `string[]` | Random access to a data row by index. |
| `ColumnCount` | `int` | Number of columns enforced across all rows. |
| `LineCount` | `int` | Number of data rows. |

### Builder

The builder enforces a header decision **first**, then accepts rows. Each row can be supplied either as a raw CSV line (`WithLine`) that is parsed for you, or as pre-split fields (`WithFields`).

{% tabs %}
{% tab title="With a header" %}
```csharp
using AceLand.Serialization.Models;

var csv = CsvData.Builder()
    .WithHeaderFields("id", "name", "score")
    .WithFields("1", "Aria", "990")
    .WithFields("2", "Bran", "875")
    .Build();

Debug.Log(csv.ColumnCount); // 3
Debug.Log(csv.LineCount);   // 2
Debug.Log(csv[0][1]);       // Aria
```
{% endtab %}

{% tab title="From raw lines" %}
```csharp
using AceLand.Serialization.Models;

var csv = CsvData.Builder()
    .WithHeader("id,name,score")
    .WithLine("1,Aria,990")
    .WithLine("2,Bran,875")
    .Build();

foreach (var row in csv.Lines)
    Debug.Log(string.Join(" | ", row));
```
{% endtab %}

{% tab title="Without a header" %}
```csharp
using AceLand.Serialization.Models;

var csv = CsvData.Builder()
    .WithoutHeader()
    .WithFields("1", "Aria", "990")
    .WithFields("2", "Bran", "875")
    .Build();

Debug.Log(csv.HasHeader); // False
```
{% endtab %}
{% endtabs %}

{% hint style="warning" %}
The first row added sets the column count. Any later row with a different number of columns is **rejected** and logs a `Column count mismatch` warning — the mismatched row is simply skipped rather than throwing. See [CSV Reading](csv.md) for more.
{% endhint %}

---

## PathData

`PathData` is a normalized, comparable representation of a fully-qualified file path. It resolves to an absolute path, trims trailing separators (while preserving the root), and provides case-correct comparison per platform (case-insensitive on Windows, case-sensitive elsewhere).

### Members

| Member | Type | Description |
| --- | --- | --- |
| `FullPath` | `string` | The absolute, resolved path. |
| `Filename` | `string` | File name including extension. |
| `FilenameWithoutExtension` | `string` | File name without extension. |
| `FileExtension` | `string` | The extension including the leading dot. |

`PathData` also implements `IComparable`, `IComparable<string>`, `IComparable<PathData>`, and `IEquatable<PathData>`, supports `==` / `!=`, and has an **implicit conversion to `string`** (returning `FullPath`).

### Builder

`WithPath` accepts one or more path parts that are combined with the correct separator.

{% tabs %}
{% tab title="Single path" %}
```csharp
using AceLand.Serialization.Models;
using UnityEngine;

var path = PathData.Builder()
    .WithPath(Application.persistentDataPath, "saves", "slot1.dat")
    .Build();

Debug.Log(path.Filename);                 // slot1.dat
Debug.Log(path.FilenameWithoutExtension); // slot1
Debug.Log(path.FileExtension);            // .dat
```
{% endtab %}

{% tab title="Use as a string" %}
```csharp
using AceLand.Serialization.Models;
using System.IO;
using UnityEngine;

var path = PathData.Builder()
    .WithPath(Application.persistentDataPath, "config.json")
    .Build();

// implicit conversion to string — pass directly to System.IO APIs
if (File.Exists(path))
{
    var text = File.ReadAllText(path);
}
```
{% endtab %}

{% tab title="Comparison" %}
```csharp
using AceLand.Serialization.Models;
using UnityEngine;

var a = PathData.Builder().WithPath(Application.persistentDataPath, "a.txt").Build();
var b = PathData.Builder().WithPath(Application.persistentDataPath, "a.txt").Build();

Debug.Log(a == b); // True — value equality on the resolved path
```
{% endtab %}
{% endtabs %}

{% hint style="info" %}
`PathData` only supports **fully-qualified** paths. If the combined parts do not resolve to an absolute path, `Build()` logs an error and returns `null`. Prefer anchoring on `Application.persistentDataPath` or another absolute root.
{% endhint %}

`PathData` is the primary input for [CSV Reading](csv.md) and file-based [Binary Serialization](binary.md).

---

## Best Practices

- Let the extension methods build these models for you; reach for the builders only when composing data from scratch.
- Always pair `WithTypeName()` on both write and read when using polymorphic JSON.
- Keep CSV rows uniform in width to avoid dropped rows from the column-count guard.
- Anchor `PathData` on an absolute root such as `Application.persistentDataPath`, and lean on its implicit `string` conversion to interop with `System.IO`.
