using PM.AutomaticManager.Configs;
using PM.AutomaticManager.Factories;
using PM.AutomaticManager.Proxies;
using PM.Core.PMemory;
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

        internal PAllocator Allocator { get; set; }
        private readonly PMemoryManager _pManager;

        public PersistentFactory()
        {
            Allocator = new(
                PmFactory.CreatePmCSharpDefinedTypes(PmGlobalConfiguration.PmMemoryFilePath),
                PmFactory.CreatePmCSharpDefinedTypes(PmGlobalConfiguration.PmMemoryFileTransactionPath)
            );
            _pManager = new(Allocator);
        }

        public T CreateRootObject<T>(string objectUserID)
        {
            if (string.IsNullOrWhiteSpace(objectUserID))
                throw new ArgumentNullException($"{nameof(objectUserID)} cannot be null");

            if (_pManager.ObjectExists(objectUserID))
            {
                Log.Information("Object user ID '{object}' found in PM. Start loading into memory...", objectUserID);
                _pManager.RegisterNewObjectPropertiesInfoMapper(typeof(T));
                var persistentRegion = _pManager.GetRegionByObjectUserID(objectUserID);
                var interceptor = new PmInterceptor(persistentRegion, _pManager, typeof(T));
                var obj = _generator.CreateClassProxy(typeof(T), interceptor);
                Log.Information("Object user ID '{object}' load finished", objectUserID);
                return (T)obj;
            }
            else
            {
                Log.Information("Object user ID '{object}' not found in PM. Start creation...", objectUserID);
                var persistentRegion = _pManager.AllocRootObjectByType(typeof(T), objectUserID);
                var interceptor = new PmInterceptor(persistentRegion, _pManager, typeof(T));
                var obj = _generator.CreateClassProxy(typeof(T), interceptor);
                Log.Information("Object user ID '{object}' creating finished", objectUserID);
                return (T)obj;
            }
        }
    }
}
