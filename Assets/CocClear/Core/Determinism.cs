// CocClear.Core — engine-free. This assembly has noEngineReferences: true, so
// `UnityEngine.Random` is not merely discouraged here, it is unreachable.
//
// Every procedural / simulation path seeds from here. The gate proves it:
// same seed, same sequence, forever. (Playbook G4)

namespace CocClear.Core
{
    /// <summary>FNV-1a 32-bit. Stable across runs, platforms, and .NET versions.</summary>
    public static class Hash
    {
        private const uint Offset = 2166136261u;
        private const uint Prime = 16777619u;

        public static uint Fnv1a(string value)
        {
            var hash = Offset;
            for (var i = 0; i < value.Length; i++)
            {
                hash ^= value[i];
                hash *= Prime;
            }

            return hash;
        }

        public static uint Combine(uint a, uint b)
        {
            var hash = a ^ Offset;
            hash *= Prime;
            hash ^= b;
            hash *= Prime;
            return hash;
        }
    }

    /// <summary>xorshift32. Deterministic, allocation-free, seeded explicitly.</summary>
    public struct DeterministicRandom
    {
        private uint state;

        public DeterministicRandom(uint seed)
        {
            // xorshift dies on zero; remap it rather than silently degrading.
            state = seed == 0u ? Offset : seed;
        }

        private const uint Offset = 2166136261u;

        public static DeterministicRandom FromKey(string key) => new DeterministicRandom(Hash.Fnv1a(key));

        public uint NextUInt()
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }

        /// <summary>[minInclusive, maxExclusive)</summary>
        public int Range(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                return minInclusive;
            }

            var span = (uint)(maxExclusive - minInclusive);
            return minInclusive + (int)(NextUInt() % span);
        }

        public float Next01() => (NextUInt() >> 8) * (1.0f / 16777216.0f);
    }
}
