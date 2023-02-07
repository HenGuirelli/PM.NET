using PM;
using PM.Configs;
using PM.NET.ConsoleApp;

PmGlobalConfiguration.PmTarget = PmTargets.TraditionalMemoryMappedFile;
Random _random = new Random();
IPersistentFactory factory = new PersistentFactory();
var obj = factory.CreateRootObject<DomainObject>("OnWriteReadOperation_ShouldRunWithoutThorwException");
//var obj = new DomainObject();
ulong countWrite = 0;
ulong countRead = 0;

var t1 = new Thread(() =>
{
    while (true)
    {
        obj.PropULong += (ulong)_random.Next(100);
    }
});

t1.Name = "thread 1 escrita e leitura";
t1.Priority = ThreadPriority.AboveNormal;
t1.Start();

Thread.Sleep(Timeout.Infinite);