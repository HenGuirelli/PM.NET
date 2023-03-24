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
    public class PointersToPersistentObjectsTests : UnitTest
    {
        [Fact]
        public void OnGetNext_ShouldGetNextPointer()
        {
            File.Delete(Path.Combine(PmGlobalConfiguration.PmInternalsFolder, "__PointersToPersistentObjects.pm"));
            var pointersToPersistentObjects = new PointersToPersistentObjects();
            
            var value1 = pointersToPersistentObjects.GetNext();
            var value2 = pointersToPersistentObjects.GetNext();
            var value3 = pointersToPersistentObjects.GetNext();
            Assert.Equal(value2, value1 - 1);
            Assert.Equal(value3, value2 - 1);
        }
    }
}
