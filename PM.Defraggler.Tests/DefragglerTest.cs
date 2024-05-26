using PM.AutomaticManager.Configs;
using PM.Tests.Common;
using Xunit.Abstractions;

namespace PM.Defraggler.Tests
{
    public class ClassWithReferences
    {
        public virtual ClassWithReferences Reference1 { get; set; }
        public virtual ClassWithReferences Reference2 { get; set; }
    }


    public class DefragglerTest : UnitTest
    {
        private readonly ITestOutputHelper _output;

        public DefragglerTest(ITestOutputHelper output)
            : base(output)
        {
            _output = output;
        }

        [Fact]
        public void Test1()
        {
            PmGlobalConfiguration.PmTarget = Core.PmTargets.TraditionalMemoryMappedFile;
            PmGlobalConfiguration.PersistentGCEnable = false;

            var factory = new AutomaticManager.PersistentFactory();
            var proxyObj = factory.CreateRootObject<ClassWithReferences>(nameof(Test1));

            // Create objects
            ClassWithReferences obj = proxyObj;
            for (var i = 0; i < 10; i++)
            {
                obj.Reference1 = new ClassWithReferences();
                obj.Reference2 = new ClassWithReferences();
                obj = obj.Reference1;
            }

            // RemoveBlockLayout as bunch of objects
            proxyObj.Reference1 = null!;


            var defraggler = new Defraggler(factory.Allocator.PersistentMemory, factory.Allocator.TransactionFile.PmCSharpDefinedTypes);
            defraggler.Defrag();
        }
    }
}