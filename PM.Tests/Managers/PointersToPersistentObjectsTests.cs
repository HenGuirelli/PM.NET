using PM.Configs;
using PM.Managers;
using PM.Tests.Common;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace PM.Tests.Managers
{
    public class PointersToPersistentObjectsTests : UnitTest
    {
        public PointersToPersistentObjectsTests(ITestOutputHelper output)
            : base(output)
        {
        }

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
