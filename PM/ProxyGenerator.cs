using Castle.DynamicProxy;
using System.Collections.Concurrent;

namespace PM
{
    public class PmProxyGenerator
    {
        private static readonly ProxyGenerator _generator = new();
        private readonly ConcurrentDictionary<Type, ConcurrentQueue<object>> _proxyCaching = new();
        private readonly ConcurrentDictionary<Type, Task> _creationProxyTasks = new();
        private readonly ConcurrentDictionary<Type, object> _creationProxyTaskLocks = new();
        public int ProxyCacheCount { get; }

        public PmProxyGenerator(int proxyCacheCount = 100)
        {
            ProxyCacheCount = proxyCacheCount;
        }

        public int GetCacheCount(Type type)
        {
            return _proxyCaching[type]?.Count ?? 0;
        }

        public object CreateClassProxy(Type type, IInterceptor interceptor)
        {
            if (_proxyCaching.TryGetValue(type, out var foundCachingItens) &&
                foundCachingItens.TryDequeue(out var cache))
            {
                return cache;
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
            var notFoundCachingItens = _proxyCaching[type] = new ConcurrentQueue<object>();
            var firstItemQueued = false;
            var t = Task.Run(() =>
            {
                // ProxyCacheCount + 1 because the first object is
                // immediatly dequeued to return to caller.
                for (int i = 0; i < ProxyCacheCount + 1; i++)
                {
                    notFoundCachingItens.Enqueue(
                        _generator.CreateClassProxy(type, interceptor));
                    firstItemQueued = true;
                }
            });
            SpinWait.SpinUntil(() => firstItemQueued);
            return t;
        }
    }
}
