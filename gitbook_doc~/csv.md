# CSV

Read CSV files and text assets into clean, structured rows — quoted fields included.

## Overview
CSV support lives in the `AceLand.Serialization.CSV` namespace as extension methods on
[`PathData`](data-models.md) and Unity's `TextAsset`. There are two styles:

- **Streaming rows** — iterate row-by-row as `string[]` with `ReadAsCsv()`.
- **Structured data** — load everything into a [`CsvData`](data-models.md) object with `ReadAsCsvData()`.

Both understand quoted fields (`"a,b"` is one field, not two) and let you skip a header line.

---

## The Extension Methods

| Method | On | Returns | Purpose |
| --- | --- | --- | --- |
| `ReadAsCsv(hasHeader = false)` | `PathData` / `TextAsset` | `IEnumerable<string[]>` | Stream each row as fields |
| `ReadAsCsvData(hasHeader = false)` | `PathData` / `TextAsset` | `CsvData` | Load the whole table into one object |

---

## Streaming Rows
`ReadAsCsv()` yields one `string[]` per non-empty line. This is the lightest option for large files because
it never builds the whole table in memory.

{% tabs %}
{% tab title="From a file" %}
```csharp
using AceLand.Serialization.CSV;
using AceLand.Serialization.Models;
using UnityEngine;

var path = PathData.Builder()
    .WithPath(Application.streamingAssetsPath, "enemies.csv")
    .Build();

// hasHeader: true skips the first line
foreach (var fields in path.ReadAsCsv(hasHeader: true))
{
    var id = fields[0];
    var hp = int.Parse(fields[1]);
    Debug.Log($"{id} has {hp} HP");
}
```
{% endtab %}

{% tab title="From a TextAsset" %}
```csharp
using AceLand.Serialization.CSV;
using UnityEngine;

public class CsvLoader : MonoBehaviour
{
    public TextAsset csv;   // dropped in from the Inspector

    private void Start()
    {
        foreach (var fields in csv.ReadAsCsv(hasHeader: true))
            Debug.Log(string.Join(" | ", fields));
    }
}
```
{% endtab %}
{% endtabs %}

{% hint style="info" %}
When reading from a `PathData` that doesn't exist, `ReadAsCsv()` logs an error and yields nothing instead of
throwing — so a missing file won't crash your loop.
{% endhint %}

---

## Structured Data
`ReadAsCsvData()` returns a [`CsvData`](data-models.md) that keeps the header (if any), exposes the row count
and column count, and lets you index into rows directly.

```csharp
using AceLand.Serialization.CSV;
using AceLand.Serialization.Models;
using UnityEngine;

var path = PathData.Builder()
    .WithPath(Application.streamingAssetsPath, "enemies.csv")
    .Build();

CsvData table = path.ReadAsCsvData(hasHeader: true);

Debug.Log($"Columns: {table.ColumnCount}, Rows: {table.LineCount}");

if (table.HasHeader)
    Debug.Log("Header: " + string.Join(", ", table.Header));

// Index a row, then a field
string[] firstRow = table[0];
Debug.Log(firstRow[0]);

// Or iterate all data rows (header excluded)
foreach (var row in table.Lines)
    Debug.Log(string.Join(" | ", row));
```

| `CsvData` member | Description |
| --- | --- |
| `Header` | The header fields (empty array when `hasHeader` is false) |
| `HasHeader` | Whether a header row was provided |
| `Lines` | All data rows (header excluded) |
| `this[int index]` | The row at `index` as a `string[]` |
| `ColumnCount` | Expected number of columns |
| `LineCount` | Number of data rows |

---

## Quoted Fields
The parser follows common CSV quoting rules, so commas and escaped quotes inside a quoted field are
preserved:

```
name,note
"Smith, John","He said ""hi"""
```

reads back as two fields per row:

```
["name", "note"]
["Smith, John", "He said \"hi\""]
```

{% hint style="warning" %}
Rows whose column count doesn't match the first row are skipped with a warning when building a `CsvData`.
Keep every row the same width to avoid silently dropping data.
{% endhint %}

---

## Building CsvData Manually
You can also construct a `CsvData` in code via its builder — useful for tests or generating tables at
runtime. See [Data Models](data-models.md#csvdata) for the full builder walkthrough.

```csharp
using AceLand.Serialization.Models;

var table = CsvData.Builder()
    .WithHeaderFields("id", "hp")
    .WithFields("orc_01", "30")
    .WithFields("orc_02", "45")
    .Build();
```
