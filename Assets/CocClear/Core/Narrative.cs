using System;

namespace CocClear.Core
{
    public enum SceneTransitionStyle
    {
        None,
        FastBottomToTop,
    }

    /// <summary>Engine-free, linear narrative data. Runtime rendering is intentionally separate.</summary>
    public readonly struct NarrativeLine
    {
        public NarrativeLine(string speaker, string text, string sceneId = null, SceneTransitionStyle sceneTransition = SceneTransitionStyle.None)
        {
            Speaker = speaker ?? throw new ArgumentNullException(nameof(speaker));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            SceneId = sceneId ?? string.Empty;
            SceneTransition = sceneTransition;
        }

        public string Speaker { get; }
        public string Text { get; }
        public string SceneId { get; }
        public SceneTransitionStyle SceneTransition { get; }
        public bool IsNarration => string.IsNullOrEmpty(Speaker);

        public static NarrativeLine Narration(string text, string sceneId = null, SceneTransitionStyle sceneTransition = SceneTransitionStyle.None)
        {
            return new NarrativeLine(string.Empty, text, sceneId, sceneTransition);
        }
    }

    /// <summary>Bounds-safe progress cursor for a sequence of dialogue lines.</summary>
    public sealed class NarrativeSequence
    {
        private readonly NarrativeLine[] lines;

        public NarrativeSequence(NarrativeLine[] lines)
        {
            if (lines == null || lines.Length == 0)
            {
                throw new ArgumentException("A narrative sequence needs at least one line.", nameof(lines));
            }

            this.lines = (NarrativeLine[])lines.Clone();
        }

        public int Count => lines.Length;
        public int CurrentIndex { get; private set; }
        public NarrativeLine Current => lines[CurrentIndex];
        public bool IsFirst => CurrentIndex == 0;
        public bool IsLast => CurrentIndex == lines.Length - 1;

        public bool MoveNext()
        {
            if (IsLast)
            {
                return false;
            }

            CurrentIndex++;
            return true;
        }

        public bool MovePrevious()
        {
            if (IsFirst)
            {
                return false;
            }

            CurrentIndex--;
            return true;
        }

        public void SetIndex(int index)
        {
            CurrentIndex = Math.Max(0, Math.Min(index, lines.Length - 1));
        }

        /// <summary>Returns a snapshot of every line the player has reached, including the current line.</summary>
        public NarrativeLine[] GetLinesThroughCurrent()
        {
            var visitedLines = new NarrativeLine[CurrentIndex + 1];
            Array.Copy(lines, visitedLines, visitedLines.Length);
            return visitedLines;
        }
    }

    /// <summary>Playable prologue transcribed from the vault's current scenario notes.</summary>
    public static class PrologueScript
    {
        public static NarrativeLine[] Create()
        {
            return new[]
            {
                NarrativeLine.Narration("가끔 그런 날이 있다. 뭘 해도 잘 안 풀리는 그런 날.", "bedroom"),
                NarrativeLine.Narration("오늘 따라 그런 날이 될 것 같다는 예감이 드는 것을 깨달았을 때."),
                NarrativeLine.Narration("따르르르릉-!"),
                new NarrativeLine("나", "어...알람 시계?"),
                new NarrativeLine("나", "몇시길래 그러냐...어?"),
                NarrativeLine.Narration("난 아침을 대차게 망치고 말았다."),
                new NarrativeLine("나", "8시???"),
                NarrativeLine.Narration("햇살이 비치는 창문 너머로 본격적인 하루가 시작되었다...아마도.", "corridor", SceneTransitionStyle.FastBottomToTop),
                NarrativeLine.Narration("사실 본격적인 시작은 일어난 순간부터 하긴 했다."),
                NarrativeLine.Narration("평소엔 7시에 일어나서 8시 20분까지 널널하게 시간을 잡고 가던 회사를 8시부터 시작했으니, 당연한 일이었다."),
                NarrativeLine.Narration("결국 어찌저찌 왔으니 다행이긴 한가..."),
                new NarrativeLine("나", "흐아아암..."),
                NarrativeLine.Narration("그렇게 길게 하품을 내뱉고 있을 즈음, 경비원 분께서 말을 건네셨다."),
                new NarrativeLine("경비원", "어이고, 오늘은 늦으실 뻔하셨네요?"),
                new NarrativeLine("나", "오늘따라 버스가 많이 늦더라고요. 그래도 지각은 안했으니 다행이죠."),
                NarrativeLine.Narration("대답을 들은 경비원 분은 허허 웃으시며 잘 들어가라고 하셨다."),
                NarrativeLine.Narration("이런 말을 듣고도 그냥 갈 수는 없지."),
                new NarrativeLine("나", "먼저 가보겠습니다! 좋은 하루 보내세요!"),
                NarrativeLine.Narration("유쾌하게 말하고선 안 쪽으로 향했다."),
                NarrativeLine.Narration("아침에 그렇게 힘들게 왔지만 그래도 회사 안은 평화로워서 편안했다.", "corridor"),
                NarrativeLine.Narration("얼마나 걸어왔을까, 저 앞에 본격적인 업무를 담당하는 사람들이 보였다.", "corridor-crowd", SceneTransitionStyle.FastBottomToTop),
            };
        }
    }

    /// <summary>One entry visible in the archive. Add every new episode to ScenarioArchive.CreateAll.</summary>
    public readonly struct ArchiveRecord
    {
        public ArchiveRecord(string episodeTitle, int order, NarrativeLine line)
        {
            EpisodeTitle = episodeTitle ?? throw new ArgumentNullException(nameof(episodeTitle));
            Order = order;
            Line = line;
        }

        public string EpisodeTitle { get; }
        public int Order { get; }
        public NarrativeLine Line { get; }
    }

    /// <summary>Single source for every scenario record shown in the in-game archive.</summary>
    public static class ScenarioArchive
    {
        public static readonly string[] EpisodeTitles =
        {
            "프롤로그",
            "1장",
            "2장",
            "3장",
        };

        public static ArchiveRecord[] CreateAll()
        {
            var prologue = PrologueScript.Create();
            var records = new ArchiveRecord[prologue.Length];
            for (var i = 0; i < prologue.Length; i++)
            {
                records[i] = new ArchiveRecord(EpisodeTitles[0], i + 1, prologue[i]);
            }

            return records;
        }

        public static ArchiveRecord[] CreateForEpisode(string episodeTitle)
        {
            if (episodeTitle == null)
            {
                throw new ArgumentNullException(nameof(episodeTitle));
            }

            var allRecords = CreateAll();
            var count = 0;
            for (var i = 0; i < allRecords.Length; i++)
            {
                if (allRecords[i].EpisodeTitle == episodeTitle)
                {
                    count++;
                }
            }

            var episodeRecords = new ArchiveRecord[count];
            var targetIndex = 0;
            for (var i = 0; i < allRecords.Length; i++)
            {
                if (allRecords[i].EpisodeTitle == episodeTitle)
                {
                    episodeRecords[targetIndex++] = allRecords[i];
                }
            }

            return episodeRecords;
        }
    }
}
