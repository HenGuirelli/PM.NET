using BenchmarkDotNet.Attributes;
using PM.AutomaticManager;
using PM.AutomaticManager.Configs;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Benchmarks
{
    public class SSDPersistentObjectsBenchmark : PersistentObjectsBenchmark
    {
        private PersistentFactory? _persistentFactorySSD;
        private RootObject _proxySSD;

        protected override void SetupPmDotnet(ConfigFile configFile)
        {
            PmGlobalConfiguration.PmTarget = PM.Core.PmTargets.TraditionalMemoryMappedFile;
            if (configFile.PersistentObjectsFilePathSSD != null)
            {
                PmGlobalConfiguration.PmInternalsFolder = configFile.PersistentObjectsFilePathSSD!;
            }
            if (configFile.PersistentObjectsFilePathPm != null)
            {
                PmGlobalConfiguration.PmInternalsFolder = configFile.PersistentObjectsFilePathPm!;
            }

            _persistentFactorySSD = new PersistentFactory();
            _proxySSD = _persistentFactorySSD.CreateRootObject<RootObject>("RootObj");
        }

        #region ProxyObjects SSD
        [Benchmark]
        public void ProxyObjects_Write_SSD()
        {
            _proxySSD.Text = ValueToWrite;
        }

        [Benchmark]
        public void ProxyObjects_Read_SSD()
        {
            var rand = _proxySSD.Text;
            GC.KeepAlive(rand);
        }
        #endregion
    }
}
