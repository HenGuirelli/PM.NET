using Xunit;
using PM.Configs;
using System.Threading.Tasks;
using PM.Tests.Common;
using Bogus;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Collections.Concurrent;
using PM.Core;

namespace PM.Transactions.Tests
{
    public class TransactionExtensionsTests : UnitTest
    {
        [Fact]
        public void OnRunTransaction_ShouldCommitValues()
        {
            IPersistentFactory factory = new PersistentFactory();
            var obj = factory.CreateRootObject<DomainObject>(CreateFilePath(nameof(OnRunTransaction_ShouldCommitValues)));

            obj.Transaction(() =>
            {
                obj.PropBool = true;
                obj.PropByte = byte.MinValue;
                obj.PropChar = char.MinValue;
                obj.PropDecimal = decimal.MinValue;
                obj.PropDouble = double.MinValue;
                obj.PropFloat = float.MinValue;
                obj.PropInt = int.MinValue;
                obj.PropLong = long.MinValue;
                obj.PropSByte = sbyte.MinValue;
                obj.PropShort = short.MinValue;
                obj.PropUInt = uint.MinValue;
                obj.PropULong = ulong.MinValue;
                obj.PropUShort = ushort.MinValue;
                obj.PropStr = "Hello Transaction!";
            });

            Assert.True(obj.PropBool);
            Assert.Equal(byte.MinValue, obj.PropByte);
            Assert.Equal(char.MinValue, obj.PropChar);
            Assert.Equal(decimal.MinValue, obj.PropDecimal);
            Assert.Equal(double.MinValue, obj.PropDouble);
            Assert.Equal(float.MinValue, obj.PropFloat);
            Assert.Equal(int.MinValue, obj.PropInt);
            Assert.Equal(sbyte.MinValue, obj.PropSByte);
            Assert.Equal(short.MinValue, obj.PropShort);
            Assert.Equal(uint.MinValue, obj.PropUInt);
            Assert.Equal(ulong.MinValue, obj.PropULong);
            Assert.Equal(ushort.MinValue, obj.PropUShort);
            Assert.Equal("Hello Transaction!", obj.PropStr);
        }

        [Fact]
        public void OnRunTransaction_ShouldValuesInnerTransactionOnlyVisibleInsideTransction()
        {
            IPersistentFactory factory = new PersistentFactory();
            var obj = factory.CreateRootObject<DomainObject>(CreateFilePath(nameof(OnRunTransaction_ShouldValuesInnerTransactionOnlyVisibleInsideTransction)));

            var t1 = Task.Run(() =>
            {
                obj.Transaction(() =>
                {
                    obj.PropInt = int.MinValue;
                    Assert.Equal(int.MinValue, obj.PropInt);
                    Task.Delay(500).Wait();
                });
            });

            var t2 = Task.Run(async () =>
            {
                await Task.Delay(250);
                Assert.Equal(0, obj.PropInt);
            });

            Task.WhenAll(t1, t2).Wait();

            Assert.True(t1.IsCompletedSuccessfully);
            Assert.True(t2.IsCompletedSuccessfully);
        }

        [Fact]
        public void OnApplyPendingTransactions_WhenTransactionCrashCopingToOriginalFile_ShouldApplyTransaction()
        {
            var oldPmTarget = PmGlobalConfiguration.PmTarget;
            var oldPmInternalsFolderConfig = PmGlobalConfiguration.PmInternalsFolder;
            PmGlobalConfiguration.PmInternalsFolder = "./unittests/internals";

            PmGlobalConfiguration.PmTarget = PmTargets.TraditionalMemoryMappedFile;

            IPersistentFactory factory = new PersistentFactory();
            var obj = factory.CreateRootObject<DomainObject>(CreateFilePath(nameof(OnApplyPendingTransactions_WhenTransactionCrashCopingToOriginalFile_ShouldApplyTransaction)));

            var transactionManager = new TransactionManager<DomainObject>(obj);
            transactionManager.Begin();
            obj.PropInt = int.MaxValue;
            // Commit only the log file, not the whole transaction.
            // This is will write the commit byte in log file, but values
            // will not be copied to original file.
            transactionManager.LogFile.Commit();

            TransactionManager<DomainObject>.ApplyPendingTransactions();

            var obj2 = factory.CreateRootObject<DomainObject>(CreateFilePath(nameof(OnApplyPendingTransactions_WhenTransactionCrashCopingToOriginalFile_ShouldApplyTransaction)));
            Assert.Equal(int.MaxValue, obj2.PropInt);

            PmGlobalConfiguration.PmTarget = oldPmTarget;
            PmGlobalConfiguration.PmInternalsFolder = oldPmInternalsFolderConfig;
        }


