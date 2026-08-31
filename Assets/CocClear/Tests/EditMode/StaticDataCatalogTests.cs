using System;
using CocClear.Core;
using CocClear.Runtime.Data;
using NUnit.Framework;

namespace CocClear.Tests
{
    public sealed class StaticDataCatalogTests
    {
        private const string Characters = "id,displayName,defaultExpressionId,portraitResource\nkim-yuin,김유인,kim-yuin.normal,Characters/KimYuin\n";
        private const string Scenes = "id,backgroundResource,defaultTransition\nbedroom,Backgrounds/PrologueBedroom,None\ncorridor,Backgrounds/CleanCorridor,FastBottomToTop\n";
        private const string Episodes = "id,displayName,order,scriptId\nprologue,프롤로그,0,prologue\n";

        [Test]
        public void Parse_BuildsImmutableBootstrapCatalog()
        {
            var catalog = StaticDataCatalog.Parse(Characters, Scenes, Episodes);

            Assert.AreEqual("김유인", catalog.Characters["kim-yuin"].DisplayName);
            Assert.AreEqual("Backgrounds/CleanCorridor", catalog.Scenes["corridor"].BackgroundResource);
            Assert.AreEqual(SceneTransitionStyle.FastBottomToTop, catalog.Scenes["corridor"].DefaultTransition);
            Assert.AreEqual("prologue", catalog.Episodes["prologue"].ScriptId);
        }

        [Test]
        public void Parse_RejectsDuplicateIds()
        {
            var duplicateCharacters = Characters + "kim-yuin,다른 이름,kim-yuin.normal,Characters/KimYuin\n";

            Assert.Throws<FormatException>(() => StaticDataCatalog.Parse(duplicateCharacters, Scenes, Episodes));
        }

        [Test]
        public void Parse_RejectsMissingRequiredBinding()
        {
            const string invalidScenes = "id,backgroundResource,defaultTransition\nbedroom,,None\n";

            Assert.Throws<FormatException>(() => StaticDataCatalog.Parse(Characters, invalidScenes, Episodes));
        }

        [Test]
        public void Parse_RejectsUnknownTransition()
        {
            const string invalidScenes = "id,backgroundResource,defaultTransition\nbedroom,Backgrounds/PrologueBedroom,DiagonalWipe\n";

            Assert.Throws<FormatException>(() => StaticDataCatalog.Parse(Characters, invalidScenes, Episodes));
        }

        [Test]
        public void Parse_SupportsQuotedFieldsAndEscapedQuotes()
        {
            const string quotedCharacters = "id,displayName,defaultExpressionId,portraitResource\nkim-yuin,\"김, \"\"유인\"\"\",kim-yuin.normal,Characters/KimYuin\n";

            var catalog = StaticDataCatalog.Parse(quotedCharacters, Scenes, Episodes);

            Assert.AreEqual("김, \"유인\"", catalog.Characters["kim-yuin"].DisplayName);
        }
    }
}
