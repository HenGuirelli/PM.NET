using Castle.DynamicProxy;
using PM.CastleHelpers;
using PM.Managers;
using PM.Proxies;
using System.Collections.Concurrent;
using System.Diagnostics;

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
        private readonly IDictionary<ulong, ulong> _pointers;

        public int TotalProxyCreatedCount => _totalProxyCreatedCount;

        private static readonly ProxyGenerator _generator = new();
        private readonly ConcurrentDictionary<Type, ConcurrentQueue<CacheItem>> _proxyCaching = new();
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

        internal void EnqueueCache(Type type, CacheItem cacheItem)
        {
            var queue = _proxyCaching.AddOrUpdate(type,
                new ConcurrentQueue<CacheItem>(),
                (key, value) => value);

            queue.Enqueue(cacheItem);
            _reuseCacheCount++;
        }

        public object CreateClassProxy(Type type, IPmInterceptor interceptor)
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

        private static DateTime _startTimeApplication;
        private static Stopwatch _totalTimeOnGC = new();

        private static readonly Thread _thread;

        static CacheItem()
        {
            _startTimeApplication = DateTime.Now;
            _thread = new Thread(() =>
            {
                while (true)
                {
                    var totalTimeApplication = DateTime.Now - _startTimeApplication;
                    Console.WriteLine(
                        $"Total time app:\t{totalTimeApplication.TotalMilliseconds} " +
                        $"Total time GC:\t{_totalTimeOnGC.ElapsedMilliseconds}");

                    Thread.Sleep(5000);
                }
            });
            _thread.Name = "CacheItem thread";
            _thread.Start();
        }

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
            _totalTimeOnGC.Start();
            try
            {
                var interceptor = CastleManager.GetInterceptor(Proxy);

                if (interceptor!.FilePointerCount == 0 &&
                    interceptor!.FilePointer.EndsWith(PmExtensions.PmInternalFile))
                {
                    FileHandlerManager.ReleaseObjectFromMemory(interceptor!.PmMemoryMappedFile);
                    FileHandlerManager.CloseAndRemoveFile(interceptor!.PmMemoryMappedFile);
                }
            }
            catch (Exception ex)
            {
                // TODO: log error
            }

            if (_pmProxyGenerator.GetCacheCount(_type) < _pmProxyGenerator.MinProxyCacheCount)
            {
                GC.ReRegisterForFinalize(this);
                _pmProxyGenerator.EnqueueCache(_type, this);
            }

            _totalTimeOnGC.Stop();
        }
    }
}