        [Fact]
        public void OnMultipleTransactionRunnings_ShouldNotThrowEception()
        {
            //var timeRunning = TimeSpan.FromSeconds(3);
            //var initDatetime = DateTime.Now;
            int parallelDegree = 5000;

            #region create faker
            var objectGeneratorWithRandomData =
                new Faker<DomainObject>()
                .RuleFor(u => u.PropStr, f => f.Random.Guid().ToString())
                .RuleFor(u => u.PropByte, f => f.Random.Byte())
                .RuleFor(u => u.PropSByte, f => f.Random.SByte())
                .RuleFor(u => u.PropShort, f => f.Random.Short())
                .RuleFor(u => u.PropUShort, f => f.Random.UShort())
                .RuleFor(u => u.PropUInt, f => f.Random.UInt())
                .RuleFor(u => u.PropInt, f => f.Random.Int())
                .RuleFor(u => u.PropLong, f => f.Random.Long())
                .RuleFor(u => u.PropULong, f => f.Random.ULong())
                .RuleFor(u => u.PropFloat, f => f.Random.Float())
                .RuleFor(u => u.PropDouble, f => f.Random.Double())
                .RuleFor(u => u.PropDecimal, f => f.Random.Decimal())
                .RuleFor(u => u.PropChar, f => f.Random.Char())
                .RuleFor(u => u.PropBool, f => f.Random.Bool());
            #endregion

            var queue = new ConcurrentQueue<DomainObject>();
            var queueobj = new ConcurrentQueue<DomainObject>();

            IPersistentFactory factory = new PersistentFactory();
            for (int i = 0; i < parallelDegree; i++)
            {
                queue.Enqueue(objectGeneratorWithRandomData.Generate());
                queueobj.Enqueue(factory.CreateRootObject<DomainObject>(CreateFilePath(nameof(OnMultipleTransactionRunnings_ShouldNotThrowEception))));
            }

            var tasks = new List<Task>();
            bool hasError = false;
            var stopWatch = Stopwatch.StartNew();
            int ignored = 0;
            while (queueobj.TryDequeue(out var obj))
            {
                var t = Task.Run(() =>
                {
                    obj.Transaction(() =>
                    {
                        if (queue.TryDequeue(out var randomDataObj))
                        {
                            obj.PropStr = randomDataObj.PropStr;
                            obj.PropByte = randomDataObj.PropByte;
                            obj.PropSByte = randomDataObj.PropSByte;
                            obj.PropShort = randomDataObj.PropShort;
                            obj.PropUShort = randomDataObj.PropUShort;
                            obj.PropUInt = randomDataObj.PropUInt;
                            obj.PropInt = randomDataObj.PropInt;
                            obj.PropLong = randomDataObj.PropLong;
                            obj.PropULong = randomDataObj.PropULong;
                            obj.PropFloat = randomDataObj.PropFloat;
                            obj.PropDouble = randomDataObj.PropDouble;
                            obj.PropDecimal = randomDataObj.PropDecimal;
                            obj.PropChar = randomDataObj.PropChar;
                            obj.PropBool = randomDataObj.PropBool;
                        }
                        else
                        {
                            ignored++;
                        }
                    });
                });
                t.ContinueWith(t => hasError = true, TaskContinuationOptions.OnlyOnFaulted);
                tasks.Add(t);
            }
            Task.WhenAll(tasks).Wait();
            stopWatch.Stop();
            File.AppendAllText(@"D:\temp\metrica mestrado\transactions.txt", $"{parallelDegree};{stopWatch.ElapsedMilliseconds}\n");

            Assert.False(hasError);
            Assert.Equal(0, ignored);
        }
    }
}