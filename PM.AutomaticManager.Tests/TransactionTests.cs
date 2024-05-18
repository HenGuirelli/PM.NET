using PM.AutomaticManager.Configs;
using PM.AutomaticManager.Tansactions;
using PM.AutomaticManager.Tests.TestObjects;
using PM.Tests.Common;
using PM.Transactions;
using Xunit.Abstractions;

namespace PM.AutomaticManager.Tests
{
    public class TransactionTests : UnitTest
    {
        private readonly ITestOutputHelper _output;

        public TransactionTests(ITestOutputHelper output)
            : base(output)
        {
            _output = output;
        }

        [Fact]
        public void OnTransaction_ShouldDoSimpleOperations()
        {
            PmGlobalConfiguration.PmTarget = Core.PmTargets.TraditionalMemoryMappedFile;

            var factory = new PersistentFactory();
            var proxyObj = factory.CreateRootObject<PocoClass>(nameof(OnTransaction_ShouldDoSimpleOperations));

            proxyObj.Transaction(factory.PMemoryManager, () =>
            {
                proxyObj.IntVal1 = 1;
                proxyObj.IntVal2 = 2;

                proxyObj.IntVal2 += proxyObj.IntVal1;

                Assert.Equal(3, proxyObj.IntVal2);
            });

            Assert.Equal(1, proxyObj.IntVal1);
            Assert.Equal(3, proxyObj.IntVal2);
        }
    }
}
