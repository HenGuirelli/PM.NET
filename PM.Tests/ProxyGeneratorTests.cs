using Castle.DynamicProxy;
using Moq;
using PM.Core;
using PM.Proxies;
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
                Mock.Of<IPmInterceptor>());

            Assert.NotNull(obj);
        }

        [Fact]
        public void OnCreateClassProxy_ShouldCreateCache()
        {
            PmProxyGenerator proxyGenerator = new();
            var obj = proxyGenerator.CreateClassProxy(
                typeof(DomainObj),
                Mock.Of<IPmInterceptor>());

            Thread.Sleep(200);

            Assert.Equal(
                proxyGenerator.MinProxyCacheCount,
                proxyGenerator.GetCacheCount(typeof(DomainObj)));
        }

        [Fact]
        public void OnCreateClassProxy_WhenUseAllCache_ShouldCreateMoreCache()
        {
            PmProxyGenerator proxyGenerator = new(proxyCacheCount: 100);

            for (int i = 0; i < proxyGenerator.MinProxyCacheCount + 2; i++)
            {
                var obj = proxyGenerator.CreateClassProxy(
                    typeof(DomainObj),
                    Mock.Of<IPmInterceptor>());
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
            /// (there is only 100 item in the queue).
            /// 
            /// In this case, when the GC passes, it must replace the proxies already
            /// used in the cache again.
            ///

            PmProxyGenerator proxyGenerator = new(proxyCacheCount: 100);

            for (int i = 0; i < (proxyGenerator.MinProxyCacheCount + 10); i++)
            {
                var obj = proxyGenerator.CreateClassProxy(
                    typeof(DomainObj),
                    Mock.Of<IPmInterceptor>());

                if (proxyGenerator.GetCacheCount(typeof(DomainObj)) == 1)
                {
                    GC.Collect();
                    Thread.Sleep(500);
                }
            }

            // Reuse must be at least half of proxy cache length
            Assert.True(
                proxyGenerator.ReuseCacheCount >=
                proxyGenerator.MinProxyCacheCount / 2
            );
        }

        [Fact]
        public void OnCreateClassProxy_WhenGcCollectProxies_ShouldDeletePmFile()
        {
            PmProxyGenerator proxyGenerator = new(proxyCacheCount: 50);
            var isDeleted = false;
            var fileBasedStreamMock = new Mock<FileBasedStream>();
            fileBasedStreamMock.Setup(x => x.Delete())
                .Callback(() =>
                {
                    isDeleted = true;
                });

            var interceptorMock = new Mock<IPmInterceptor>();
            interceptorMock.SetupGet(x => x.PmMemoryMappedFile)
                .Returns(fileBasedStreamMock.Object);

            var obj = proxyGenerator.CreateClassProxy(
                typeof(DomainObj),
                interceptorMock.Object);

            obj = null;

            // Call GC to collect "obj"
            GC.Collect();
            // Wait collect
            Thread.Sleep(500);

            // Reuse must be at least half of proxy cache length
            Assert.True(isDeleted);
        }
    }
}
