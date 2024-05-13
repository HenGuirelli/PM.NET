using Castle.DynamicProxy;
using Serilog;
using System.Diagnostics;

namespace PM.AutomaticManager.Proxies
{
    public class ProxyGeneratorCacheItem
    {
        public object Proxy { get; }

        private readonly PmProxyGenerator _pmProxyGenerator;
        private readonly IProxyTargetAccessor _proxyTargetAccessor;
        private readonly Type _type;

        private static readonly DateTime _startTimeApplication;
        private static readonly Stopwatch _totalTimeOnGC = new();

        private static readonly Thread _thread;

        static ProxyGeneratorCacheItem()
        {
            _startTimeApplication = DateTime.Now;
            _thread = new Thread(() =>
            {
                while (true)
                {
                    var totalTimeApplication = DateTime.Now - _startTimeApplication;
                    Log.Information(
                        "Total time app:\t{TotalMilliseconds} " +
                        "Total time GC:\t{ElapsedMilliseconds}",
                        totalTimeApplication.TotalMilliseconds,
                        _totalTimeOnGC.ElapsedMilliseconds);

                    Thread.Sleep(5000);
                }
            });
            _thread.Name = "CacheItem thread";
            _thread.Start();
        }

        public ProxyGeneratorCacheItem(PmProxyGenerator pmProxyGenerator, object proxy, Type type)
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

        ~ProxyGeneratorCacheItem()
        {
            //if (PmGlobalConfiguration.PersistentGCEnable)
            //{
            //    _totalTimeOnGC.Start();
            //    try
            //    {
            //        var interceptor = CastleManager.GetInterceptor(Proxy);

            //        if (interceptor!.FilePointerCount == 0 &&
            //            interceptor!.FilePointer.EndsWith(PmExtensions.PmInternalFile))
            //        {
            //            Log.Verbose("Removing file {filepath} of object {@obj}",
            //                interceptor!.FilePointer, Proxy);
            //            FileHandlerManager.CloseAndRemoveFile(interceptor!.PmMemoryMappedFile);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.Error(ex, "Error on destructor object");
            //    }

            //    if (_pmProxyGenerator.GetCacheCount(_type) < _pmProxyGenerator.MinProxyCacheCount)
            //    {
            //        GC.ReRegisterForFinalize(this);
            //        _pmProxyGenerator.EnqueueCache(_type, this);
            //    }

            //    _totalTimeOnGC.Stop();
            //}
        }
    }
}
