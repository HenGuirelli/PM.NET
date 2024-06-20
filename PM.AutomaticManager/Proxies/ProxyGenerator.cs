using Castle.DynamicProxy;
using System.Collections.Concurrent;

namespace PM.AutomaticManager.Proxies
{
    public class PmProxyGenerator
    {
        public int MinProxyCacheCount { get; }

        private volatile int _hitCount;
        public int HitCount => _hitCount;

        private volatile int _missCount;
        public int MissCount => _missCount;

        private volatile int _reuseCacheCount;
        public int ReuseCacheCount => _reuseCacheCount;

        private volatile int _totalProxyCreatedCount;
        private readonly IDictionary<ulong, ulong> _pointers;

        public int TotalProxyCreatedCount => _totalProxyCreatedCount;

        private static readonly ProxyGenerator _generator = new();
        private readonly ConcurrentDictionary<Type, ConcurrentQueue<ProxyGeneratorCacheItem>> _proxyCaching = new();
        private readonly ConcurrentDictionary<Type, Task> _creationProxyTasks = new();
        private readonly ConcurrentDictionary<Type, object> _creationProxyTaskLocks = new();
        private readonly StandardInterceptor _standardInterceptor = new();

        public PmProxyGenerator(
            IDictionary<ulong, ulong> initialPointers = null,
            int proxyCacheCount = 500)
        {
            MinProxyCacheCount = proxyCacheCount;
            _pointers = initialPointers;
        }

        public int GetCacheCount(Type type)
        {
            return _proxyCaching[type]?.Count ?? 0;
        }

        internal void EnqueueCache(Type type, ProxyGeneratorCacheItem cacheItem)
        {
            var queue = _proxyCaching.AddOrUpdate(type,
                new ConcurrentQueue<ProxyGeneratorCacheItem>(),
                (key, value) => value);

            queue.Enqueue(cacheItem);
            _reuseCacheCount++;
        }

        public object CreateClassProxy(Type type, IInterceptor interceptor)
        {
            if (MinProxyCacheCount == 0)
            {
                var obj = Activator.CreateInstance(type);
                var proxy= _generator.CreateClassProxyWithTarget(type, obj, _standardInterceptor);
                var cacheItem = new ProxyGeneratorCacheItem(this, proxy, type);
                cacheItem.SetInterceptor(interceptor);
                return cacheItem.Proxy;
            }

            if (_proxyCaching.TryGetValue(type, out var foundCachingItens) &&
                foundCachingItens.TryDequeue(out var cache))
            {
                if (foundCachingItens.IsEmpty)
                {
                    StartPopulateCache(type);
                    _hitCount--;
                }

                cache.SetInterceptor(interceptor);
                _hitCount++;
                return cache.Proxy;
            }

            StartPopulateCache(type);
            _hitCount--;

            return CreateClassProxy(type, interceptor);
        }

        private void StartPopulateCache(Type type)
        {
            lock (_creationProxyTaskLocks.GetOrAdd(type, new object()))
            {
                if (_proxyCaching.TryGetValue(type, out var cachingItens) &&
                    _creationProxyTasks.TryGetValue(type, out var task))
                {
                    if (task.IsCompleted && cachingItens.IsEmpty)
                    {
                        Task t1 = StartNewTask(type);
                        _creationProxyTasks[type] = t1;
                        _missCount++;
                        return;
                    }
                    return;
                }

                Task t2 = StartNewTask(type);
                _creationProxyTasks[type] = t2;
                _missCount++;
            }
        }

        private Task StartNewTask(Type type)
        {
            if (!_proxyCaching.TryGetValue(type, out var notFoundCachingItens))
            {
                notFoundCachingItens = _proxyCaching[type] = new ConcurrentQueue<ProxyGeneratorCacheItem>();
            }
            var firstItemQueued = false;
            var t = Task.Run(() =>
            {
                // ProxyCacheCount + 1 because the first object is
                // immediatly dequeued to return to caller.
                for (int i = 0; i < MinProxyCacheCount + 1; i++)
                {
                    var obj = Activator.CreateInstance(type);
                    var proxy = _generator.CreateClassProxyWithTarget(type, obj, _standardInterceptor);
                    notFoundCachingItens.Enqueue(
                        new ProxyGeneratorCacheItem(this, proxy, type)
                    );
                    _totalProxyCreatedCount++;
                    firstItemQueued = true;
                }
            });
            SpinWait.SpinUntil(() => firstItemQueued);
            return t;
        }
    }
}
