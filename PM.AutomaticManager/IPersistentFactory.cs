using PM.AutomaticManager.Configs;
using PM.AutomaticManager.Factories;
using PM.AutomaticManager.Proxies;
using PM.FileEngine;
using Serilog;

namespace PM.AutomaticManager
{
    public interface IPersistentFactory
    {
        T CreateRootObject<T>(string pmFilename);
    }

    public class PersistentFactory : IPersistentFactory
    {
        private readonly PmProxyGenerator _generator = new(proxyCacheCount: PmGlobalConfiguration.ProxyCacheCount);
        private readonly Dictionary<string, object> _rootObjectsCache = new();

        private static PAllocator _allocator;
        private static PMemoryManager _pMemoryManager;

        public PAllocator Allocator => _allocator;
        public PMemoryManager PMemoryManager => _pMemoryManager;

        static PersistentFactory()
        {
            _allocator = new(
               PmFactory.CreatePmCSharpDefinedTypes(PmGlobalConfiguration.PmMemoryFilePath),
               PmFactory.CreatePmCSharpDefinedTypes(PmGlobalConfiguration.PmMemoryFileTransactionPath)
           );
            _pMemoryManager = new(_allocator);
        }

        public object CreateRootObject(Type type, string objectUserID)
        {
            if (string.IsNullOrWhiteSpace(objectUserID))
                throw new ArgumentNullException($"{nameof(objectUserID)} cannot be null");

            if (_rootObjectsCache.TryGetValue(objectUserID, out var rootObj))
            {
                return rootObj;
            }

            if (PMemoryManager.ObjectExists(objectUserID))
            {
                Log.Debug("Object user ID '{object}' found in PM. Start loading into memory...", objectUserID);
                PMemoryManager.RegisterNewObjectPropertiesInfoMapper(type);
                var persistentRegion = PMemoryManager.GetRegionByObjectUserID(objectUserID);
                var interceptor = new PmInterceptor(persistentRegion, PMemoryManager, type)
                {
                    IsRootObject = true
                };
                var obj = _generator.CreateClassProxy(type, interceptor);
                Log.Debug("Object user ID '{object}' load finished", objectUserID);
                return obj;
            }
            else
            {
                Log.Debug("Object user ID '{object}' not found in PM. Start creation...", objectUserID);
                var persistentRegion = PMemoryManager.AllocRootObjectByType(type, objectUserID);
                var interceptor = new PmInterceptor(persistentRegion, PMemoryManager, type)
                {
                    IsRootObject = true
                };
                var obj = _generator.CreateClassProxy(type, interceptor);
                Log.Debug("Object user ID '{object}' creating finished", objectUserID);
                return obj;
            }
        }

        public T CreateRootObject<T>(string objectUserID)
        {
            return (T)CreateRootObject(typeof(T), objectUserID);
        }
    }
}
