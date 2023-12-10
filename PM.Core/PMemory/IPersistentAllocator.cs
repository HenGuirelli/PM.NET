namespace PM.Core.PMemory
{
    public interface IPersistentAllocator
    {
        nint Alloc(long size);
        void Free(nint pointer);
    }
}
