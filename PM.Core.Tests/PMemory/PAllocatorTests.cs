using PM.Core.PMemory;
using PM.Tests.Common;
using System.IO;
using Xunit;

namespace PM.Core.Tests.PMemory
{
    public class PAllocatorTests : UnitTest
    {
        [Fact]
        public void OnCtor_ShouldCreatePMemoryLayout()
        {
            DeleteFile(nameof(OnCtor_ShouldCreatePMemoryLayout));

            var pmStream = CreatePmStream(nameof(OnCtor_ShouldCreatePMemoryLayout), 4096);

            var persistentAllocatorHeader = new PersistentAllocatorHeader();

            persistentAllocatorHeader.AddBlock(new PersistentBlockLayout(regionSize: 8, regionQuantity: 2));
            persistentAllocatorHeader.AddBlock(new PersistentBlockLayout(regionSize: 16, regionQuantity: 2));
            persistentAllocatorHeader.AddBlock(new PersistentBlockLayout(regionSize: 32, regionQuantity: 2));

            var pAllocator = new PAllocator(persistentAllocatorHeader, new PmCSharpDefinedTypes(pmStream));
            var filepath = pAllocator.FilePath;
            pAllocator.Dispose();

            string content = File.ReadAllText(filepath);
        }
    }
}
