namespace PM.Common
{
    public class BitwiseOperations
    {
        public static bool VerifyBit(ulong bitmap, int index)
        {
            int bitOffset = 7 - (index % 8);

            byte mask = (byte)(1 << bitOffset);

            return (bitmap & mask) != 0;
        }

        public static bool IsPowerOfTwo(int number)
        {
            return number > 0 && (number & (number - 1)) == 0;
        }
        public static bool IsPowerOfTwo(uint number)
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
