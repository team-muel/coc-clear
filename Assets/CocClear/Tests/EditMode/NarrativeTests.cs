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

            Assert.Greater(sequence.Count, 8);
            Assert.IsTrue(sequence.Current.IsNarration);
            Assert.IsTrue(sequence.Current.Text.Contains("가끔 그런 날"));
            sequence.SetIndex(sequence.Count - 1);
            Assert.IsTrue(sequence.Current.IsNarration);
            Assert.IsTrue(sequence.Current.Text.Contains("업무를 담당"));
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
            Assert.AreEqual("나", sequence.Current.Speaker);
            Assert.IsTrue(sequence.Current.Text.Contains("몇시"));
        }

        [Test]
        public void Progress_ExposesEveryReachedLineForBacklog()
        {
            var sequence = new NarrativeSequence(PrologueScript.Create());

            sequence.SetIndex(2);
            var visited = sequence.GetLinesThroughCurrent();

            Assert.AreEqual(3, visited.Length);
            Assert.IsTrue(visited[0].IsNarration);
            Assert.IsTrue(visited[1].IsNarration);
            Assert.AreEqual(sequence.Current.Text, visited[2].Text);
        }

        [Test]
        public void Archive_ContainsEveryPrologueLine()
        {
            var records = ScenarioArchive.CreateAll();

            Assert.AreEqual(PrologueScript.Create().Length, records.Length);
            Assert.AreEqual("프롤로그", records[0].EpisodeTitle);
            Assert.AreEqual(1, records[0].Order);
            Assert.IsTrue(records[0].Line.IsNarration);
        }

        [Test]
        public void Narration_HasNoSpeakerWhileQuotedLinesKeepTheirSpeaker()
        {
            var narration = NarrativeLine.Narration("서술문", "corridor");
            var dialogue = new NarrativeLine("나", "대사");

            Assert.IsTrue(narration.IsNarration);
            Assert.AreEqual(string.Empty, narration.Speaker);
            Assert.AreEqual("corridor", narration.SceneId);
            Assert.IsFalse(dialogue.IsNarration);
            Assert.AreEqual("나", dialogue.Speaker);
        }

        [Test]
        public void Prologue_ContainsSceneChangesForExistingBackgroundArt()
        {
            var lines = PrologueScript.Create();

            Assert.AreEqual("bedroom", lines[0].SceneId);
            Assert.AreEqual("corridor", lines[7].SceneId);
            Assert.AreEqual("corridor", lines[19].SceneId);
            Assert.AreEqual("corridor-crowd", lines[20].SceneId);
            Assert.AreEqual(SceneTransitionStyle.FastBottomToTop, lines[7].SceneTransition);
            Assert.AreEqual(SceneTransitionStyle.FastBottomToTop, lines[20].SceneTransition);
        }

        [Test]
        public void Archive_GroupsLogsByPrologueAndChapters()
        {
            CollectionAssert.AreEqual(new[] { "프롤로그", "1장", "2장", "3장" }, ScenarioArchive.EpisodeTitles);
            Assert.AreEqual(PrologueScript.Create().Length, ScenarioArchive.CreateForEpisode("프롤로그").Length);
            Assert.AreEqual(0, ScenarioArchive.CreateForEpisode("1장").Length);
            Assert.AreEqual(0, ScenarioArchive.CreateForEpisode("2장").Length);
            Assert.AreEqual(0, ScenarioArchive.CreateForEpisode("3장").Length);
        }
    }
}
