// CoC-Clear static-data SCHEMA — single source of truth.
//
// Consumed by the Sdp CLIs (StaticDataHeaderGenerator + ExcelColumnExtractor) at
// BUILD TIME to (a) generate/sync Excel headers and (b) extract + validate
// Excel -> CSV. This file targets modern .NET and is NOT compiled into Unity.
// Unity reads the resulting validated CSV via a thin loader.
//
// One record == one Excel sheet row.
//   [StaticDataRecord(file, sheet)]  binds the record to a workbook sheet.
//   [ColumnName("camelCase")]        pins the header. The record conforms to the
//                                    CSV contract, never the reverse.
//   [Range]                          validated at extract AND at load.
//   [NullString("")]                 empty cell means null (required on nullable).
//
// Column change = edit here. The extractor then fails loudly if Excel drifts.
//
// STATUS: placeholder. The game's domain is not decided yet (see vault ledger A1).
// The record below exists so the pipeline is exercised end-to-end from day one.
// Replace it — don't build around it.

using Sdp.Attributes;

namespace CocClear.DataSchema
{
    public enum Rarity { Common, Rare, Epic }

    [StaticDataRecord("CocClear_GameData", "Items")]
    public sealed record ItemRecord(
        [ColumnName("id")] string Id,
        [ColumnName("displayName")] string DisplayName,
        [ColumnName("rarity")] Rarity Rarity,
        [ColumnName("price")][Range(0, 1_000_000)] int Price,
        [ColumnName("description")][NullString("")] string? Description);
}
