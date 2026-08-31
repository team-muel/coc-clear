using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using CocClear.Core;
using UnityEngine;

namespace CocClear.Runtime.Data
{
    public sealed class CharacterData
    {
        public CharacterData(string id, string displayName, string defaultExpressionId, string portraitResource)
        {
            Id = id;
            DisplayName = displayName;
            DefaultExpressionId = defaultExpressionId;
            PortraitResource = portraitResource;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string DefaultExpressionId { get; }
        public string PortraitResource { get; }
    }

    public sealed class SceneData
    {
        public SceneData(string id, string backgroundResource, SceneTransitionStyle defaultTransition)
        {
            Id = id;
            BackgroundResource = backgroundResource;
            DefaultTransition = defaultTransition;
        }

        public string Id { get; }
        public string BackgroundResource { get; }
        public SceneTransitionStyle DefaultTransition { get; }
    }

    public sealed class EpisodeData
    {
        public EpisodeData(string id, string displayName, int order, string scriptId)
        {
            Id = id;
            DisplayName = displayName;
            Order = order;
            ScriptId = scriptId;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public int Order { get; }
        public string ScriptId { get; }
    }

    /// <summary>Immutable runtime view of build-time validated static content.</summary>
    public sealed class StaticDataCatalog
    {
        private StaticDataCatalog(
            IDictionary<string, CharacterData> characters,
            IDictionary<string, SceneData> scenes,
            IDictionary<string, EpisodeData> episodes)
        {
            Characters = new ReadOnlyDictionary<string, CharacterData>(characters);
            Scenes = new ReadOnlyDictionary<string, SceneData>(scenes);
            Episodes = new ReadOnlyDictionary<string, EpisodeData>(episodes);
        }

        public IReadOnlyDictionary<string, CharacterData> Characters { get; }
        public IReadOnlyDictionary<string, SceneData> Scenes { get; }
        public IReadOnlyDictionary<string, EpisodeData> Episodes { get; }

        public static StaticDataCatalog FromTextAssets(TextAsset characters, TextAsset scenes, TextAsset episodes)
        {
            if (characters == null) throw new ArgumentNullException(nameof(characters));
            if (scenes == null) throw new ArgumentNullException(nameof(scenes));
            if (episodes == null) throw new ArgumentNullException(nameof(episodes));
            return Parse(characters.text, scenes.text, episodes.text);
        }

        public static StaticDataCatalog Parse(string charactersCsv, string scenesCsv, string episodesCsv)
        {
            var characters = new Dictionary<string, CharacterData>(StringComparer.Ordinal);
            foreach (var row in CsvTable.Parse("Characters", charactersCsv, "id", "displayName", "defaultExpressionId", "portraitResource"))
            {
                var id = Required("Characters", row, 0);
                AddUnique(characters, id, new CharacterData(
                    id,
                    Required("Characters", row, 1),
                    Required("Characters", row, 2),
                    Required("Characters", row, 3)));
            }

            var scenes = new Dictionary<string, SceneData>(StringComparer.Ordinal);
            foreach (var row in CsvTable.Parse("Scenes", scenesCsv, "id", "backgroundResource", "defaultTransition"))
            {
                var id = Required("Scenes", row, 0);
                SceneTransitionStyle transition;
                if (!Enum.TryParse(Required("Scenes", row, 2), false, out transition))
                {
                    throw new FormatException("Scenes[" + id + "] has an unknown defaultTransition.");
                }

                AddUnique(scenes, id, new SceneData(id, Required("Scenes", row, 1), transition));
            }

            var episodes = new Dictionary<string, EpisodeData>(StringComparer.Ordinal);
            foreach (var row in CsvTable.Parse("Episodes", episodesCsv, "id", "displayName", "order", "scriptId"))
            {
                var id = Required("Episodes", row, 0);
                int order;
                if (!int.TryParse(Required("Episodes", row, 2), NumberStyles.Integer, CultureInfo.InvariantCulture, out order)
                    || order < 0 || order > 999)
                {
                    throw new FormatException("Episodes[" + id + "] order must be between 0 and 999.");
                }

                AddUnique(episodes, id, new EpisodeData(
                    id,
                    Required("Episodes", row, 1),
                    order,
                    Required("Episodes", row, 3)));
            }

            return new StaticDataCatalog(characters, scenes, episodes);
        }

        private static string Required(string table, string[] row, int index)
        {
            if (string.IsNullOrWhiteSpace(row[index]))
            {
                throw new FormatException(table + " contains an empty required value at column " + index + ".");
            }

            return row[index];
        }

        private static void AddUnique<T>(IDictionary<string, T> table, string id, T value)
        {
            if (table.ContainsKey(id))
            {
                throw new FormatException("Duplicate static-data id: " + id);
            }

            table.Add(id, value);
        }

        private static class CsvTable
        {
            public static IList<string[]> Parse(string table, string csv, params string[] expectedHeader)
            {
                if (csv == null) throw new ArgumentNullException(nameof(csv));
                var rows = ParseRows(csv);
                if (rows.Count == 0)
                {
                    throw new FormatException(table + " CSV is empty.");
                }

                var header = rows[0];
                if (header.Length != expectedHeader.Length)
                {
                    throw new FormatException(table + " header column count does not match the schema.");
                }

                for (var i = 0; i < expectedHeader.Length; i++)
                {
                    if (!string.Equals(header[i], expectedHeader[i], StringComparison.Ordinal))
                    {
                        throw new FormatException(table + " header mismatch at column " + i + ".");
                    }
                }

                var data = new List<string[]>();
                for (var i = 1; i < rows.Count; i++)
                {
                    if (rows[i].Length == 1 && rows[i][0].Length == 0)
                    {
                        continue;
                    }

                    if (rows[i].Length != expectedHeader.Length)
                    {
                        throw new FormatException(table + " row " + (i + 1) + " has the wrong column count.");
                    }

                    data.Add(rows[i]);
                }

                return data;
            }

            private static List<string[]> ParseRows(string csv)
            {
                var rows = new List<string[]>();
                var row = new List<string>();
                var field = new StringBuilder();
                var quoted = false;

                for (var i = 0; i < csv.Length; i++)
                {
                    var c = csv[i];
                    if (quoted)
                    {
                        if (c == '"' && i + 1 < csv.Length && csv[i + 1] == '"')
                        {
                            field.Append('"');
                            i++;
                        }
                        else if (c == '"')
                        {
                            quoted = false;
                        }
                        else
                        {
                            field.Append(c);
                        }
                    }
                    else if (c == '"')
                    {
                        quoted = true;
                    }
                    else if (c == ',')
                    {
                        row.Add(field.ToString());
                        field.Length = 0;
                    }
                    else if (c == '\n')
                    {
                        row.Add(field.ToString());
                        field.Length = 0;
                        rows.Add(row.ToArray());
                        row.Clear();
                    }
                    else if (c != '\r')
                    {
                        field.Append(c);
                    }
                }

                if (quoted)
                {
                    throw new FormatException("CSV has an unterminated quoted field.");
                }

                if (field.Length > 0 || row.Count > 0)
                {
                    row.Add(field.ToString());
                    rows.Add(row.ToArray());
                }

                return rows;
            }
        }
    }
}
