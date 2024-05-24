using PM.AutomaticManager.Configs;
using PM.AutomaticManager.Proxies;
using PM.AutomaticManager.Tests.TestObjects;
using PM.Tests.Common;
using Xunit.Abstractions;

namespace PM.AutomaticManager.Tests
{

    public class TestOutputHelperMock : ITestOutputHelper
    {
        private readonly ITestOutputHelper _original;

        public string Messages { get; private set; } = string.Empty;

        public TestOutputHelperMock(ITestOutputHelper original)
        {
            _original = original;
        }

        public void WriteLine(string message)
        {
            Messages += message;
            _original.WriteLine(message);
        }

        public void WriteLine(string format, params object[] args)
        {
            Messages += string.Format(format, args);
            _original.WriteLine(format, args);
        }
    }

    public class GarbageCollectorTests : UnitTest
    {
        private readonly TestOutputHelperMock _outputMock;

        public GarbageCollectorTests(ITestOutputHelper output)
            : base(new TestOutputHelperMock(output))
        {
            _outputMock = (TestOutputHelperMock)base.Output;
        }

        [Fact]
        public void OnGcCollect_ShouldMarkAsFreeRegion()
        {
#if DEBUG
            PersistentFactory.Purge();
#endif

            PmGlobalConfiguration.PmTarget = Core.PmTargets.TraditionalMemoryMappedFile;

            var factory = new PersistentFactory();
            var proxyObj = factory.CreateRootObject<RootClass>(nameof(OnGcCollect_ShouldMarkAsFreeRegion));

            proxyObj.InnerObject1 = new InnerClass1();
            proxyObj.InnerObject2 = new InnerClass1();

            uint blockId1 = 0;
            byte regionIndex1 = 0;
            if (CastleManager.TryGetCastleProxyInterceptor(proxyObj.InnerObject1, out var pmInterceptor1))
            {
                blockId1 = pmInterceptor1.PersistentRegion.BlockID;
                regionIndex1 = pmInterceptor1.PersistentRegion.RegionIndex;
            }
            uint blockId2 = 0;
            byte regionIndex2 = 0;
            if (CastleManager.TryGetCastleProxyInterceptor(proxyObj.InnerObject2, out var pmInterceptor2))
            {
                blockId2 = pmInterceptor2.PersistentRegion.BlockID;
                regionIndex2 = pmInterceptor2.PersistentRegion.RegionIndex;
            }

            proxyObj.InnerObject1 = null;
            proxyObj.InnerObject2 = null;

            GC.Collect();
            Thread.Sleep(1000);

            var expectedLog1 = $"Removing persistent object at BlockId '{blockId1}' and RegionIndex '{regionIndex1}'";
            Assert.Contains(expectedLog1, _outputMock.Messages);
            var expectedLog2 = $"Removing persistent object at BlockId '{blockId2}' and RegionIndex '{regionIndex2}'";
            Assert.Contains(expectedLog2, _outputMock.Messages);
        }
    }
}
