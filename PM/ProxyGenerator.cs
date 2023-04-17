using Castle.DynamicProxy;
using System.Collections.Concurrent;

namespace PM
{
    public class PmProxyGenerator
    {
        public int ProxyCacheCount { get; }

        private static readonly ProxyGenerator _generator = new();
        private readonly ConcurrentDictionary<Type, ConcurrentQueue<CacheItem>> _proxyCaching = new();
        private readonly ConcurrentDictionary<Type, Task> _creationProxyTasks = new();
        private readonly ConcurrentDictionary<Type, object> _creationProxyTaskLocks = new();

        public PmProxyGenerator(int proxyCacheCount = 100)
        {
            ProxyCacheCount = proxyCacheCount;
        }

        public int GetCacheCount(Type type)
        {
            return _proxyCaching[type]?.Count ?? 0;
        }

        internal void EnqueueCache(Type type, CacheItem cacheItem)
        {
            var queue = _proxyCaching.AddOrUpdate(type,
                new ConcurrentQueue<CacheItem>(), 
                (key, value) => value);

            queue.Enqueue(cacheItem);
        }

        public object CreateClassProxy(Type type, IInterceptor interceptor)
        {
            if (_proxyCaching.TryGetValue(type, out var foundCachingItens) &&
                foundCachingItens.TryDequeue(out var cache))
            {
                if (foundCachingItens.IsEmpty)
                    StartPopulateCache(type, interceptor);

                return cache.Proxy;
            }

            StartPopulateCache(type, interceptor);

            return CreateClassProxy(type, interceptor);
        }

        private void StartPopulateCache(
            Type type,
            IInterceptor interceptor)
        {
            lock (_creationProxyTaskLocks.GetOrAdd(type, new object()))
            {
                if (_proxyCaching.TryGetValue(type, out var cachingItens) &&
                    _creationProxyTasks.TryGetValue(type, out var task))
                {
                    if (task.IsCompleted && cachingItens.IsEmpty)
                    {
                        Task t1 = StartNewTask(type, interceptor);
                        _creationProxyTasks[type] = t1;
                        return;
                    }
                    return;
                }

                Task t2 = StartNewTask(type, interceptor);
                _creationProxyTasks[type] = t2;
            }
        }

        private Task StartNewTask(Type type, IInterceptor interceptor)
        {
            if (!_proxyCaching.TryGetValue(type, out var notFoundCachingItens))
            {
                notFoundCachingItens = _proxyCaching[type] = new ConcurrentQueue<CacheItem>();
            }
            var firstItemQueued = false;
            var t = Task.Run(() =>
            {
                // ProxyCacheCount + 1 because the first object is
                // immediatly dequeued to return to caller.
                for (int i = 0; i < ProxyCacheCount + 1; i++)
                {
                    var proxy = _generator.CreateClassProxyWithTarget(type, new object(), interceptor);
                    notFoundCachingItens.Enqueue(
                        new CacheItem(this, proxy, type)
                        );
                    firstItemQueued = true;
                }
            });
            SpinWait.SpinUntil(() => firstItemQueued);
            return t;
        }
    }

    public class CacheItem
    {
        public object Proxy { get; }

        private readonly PmProxyGenerator _pmProxyGenerator;
        private readonly Type _type;

        public CacheItem(PmProxyGenerator pmProxyGenerator, object proxy, Type type)
        {
            _pmProxyGenerator = pmProxyGenerator;
            _type = type;
            Proxy = proxy;
        }

        ~CacheItem()
        {
            GC.ReRegisterForFinalize(this);
            _pmProxyGenerator.EnqueueCache(_type, this);
        }
    }
}
