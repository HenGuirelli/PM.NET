using Moq;
using PM.Configs;
using PM.Proxies;
using System;
using System.Threading;
using Xunit;

namespace PM.Tests
{


    public class InnerClass
    {
        public virtual int MyProperty { get; set; }
    }

    public class RootObject
    {
        public virtual int IntVal { get; set; }

        public virtual long LongVal { get; set; }

        public virtual short ShortVal { get; set; }

        public virtual byte ByteVal { get; set; }

        public virtual double DoubleVal { get; set; }

        public virtual float FloatVal { get; set; }

        public virtual decimal DecimalVal { get; set; }

        public virtual char CharVal { get; set; }

        public virtual bool BoolVal { get; set; }

        public virtual string StringVal { get; set; }
        public virtual InnerClass InnerObject { get; set; }
    }

    public class DomainObj
    {
    }

    public class ProxyGeneratorTests
    {
        [Fact]
        public void AAA()
        {
            PmGlobalConfiguration.PmTarget = Core.PmTargets.TraditionalMemoryMappedFile;

            IPersistentFactory _persistentFactorySSD = new PersistentFactory();
            var _proxy = _persistentFactorySSD.CreateRootObject<RootObject>("RootObj");
            _proxy.InnerObject = new InnerClass();
        }

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
    }
}
