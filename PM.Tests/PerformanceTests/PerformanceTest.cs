using PM.Configs;
using PM.Transactions.Tests;
using System;
using System.IO;
using Xunit;

namespace PM.Tests.PerformanceTests
{
    [Collection("PM.UnitTests")]
    public class PerformanceTest
    {
        private readonly static Random _random = new();

        [Fact]
        public void OnWriteReadOperation_ShouldRunWithoutThorwException()
        {
            checked
            {
                var seconds = 10;
                IPersistentFactory factory = new PersistentFactory();
                var obj = factory.CreateRootObject<DomainObject>(nameof(OnWriteReadOperation_ShouldRunWithoutThorwException));
                //var obj = new DomainObject();
                var endTime = DateTime.Now.AddSeconds(seconds);
                ulong countWrite = 0;
                ulong countRead = 0;
                bool counterOptimization = false;
                while (DateTime.Now < endTime)
                {
                    obj.PropULong += (ulong)_random.Next(100);
                    countWrite++;
                }
                if (counterOptimization)
                {
                    Console.WriteLine(obj.PropULong);
                }

                endTime = DateTime.Now.AddSeconds(seconds);
                ulong result = 0;
                while (DateTime.Now < endTime)
                {
                    result = obj.PropULong; 
                    countRead++;
                }
                if (result == 1)
                {
                    Console.WriteLine(obj.PropULong);
                }
                File.WriteAllText(
                    //@"D:\temp\metrica mestrado\writeread_op_pm_only_proxy.log",
                    @"D:\temp\metrica mestrado\writeread_op_traditionalobj.log",
                    //@"D:\temp\metrica mestrado\writeread_op_pm_memorymappedfile.log",
                    $"total={countWrite}|wpsec={(countWrite / (ulong)seconds)}|rpsec={(countRead / (ulong)seconds)}");

            }
           
        }
    }
}
