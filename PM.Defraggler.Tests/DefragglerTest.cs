using FileFormatExplain;
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
        public void OnDefrag_ShouldRemoveObjectsAndMarkAsFreeIfPossible()
        {
            File.Delete(PmGlobalConfiguration.PmMemoryFilePath);

            PmGlobalConfiguration.PmTarget = Core.PmTargets.TraditionalMemoryMappedFile;
            PmGlobalConfiguration.PersistentGCEnable = false;

            // Uses this file. It was manually changed so that block 1 asserts all regions as used
            var dumpFile1 = @"DumpFiles/PM.NET.FileMemory_1.pm";
            // PMemoryDecoder.DecodeHex(File.ReadAllBytes(@"DumpFiles/PM.NET.FileMemory_1.pm"), dump: false)
            File.WriteAllBytes(PmGlobalConfiguration.PmMemoryFilePath, File.ReadAllBytes(dumpFile1));
            var factory = new AutomaticManager.PersistentFactory();

            var defraggler = new Defraggler(factory.Allocator.PersistentMemory, factory.Allocator.TransactionFile.PmCSharpDefinedTypes);
            defraggler.Defrag();

            var decoded = PMemoryDecoder.DecodeHex(factory.Allocator.ReadOriginalFile(), dump: false);
            _output.WriteLine(decoded);
        }
    }
}