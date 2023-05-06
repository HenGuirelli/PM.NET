using PM.Collections;
using PM.Configs;
using PM.PmContent;

namespace PM.Startup
{
    public class ClassHashManager
    {
        public const string FileName = "__HashManager";

        private readonly PmList<ClassHash> _hashes = new(
            Path.Combine(
                PmGlobalConfiguration.PmInternalsFolder,
                FileName));

        public ClassHash AddHashFile(Type type, ulong pointer)
        {
            var hash = ClassHashCodeCalculator.GetHashCode(type);

            return _hashes.AddPersistent(new ClassHash
            {
                Hash = hash,
                Pointer = pointer
            });
        }

        public ClassHash? GetHashFile(Type type)
        {
            var classHash = ClassHashCodeCalculator.GetHashCode(type);
            foreach (var hash in _hashes)
            {
                if (hash.Hash == classHash)
                {
                    return hash;
                }
            }
            return null;
        }

        public void RemoveHashFile(ClassHash obj)
        {
            _hashes.Remove(obj);
        }
    }
}
