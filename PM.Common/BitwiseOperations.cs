namespace PM.Common
{
    public class BitwiseOperations
    {
        public static bool IsBitOn(ulong bitmap, int index)
        {
            if (index < 0 || index >= sizeof(ulong) * 8)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Índice fora do intervalo válido.");
            }

            ulong mask = (ulong)1 << index;

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

        public static uint RoundUpPowerOfTwo(uint value)
        {
            if (value <= 0)
            {
                throw new ArgumentException($"{nameof(value)} must be greater than zero");
            }

            // number already is power of 2
            if ((value & (value - 1)) == 0) return value;

            // Find the most significant bit and increment
            uint moreSignificantbit = 1;
            while (moreSignificantbit < value)
            {
                moreSignificantbit <<= 1;
            }

            return moreSignificantbit;
        }
    }
}
