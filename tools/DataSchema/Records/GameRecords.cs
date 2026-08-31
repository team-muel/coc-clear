// CoC-Clear static-data SCHEMA — single source of truth.
// Consumed by the Sdp host CLIs at build time. This file is not compiled by Unity.
// Stable IDs are serialized contracts; Unity resource paths are resolved by the thin loader.

using Sdp.Attributes;

namespace CocClear.DataSchema
{
    public enum SceneTransitionStyle
    {
        None,
        FastBottomToTop,
    }

    [StaticDataRecord("CocClear_GameData", "Characters")]
    public sealed record CharacterRecord(
        [ColumnName("id")] string Id,
        [ColumnName("displayName")] string DisplayName,
        [ColumnName("defaultExpressionId")] string DefaultExpressionId,
        [ColumnName("portraitResource")] string PortraitResource);

    [StaticDataRecord("CocClear_GameData", "Scenes")]
    public sealed record SceneRecord(
        [ColumnName("id")] string Id,
        [ColumnName("backgroundResource")] string BackgroundResource,
        [ColumnName("defaultTransition")] SceneTransitionStyle DefaultTransition);

    [StaticDataRecord("CocClear_GameData", "Episodes")]
    public sealed record EpisodeRecord(
        [ColumnName("id")] string Id,
        [ColumnName("displayName")] string DisplayName,
        [ColumnName("order")][Range(0, 999)] int Order,
        [ColumnName("scriptId")] string ScriptId);
}
