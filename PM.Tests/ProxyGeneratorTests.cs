using Castle.DynamicProxy;
using Moq;
using System;
using System.Threading;
using Xunit;

namespace PM.Tests
{
    public class DomainObj
    {
    }

    public class ProxyGeneratorTests
    {
        [Fact]
        public void OnCreateClassProxy_ShouldCreateObject()
        {
            PmProxyGenerator proxyGenerator = new();
            var obj = proxyGenerator.CreateClassProxy(
                typeof(DomainObj),
                Mock.Of<IInterceptor>());

            Assert.NotNull(obj);
        }

        [Fact]
        public void OnCreateClassProxy_ShouldCreateCache()
        {
            PmProxyGenerator proxyGenerator = new();
            var obj = proxyGenerator.CreateClassProxy(
                typeof(DomainObj),
                Mock.Of<IInterceptor>());

            Thread.Sleep(200);

            Assert.Equal(
                proxyGenerator.MinProxyCacheCount,
                proxyGenerator.GetCacheCount(typeof(DomainObj)));
        }

        [Fact]
        public void OnCreateClassProxy_WhenUseAllCache_ShouldCreateMoreCache()
        {
            PmProxyGenerator proxyGenerator = new(100);

            for (int i = 0; i < proxyGenerator.MinProxyCacheCount + 2; i++)
            {
                var obj = proxyGenerator.CreateClassProxy(
                    typeof(DomainObj),
                    Mock.Of<IInterceptor>());
            }

            Assert.Equal(
                proxyGenerator.MinProxyCacheCount,
                proxyGenerator.GetCacheCount(typeof(DomainObj)));
        }

        [Fact]
        public void OnCreateClassProxy_WhenGcCollectProxies_ShouldReuseProxy()
        {
            ///
            /// This test calls the GC when the cache queue is fully consumed 
            /// (there is only one item in the queue).
            /// 
            /// In this case, when the GC passes, it must replace the proxies already
            /// used in the cache again.
            ///

            PmProxyGenerator proxyGenerator = new(100);

            for (int i = 0; i < (proxyGenerator.MinProxyCacheCount + 10); i++)
            {
                var obj = proxyGenerator.CreateClassProxy(
                    typeof(DomainObj),
                    Mock.Of<IInterceptor>());

                if (proxyGenerator.GetCacheCount(typeof(DomainObj)) == 1)
                {
                    GC.Collect();
                    Thread.Sleep(500);
                }
            }

            Assert.Equal(
                proxyGenerator.MinProxyCacheCount,
                proxyGenerator.ReuseCacheCount);
        }
    }
}
