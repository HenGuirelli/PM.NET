using System.Runtime.InteropServices;

namespace PM.Core
{
    public class Flags
    {
        public const int PMEM_FILE_CREATE   = 1 << 0;
        public const int PMEM_FILE_EXCL     = 1 << 1;
        public const int PMEM_FILE_SPARSE   = 1 << 2;
        public const int PMEM_FILE_TMPFILE	= 1 << 3;
    }
    public static class LibpmemNativeMethods
    {
        [DllImport("libpmem.so", EntryPoint = "pmem_map_file")]
        public static extern IntPtr MapFile(string path, long length, int flags, int mode, out long mappedLength, out int isPersistent);

        [DllImport("libpmem.so", EntryPoint = "pmem_unmap")]
        public static extern int Unmap(IntPtr addr, long length);

        [DllImport("libpmem.so", EntryPoint = "pmem_memcpy_persist")]
        public static extern int MemcpyPersist(IntPtr dest, IntPtr src, ulong count);

        [DllImport("libpmem.so", EntryPoint = "pmem_persist")]
        public static extern void Persist(IntPtr addr, ulong len);

        [DllImport("libpmem.so", EntryPoint = "pmem_flush")]
        public static extern void Flush(IntPtr addr, ulong len);

        [DllImport("libpmem.so", EntryPoint = "pmem_drain")]
        public static extern void PmemDrain();

        [DllImport("libpmem.so", EntryPoint = "pmem_memcpy_nodrain")]
        public static extern void PmemMemcpyNoDrain(IntPtr dest, byte[] src, ulong len);

        [DllImport("libpmem.so", EntryPoint = "pmem_memset_nodrain")]
        public static extern void PmemMemsetNoDrain(IntPtr dest, byte c, ulong len);
    }
}
