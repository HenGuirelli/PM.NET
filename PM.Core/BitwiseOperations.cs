using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.Core
{
    public class BitwiseOperations
    {
        public static bool VerifyBit(byte[] bitmap, int index)
        {
            int byteIndex = index / 8;
            int bitOffset = index % 8;

            byte mask = (byte)(1 << bitOffset);

            return (bitmap[byteIndex] & mask) != 0;
        }

        public static bool IsPowerOfTwo(int number)
        {
            return number > 0 && (number & (number - 1)) == 0;
        }
    }
}
