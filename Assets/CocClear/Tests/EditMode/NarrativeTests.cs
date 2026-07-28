using CocClear.Core;
using NUnit.Framework;

namespace CocClear.Tests
{
    public sealed class NarrativeTests
    {
        [Test]
        public void Prologue_HasPlayableOpeningAndClosingLines()
        {
            var sequence = new NarrativeSequence(PrologueScript.Create());

            Assert.AreEqual(8, sequence.Count);
            Assert.AreEqual("나", sequence.Current.Speaker);
            sequence.SetIndex(sequence.Count - 1);
            Assert.IsTrue(sequence.Current.Text.Contains("일상"));
        }

        [Test]
        public void Progress_DoesNotMoveOutsideSequence()
        {
            var sequence = new NarrativeSequence(PrologueScript.Create());

            Assert.IsFalse(sequence.MovePrevious());
            sequence.SetIndex(999);
            Assert.IsTrue(sequence.IsLast);
            Assert.IsFalse(sequence.MoveNext());
        }

        [Test]
        public void Progress_CanBeRestoredFromSavedIndex()
        {
            var sequence = new NarrativeSequence(PrologueScript.Create());

            sequence.SetIndex(4);
            Assert.AreEqual("안내 방송", sequence.Current.Speaker);
            Assert.IsTrue(sequence.Current.Text.Contains("대피"));
        }
    }
}
