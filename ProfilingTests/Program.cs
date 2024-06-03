
using PM.AutomaticManager;
using PM.AutomaticManager.Configs;
using ProfilingTests;

const string ValueToWrite = "TextValue";

PmGlobalConfiguration.PmTarget = PM.Core.PmTargets.TraditionalMemoryMappedFile;
PmGlobalConfiguration.PmInternalsFolder = "./ProflingTests";

var _persistentFactorySSD = new PersistentFactory();
var _proxy = _persistentFactorySSD.CreateRootObject<RootObject>("RootObj");


while (true)
{
    _proxy.StringVal = ValueToWrite;
}
