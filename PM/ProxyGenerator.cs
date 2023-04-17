using Castle.DynamicProxy;
using System.Collections.Concurrent;

namespace PM
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
        public int TotalProxyCreatedCount => _totalProxyCreatedCount;

        private static readonly ProxyGenerator _generator = new();
        private readonly ConcurrentDictionary<Type, ConcurrentQueue<CacheItem>> _proxyCaching = new();
        private readonly ConcurrentDictionary<Type, Task> _creationProxyTasks = new();
        private readonly ConcurrentDictionary<Type, object> _creationProxyTaskLocks = new();
        private readonly StandardInterceptor _standardInterceptor = new();

        public PmProxyGenerator(int proxyCacheCount = 100)
        {
            MinProxyCacheCount = proxyCacheCount;
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
            _reuseCacheCount++;
        }

        public object CreateClassProxy(Type type, IInterceptor interceptor)
        {
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
                notFoundCachingItens = _proxyCaching[type] = new ConcurrentQueue<CacheItem>();
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
                        new CacheItem(this, proxy, type)
                    );
                    _totalProxyCreatedCount++;
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
        private readonly IProxyTargetAccessor _proxyTargetAccessor;
        private readonly Type _type;

        public CacheItem(PmProxyGenerator pmProxyGenerator, object proxy, Type type)
        {
            _pmProxyGenerator = pmProxyGenerator;
            _proxyTargetAccessor = (IProxyTargetAccessor)proxy;
            _type = type;
            Proxy = proxy;
        }

        public void SetInterceptor(IInterceptor interceptor)
        {
            var interceptors = _proxyTargetAccessor.GetInterceptors();
            for (int i = 0; i < interceptors.Length; i++)
            {
                interceptors[i] = null;
            }
            interceptors[0] = interceptor;
        }

        ~CacheItem()
        {
            GC.ReRegisterForFinalize(this);
            _pmProxyGenerator.EnqueueCache(_type, this);
        }
    }
}
