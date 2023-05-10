using PM;
using PM.CastleHelpers;
using PM.Configs;
using PM.GarbageCollectionExample;
using System.Collections.Concurrent;

var filenames = new ConcurrentDictionary<int, string>();
PmGlobalConfiguration.PmInternalsFolder = @"D:\temp\pm_tests";
PmGlobalConfiguration.PmTarget = PM.Core.PmTargets.TraditionalMemoryMappedFile;

//var creationThread = new Thread(() =>
//{
    IPersistentFactory factory = new PersistentFactory();
    var filename = $"file_root";
    var obj = factory.CreateRootObject<TestClass>(filename);
    Console.WriteLine($"Objeto root criado no arquivo {filename}, nunca será coletado");
    var count = 0;
    //while (true)
    //{
        var innerObj = new TestClass();
        obj.MyProperty = innerObj;
        CastleManager.TryGetInterceptor(obj.MyProperty, out var interceptor);
        Console.WriteLine($"Objeto interno criado no arquivo {interceptor!.FilePointer}");
        filenames[count] = interceptor!.FilePointer;
        count++;

        Thread.Sleep(3000);

        obj.MyProperty = null;
        Console.WriteLine("Objeto interno setado para null, esperando GC coletar");
        GC.Collect();
//    }
//});


var gcThread = new Thread(() =>
{
    while (true)
    {
        foreach (var filename in filenames)
        {
            if (!File.Exists(filename.Value))
            {
                Console.WriteLine($"[GC]: Arquivo deletado {filename}");
                filenames.TryRemove(filename.Key, out _);
            }
        }
        
        Thread.Sleep(500);
    }
});

//creationThread.Name = "creationThread";
//creationThread.Start();

gcThread.Name = "gcThread";
gcThread.Start();

Console.ReadLine();