using System.Collections.Concurrent;

namespace PM.PmContent
{
    public class PmHeader
    {
        private static readonly ConcurrentDictionary<Type, int> _hashes = new();

        public int ClassHash { get; }
        public bool IsRootObject { get; }

        // First item, offset 0
        public int ClassHashOffset => 0;
        // Second item, offset equals a int size.
        public int RootObjectOffset => ClassHashOffset + sizeof(int);
        // Header size equals sum of all header itens
        public int HeaderSize => RootObjectOffset + sizeof(bool);

        public PmHeader(Type type, bool isRoot)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));

            if (_hashes.TryGetValue(type, out var classHash))
            {
                ClassHash = classHash;
                IsRootObject = isRoot;
            }
            else
            {
                ClassHash = ClassHashCodeCalculator.GetHashCode(type);
                IsRootObject = isRoot;
                _hashes[type] = ClassHash;
            }
        }
    }
}
