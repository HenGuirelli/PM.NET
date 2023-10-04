using PM.Core.Factories;
using System.Collections.Concurrent;

namespace PM.Core.PMemory
{
    public class PersistentMemory
    {
        private readonly PmCSharpDefinedTypes _fileBasedStream;
        private readonly ConcurrentDictionary<IntPtr, long> _allocatedMemoryRegion = new();
        private readonly PMemoryAllocatedRegion _pMemoryAllocatedRegion;

        public PersistentMemory(
            PmCSharpDefinedTypes processFileBasedStream,
            string pmLogFileName)
        {
            _fileBasedStream = processFileBasedStream;
            _pMemoryAllocatedRegion = new PMemoryAllocatedRegion(PmFactory.CreatePmCSharpDefinedTypes(pmLogFileName));
        }

        public IntPtr Alloc(long size)
        {

            return default;
        }

        public void Free(IntPtr pointer)
        {

        }
    }
}
