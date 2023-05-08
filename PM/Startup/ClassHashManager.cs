using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
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

        private static ClassHashManager? _instance;
        private static readonly object _instanceLock = new();
        public static ClassHashManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_instanceLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ClassHashManager();
                        }
                    }
                }
                return _instance;
            }
        }

        public PmList<ClassHash> All => _hashes;

        private ClassHashManager() { }

        public ClassHash AddHashFile(Type type)
        {
            var typeAlreadyExists = GetHashFile(type);
            if (typeAlreadyExists != null)
            {
                return typeAlreadyExists;
            }


            var hash = ClassHashCodeCalculator.GetHashCode(type);

            var pClassHash = _hashes.AddPersistent(new ClassHash());
            pClassHash.Hash = hash;
            pClassHash.AssemblyName = type.Assembly.FullName!;
            pClassHash.SerializedType = type.FullName!;

            return pClassHash;
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
