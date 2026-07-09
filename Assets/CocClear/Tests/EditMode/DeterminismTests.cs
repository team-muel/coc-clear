using System.Text;
using CocClear.Core;
using NUnit.Framework;

namespace CocClear.Tests
{
    /// <summary>
    /// The gate's first citizen. If this ever goes red, procedural output has
    /// stopped being reproducible and every bug report becomes unfalsifiable.
    /// </summary>
    public sealed class DeterminismTests
    {
        [Test]
        public void SameSeed_ProducesIdenticalSequence()
        {
            var a = new DeterministicRandom(12345u);
            var b = new DeterministicRandom(12345u);

            for (var i = 0; i < 1000; i++)
            {
                Assert.AreEqual(a.NextUInt(), b.NextUInt(), $"diverged at draw {i}");
            }
        }

        [Test]
        public void DifferentSeeds_Diverge()
        {
            var a = new DeterministicRandom(1u);
            var b = new DeterministicRandom(2u);
            Assert.AreNotEqual(a.NextUInt(), b.NextUInt());
        }

        [Test]
        public void ZeroSeed_DoesNotCollapse()
        {
            var r = new DeterministicRandom(0u);
            Assert.AreNotEqual(0u, r.NextUInt());
        }

        [Test]
        public void Fnv1a_IsStableAcrossRuns()
        {
            // Pinned literal. If this changes, every persisted seed changes with it.
            Assert.AreEqual(0x811C9DC5u ^ 0u, Hash.Fnv1a(string.Empty));
            Assert.AreEqual(Hash.Fnv1a("floor:1"), Hash.Fnv1a("floor:1"));
            Assert.AreNotEqual(Hash.Fnv1a("floor:1"), Hash.Fnv1a("floor:2"));
        }

        [Test]
        public void SeededGeneration_HashesIdenticallyTwice()
        {
            Assert.AreEqual(GenerateSignature("run-seed"), GenerateSignature("run-seed"));
        }

        private static string GenerateSignature(string key)
        {
            var rng = DeterministicRandom.FromKey(key);
            var sb = new StringBuilder();
            for (var i = 0; i < 64; i++)
            {
                sb.Append(rng.Range(0, 100)).Append(',');
            }

            return sb.ToString();
        }
    }
}
