namespace PM.Core.PMemory
{
    public interface IPersistentAllocator
    {
        void Alloc(long size);
        void Free(IntPtr pointer);
    }
}
