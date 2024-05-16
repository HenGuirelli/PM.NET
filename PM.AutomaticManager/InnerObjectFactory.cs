using PM.AutomaticManager.Configs;
using PM.AutomaticManager.Proxies;
using PM.Core.PMemory;

namespace PM.AutomaticManager
{
    internal class InnerObjectFactory
    {
        private readonly PmProxyGenerator _generator = new(proxyCacheCount: PmGlobalConfiguration.ProxyCacheCount);
        private readonly PMemoryManager _pMemoryManager;

        public InnerObjectFactory(PMemoryManager pMemoryManager)
        {
            _pMemoryManager = pMemoryManager;
        }

        public object CreateInnerObject(PersistentRegion persistentRegion, Type type)
        {
            var interceptor = new PmInterceptor(persistentRegion, _pMemoryManager, type);
            return _generator.CreateClassProxy(type, interceptor);
        }
    }
}
