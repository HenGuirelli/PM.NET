using System.Collections.Concurrent;

namespace PM.PmContent
{
    public class PmHeader
    {
        private static readonly ConcurrentDictionary<Type, int> _hashes = new();
        public int HeaderSize => sizeof(int);
        public int ClassHash { get; }
        public int ClassHashOffset => 0;

        public PmHeader(Type type)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));

            if (_hashes.TryGetValue(type, out var classHash))
            {
                ClassHash = classHash;
            }
            else
            {
                ClassHash = ClassHashCodeCalculator.GetHashCode(type);
                _hashes[type] = ClassHash;
            }
        }
    }
}
