namespace PM.Core
{
    public class BitwiseOperations
    {
        public static bool VerifyBit(byte[] bitmap, int index)
        {
            int byteIndex = index / 8;
            int bitOffset = 7 - (index % 8);

            byte mask = (byte)(1 << bitOffset);

            return (bitmap[byteIndex] & mask) != 0;
        }

        public static bool IsPowerOfTwo(int number)
        {
            return number > 0 && (number & (number - 1)) == 0;
        }


        public static int RoundUpPowerOfTwo(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentException($"{nameof(value)} must be greater than zero");
            }

            // number already is power of 2
            if ((value & (value - 1)) == 0) return value;

            // Find the most significant bit and increment
            int moreSignificantbit = 1;
            while (moreSignificantbit < value)
            {
                moreSignificantbit <<= 1;
            }

            return moreSignificantbit;
        }
    }
}
