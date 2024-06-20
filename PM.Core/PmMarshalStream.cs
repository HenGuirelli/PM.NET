using PM.Common;
using System.Runtime.InteropServices;

namespace PM.Core
{
    /// <summary>
    /// This class have an exclusive write method that 
    /// use traditional Marhsal abstraction to write
    /// on memory mapped file. 
    /// 
    /// This class don't use any function from PMDK lib.
    /// 
    /// Make sure call Drain() method to assert the persistency.
    /// </summary>
    public class PmMarshalStream : PmBasedStream
    {
        public PmMarshalStream(string path, long length)
             : base(path, length)
        {
        }

        protected override void InternalWrite(
            nint destination,
            byte[] buffer,
            int offset,
            int count)
        {
            Marshal.Copy(buffer, offset, destination, count);
        }
    }
}
