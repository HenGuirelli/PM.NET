namespace PM.Core
{
    /// <summary>
    /// This class write on PM memory mapped file using
    /// the PMDK function 'PmemMemcpyNoDrain'.
    /// 
    /// Make sure call Drain() method to assert the persistency.
    /// </summary>
    public class PmMemCopyStream : PmBasedStream
    {
        public PmMemCopyStream(string path, long length)
             : base(path, length)
        {
        }

        protected override void InternalWrite(
            nint destination,
            byte[] buffer,
            int offset,
            int count)
        {
            LibpmemNativeMethods.PmemMemcpyNoDrain(destination, buffer, (ulong)buffer.Length);
        }
    } 
}
