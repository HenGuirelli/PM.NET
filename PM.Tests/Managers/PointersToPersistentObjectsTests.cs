using PM.Configs;
using PM.Managers;
using PM.Tests.Common;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PM.Tests.Managers
{
    [Collection("PM.UnitTests")]
    public class PointersToPersistentObjectsTests
    {
        public PointersToPersistentObjectsTests()
        {
            if (Constraints.UseFakePm)
                PmGlobalConfiguration.PmTarget = PmTargets.InVolatileMemory;
            else
                File.Delete(PmGlobalConfiguration.PmInternalsFolder);
        }

        [Fact]
        public void OnGetNext_ShouldGetNextPointer()
        {
            var pointersToPersistentObjects = new PointersToPersistentObjects();
            
            Assert.Equal(ulong.MaxValue, pointersToPersistentObjects.GetNext());
            Assert.Equal(ulong.MaxValue - 1, pointersToPersistentObjects.GetNext());
            Assert.Equal(ulong.MaxValue - 2, pointersToPersistentObjects.GetNext());
        }
    }
}
