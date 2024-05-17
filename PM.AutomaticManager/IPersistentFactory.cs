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

        internal PAllocator Allocator { get; set; }
        internal PMemoryManager PMemoryManager { get; set; }

        public PersistentFactory()
        {
            Allocator = new(
                PmFactory.CreatePmCSharpDefinedTypes(PmGlobalConfiguration.PmMemoryFilePath),
                PmFactory.CreatePmCSharpDefinedTypes(PmGlobalConfiguration.PmMemoryFileTransactionPath)
            );
            PMemoryManager = new(Allocator);
        }

        public object CreateRootObject(Type type, string objectUserID)
        {
            if (string.IsNullOrWhiteSpace(objectUserID))
                throw new ArgumentNullException($"{nameof(objectUserID)} cannot be null");

            if (PMemoryManager.ObjectExists(objectUserID))
            {
                Log.Information("Object user ID '{object}' found in PM. Start loading into memory...", objectUserID);
                PMemoryManager.RegisterNewObjectPropertiesInfoMapper(type);
                var persistentRegion = PMemoryManager.GetRegionByObjectUserID(objectUserID);
                var interceptor = new PmInterceptor(persistentRegion, PMemoryManager, type);
                var obj = _generator.CreateClassProxy(type, interceptor);
                Log.Information("Object user ID '{object}' load finished", objectUserID);
                return obj;
            }
            else
            {
                Log.Information("Object user ID '{object}' not found in PM. Start creation...", objectUserID);
                var persistentRegion = PMemoryManager.AllocRootObjectByType(type, objectUserID);
                var interceptor = new PmInterceptor(persistentRegion, PMemoryManager, type);
                var obj = _generator.CreateClassProxy(type, interceptor);
                Log.Information("Object user ID '{object}' creating finished", objectUserID);
                return obj;
            }
        }

        public T CreateRootObject<T>(string objectUserID)
        {
            return (T)CreateRootObject(typeof(T), objectUserID);
        }

#if DEBUG
        internal static void Purge()
        {
            File.Delete(PmGlobalConfiguration.PmMemoryFilePath);
            File.Delete(PmGlobalConfiguration.PmMemoryFileTransactionPath);
        }
#endif
    }
}
