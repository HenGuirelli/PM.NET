using Castle.DynamicProxy;
using PM.Proxies;
using System.Collections.Concurrent;

namespace PM
{
    internal class PmProxyGenerator
    {
        private static readonly ProxyGenerator _generator = new();
        private readonly ConcurrentDictionary<Type, ConcurrentQueue<object>> _proxyCaching = new();
        private readonly ConcurrentDictionary<Type, Task> _creationProxyTasks = new();
        private readonly ConcurrentDictionary<Type, object> _creationProxyTaskLocks = new();
        public int ProxyCacheCount { get; set; } = 100;

        public object CreateClassProxy(Type type, PersistentInterceptor persistentInterceptor)
        {
            if (_proxyCaching.TryGetValue(type, out var foundCachingItens) &&
                foundCachingItens.TryDequeue(out var cache))
            {
                return cache;
            }

            StartPopulateCache(type, persistentInterceptor);

            return _generator.CreateClassProxy(type, persistentInterceptor);
        }

        private void StartPopulateCache(
            Type type,
            PersistentInterceptor persistentInterceptor)
        {
            lock (_creationProxyTaskLocks.GetOrAdd(type, new object()))
            {
                if (_proxyCaching.TryGetValue(type, out var cachingItens))
                {
                    if (!cachingItens.IsEmpty)
                    {
                        return;
                    }
                }

                var notFoundCachingItens = _proxyCaching[type] = new ConcurrentQueue<object>();
                var t = Task.Run(() =>
                {
                    for (int i = 0; i < ProxyCacheCount; i++)
                    {
                        notFoundCachingItens.Enqueue(
                            _generator.CreateClassProxy(type, persistentInterceptor));
                    }
                });
                _creationProxyTasks[type] = t;
            }
        }
    }
}
