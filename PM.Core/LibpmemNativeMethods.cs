using System.Runtime.InteropServices;

namespace PM.Core
{
    internal static class LibpmemNativeMethods
    {
        [DllImport("libpmem", EntryPoint = "pmem_memset", SetLastError = true)]
        public static extern int PmemMemset(IntPtr dest, int c, UIntPtr len, int flags);

        [DllImport("libpmem", EntryPoint = "pmem_memcpy_persist", SetLastError = true)]
        public static extern void PmemMemcpyPersist(IntPtr dest, IntPtr src, UIntPtr len);

        [DllImport("libpmem", EntryPoint = "pmem_flush", SetLastError = true)]
        public static extern int PmemFlush(IntPtr addr, UIntPtr len);

        [DllImport("libpmem", EntryPoint = "pmem_drain", SetLastError = true)]
        public static extern int PmemDrain();

        [DllImport("libpmem", EntryPoint = "pmem_map", SetLastError = true)]
        public static extern IntPtr PmemMap(IntPtr addr, UIntPtr len, int prot, int flags, int fd, ulong offset);

        [DllImport("libpmem", EntryPoint = "pmem_unmap", SetLastError = true)]
        public static extern int PmemUnmap(IntPtr addr, UIntPtr len);
    }
}
