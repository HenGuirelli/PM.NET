
using PM.AutomaticManager;
using PM.AutomaticManager.Configs;
using ProfilingTests;
using Serilog;

Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .CreateLogger();

PmGlobalConfiguration.PmTarget = PM.Core.PmTargets.PM;
PmGlobalConfiguration.PmInternalsFolder = "/mnt/nvram1/henguirelli/benchmarks/";

var _persistentFactorySSD = new PersistentFactory();
int OperationCount = 20000;
for (int i = 0; i < OperationCount; i++)
{
    var _proxy = _persistentFactorySSD.CreateRootObject<RootObject>(Guid.NewGuid().ToString());
    GC.KeepAlive(_proxy);
}
