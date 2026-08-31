# CoC-Clear static-data pipeline (build-time, Sdp)

Excel (authoring) → validated CSV (committed) → Unity thin loader.

Design + type-index + dispatch model live in the design vault: `CoC-Clear/50 Dispatch Assembly.md`.

The active bootstrap contract contains `Characters`, `Scenes`, and `Episodes`. IDs are stable serialization keys; Unity resource paths are binding data, not identity.

## Layout

- `Assets/CocClear/Data/Source/CocClear_GameData.xlsx` — authoring source of truth.
- `tools/DataSchema/Records/GameRecords.cs` — **the schema, as C# records** (Sdp attributes). A column change is one edit, here.
- `Assets/CocClear/Data/Generated/*.csv` — validated extraction output. **Committed.** Unity reads these.
- Unity loader: `Assets/CocClear/Runtime/Data/` (thin CSV reader + load-time revalidation).

## Tooling — bluekms/StaticDataPipeline (Sdp), build-time CLIs only

**Never reference `Sdp.dll` inside Unity.** It targets modern .NET and is IL2CPP-incompatible. Two self-contained CLIs run on the host:

- `StaticDataHeaderGenerator` — reads `tools/DataSchema/Records/*.cs`, emits the standard header row (TSV) to paste into each Excel sheet. Keeps headers == schema.
- `ExcelColumnExtractor` — reads the records **as .cs source (Roslyn), not a compiled assembly** + the xlsx, extracts only the schema columns, validates (missing column / enum / `[Range]` / `[RegularExpression]` / FK), writes `*.csv`. **Fails the build on any violation.**

Obtain (host needs the .NET SDK pinned by that repo's `global.json`):

```
git clone https://github.com/bluekms/StaticDataPipeline.git tools/_ext/StaticDataPipeline
dotnet build -c Release tools/_ext/StaticDataPipeline
```

`tools/_ext/` is gitignored. Keep the CLIs out of `Assets/`.

CLI shape (all three are **folders**):

```
ExcelColumnExtractor -r tools/DataSchema/Records -e Assets/CocClear/Data/Source -o Assets/CocClear/Data/Generated
```

## Regen flow (after editing the xlsx or a record)

1. Columns changed? Edit `GameRecords.cs` → run `StaticDataHeaderGenerator` → paste the new header row into the sheet.
2. Run `ExcelColumnExtractor` → regenerates `Assets/CocClear/Data/Generated/*.csv`. It fails loudly on bad data.
3. Commit the CSVs. Unity revalidates on load (**second gate**) and builds the runtime catalog.

**Two gates, on purpose.** Bad data never reaches the game: once at extraction, once at load.

## Schema gotchas (paid for in Tower)

- Nullable `string?` needs `[NullString("")]`. Open-ended ints are `[NullString("")] int?`.
- Pin every header with `[ColumnName("camelCase")]`. The record conforms to the existing contract — not the other way round — so the CSV stays byte-identical and Unity needs no change.
- Enums match **by string name**. Duplicating the enum here (tool self-containment) is fine, but it must mirror the game enum exactly.
- `tools/` changes don't recompile Unity → **no gate re-run needed** for tool-only edits.
