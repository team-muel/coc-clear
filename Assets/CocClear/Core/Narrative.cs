using System;

namespace CocClear.Core
{
    /// <summary>Engine-free, linear narrative data. Runtime rendering is intentionally separate.</summary>
    public readonly struct NarrativeLine
    {
        public NarrativeLine(string speaker, string text)
        {
            Speaker = speaker ?? throw new ArgumentNullException(nameof(speaker));
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public string Speaker { get; }
        public string Text { get; }
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
    }

    /// <summary>Temporary playable prologue distilled from the vault's current scenario notes.</summary>
    public static class PrologueScript
    {
        public static NarrativeLine[] Create()
        {
            return new[]
            {
                new NarrativeLine("나", "오늘도 퇴근은 미뤄졌다. TF 도서판매부의 보고서는 아직 끝나지 않았다."),
                new NarrativeLine("김유인", "대리님, 이 표만 확인해 주시면 됩니다. 커피는 제가 사 올게요."),
                new NarrativeLine("나", "아니요. 제가 마무리하죠. 내일 일정이 꼬이면 모두가 더 힘들어집니다."),
                new NarrativeLine("김민희", "그래도 너무 무리하지 마세요. 일은 도망가지 않으니까."),
                new NarrativeLine("안내 방송", "전 직원께 알립니다. 즉시 건물 밖으로 대피해 주십시오."),
                new NarrativeLine("나", "창밖이… 왜 저렇게 뿌옇지?"),
                new NarrativeLine("김유인", "대리님, 숨을 쉬면 안 돼요. 밖에서 독가스가 들어오고 있어요!"),
                new NarrativeLine("나", "도시가 멈췄다. 그리고 우리가 알던 일상도, 여기서 끝났다."),
            };
        }
    }
}
