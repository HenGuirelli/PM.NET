using Castle.DynamicProxy;
using Moq;
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
                proxyGenerator.ProxyCacheCount,
                proxyGenerator.GetCacheCount(typeof(DomainObj)));
        }

        [Fact]
        public void OnCreateClassProxy_WhenUseAllCache_ShouldCreateMoreCache()
        {
            PmProxyGenerator proxyGenerator = new(100);

            for (int i = 0; i < proxyGenerator.ProxyCacheCount + 2; i++)
            {
                var obj = proxyGenerator.CreateClassProxy(
                    typeof(DomainObj),
                    Mock.Of<IInterceptor>());
            }

            Assert.Equal(
                proxyGenerator.ProxyCacheCount,
                proxyGenerator.GetCacheCount(typeof(DomainObj)));
        }
    }
}
