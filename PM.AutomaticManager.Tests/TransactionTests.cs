using FileFormatExplain;
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


        [Fact]
        public void OnTransaction_WithMultipleObjects_ShouldDoTransaction()
        {
            PmGlobalConfiguration.PmTarget = Core.PmTargets.TraditionalMemoryMappedFile;

            var factory = new PersistentFactory();
            var proxyObj = factory.CreateRootObject<RootClass>(nameof(OnTransaction_WithMultipleObjects_ShouldDoTransaction));

            proxyObj.Transaction(factory.PMemoryManager, () =>
            {
                proxyObj.InnerObject1 = new InnerClass1 { Val = int.MaxValue };
                proxyObj.InnerObject2 = new InnerClass1 { Val = int.MinValue };
                proxyObj.InnerObject1.InnerObject1 = new InnerClass2 { Val = int.MinValue };
            });

            Assert.Equal(int.MaxValue, proxyObj.InnerObject1.Val);
            Assert.Equal(int.MinValue, proxyObj.InnerObject2.Val);
            Assert.Equal(int.MinValue, proxyObj.InnerObject1.InnerObject1.Val);


            var decoded = PMemoryDecoder.DecodeHex(factory.Allocator.ReadOriginalFile(), dump: false);
            _output.WriteLine(decoded);
        }

        [Fact]
        public void OnTransaction_TrasactionValuesShouldRunOnlyInsideTransaction()
        {
#if DEBUG
            PersistentFactory.Purge();
#endif

            PmGlobalConfiguration.PmTarget = Core.PmTargets.TraditionalMemoryMappedFile;

            var factory = new PersistentFactory();
            var proxyObj = factory.CreateRootObject<RootClass>(nameof(OnTransaction_WithMultipleObjects_ShouldDoTransaction));

            bool error = false;

            var tTransaction = new Thread(() =>
            {
                try
                {
                    proxyObj.Transaction(factory.PMemoryManager, () =>
                    {
                        proxyObj.InnerObject1 = new InnerClass1 { Val = int.MaxValue };
                        proxyObj.InnerObject2 = new InnerClass1 { Val = int.MinValue };
                        proxyObj.InnerObject1.InnerObject1 = new InnerClass2 { Val = int.MinValue };
                        proxyObj.Val = 10000;
                        Thread.Sleep(500);
                    });

                    Assert.Equal(int.MaxValue, proxyObj.InnerObject1.Val);
                    Assert.Equal(int.MinValue, proxyObj.InnerObject2.Val);
                    Assert.Equal(int.MinValue, proxyObj.InnerObject1.InnerObject1.Val);
                    Assert.Equal(10000, proxyObj.Val);
                }
                catch (Exception ex)
                {
                    error = true;
                }
            });

            var tValidation = new Thread(() =>
            {
                try
                {
                    // Wait tTransaction starts
                    Thread.Sleep(100);

                    Assert.Null(proxyObj.InnerObject1);
                    Assert.Null(proxyObj.InnerObject2);
                }
                catch (Exception ex)
                {
                    error = true;
                }
            });

            tTransaction.Start();
            tValidation.Start();

            tTransaction.Join();
            tValidation.Join();

            var decoded = PMemoryDecoder.DecodeHex(factory.Allocator.ReadOriginalFile(), dump: false);
            _output.WriteLine(decoded);

            Assert.False(error);
        }
    }
}
