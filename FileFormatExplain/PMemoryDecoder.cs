using PM.Common;
using System.Text;

namespace FileFormatExplain
{
    public class PMemoryDecoder
    {
        public static string DecodeHex(byte[] buffer, bool dump = true, bool ignoreFreeRegions = true)
        {
            var stringBuilder = new StringBuilder();
            if (dump)
                stringBuilder.AppendLine(ByteArrayToHexStringConverter.ByteArrayToString(buffer));

            stringBuilder.AppendLine($"(0x{0.ToString("x8")}) CommitByte={buffer[0].ToString("X2")}");
            stringBuilder.AppendLine($"(0x{1.ToString("x8")}) StartBlocksOffset={BitConverter.ToUInt32(buffer, 1).ToString("X8")}");
            stringBuilder.AppendLine($"(0x{5.ToString("x8")}) Version={BitConverter.ToUInt32(buffer, 5).ToString("X8")}");
            var offsetTotal = 9;
            var count = 0;
            while (true)
            {
                stringBuilder.AppendLine($"========Block {count}========");
                var regionsQuantity = buffer[offsetTotal];
                stringBuilder.AppendLine($"(0x{offsetTotal.ToString("x8")}) RegionsQuantity={regionsQuantity.ToString("X2")}");
                offsetTotal += 1;

                var regionsSize = BitConverter.ToUInt32(buffer, offsetTotal);
                stringBuilder.AppendLine($"(0x{offsetTotal.ToString("x8")}) RegionsSize={regionsSize.ToString("X8")}");
                offsetTotal += 4;

                var freeBlocks = BitConverter.ToUInt64(buffer, offsetTotal);
                stringBuilder.AppendLine($"(0x{offsetTotal.ToString("x8")}) FreeBlocks={freeBlocks.ToString("X16")}");
                offsetTotal += 8;

                var nextBlockAddress = BitConverter.ToUInt32(buffer, offsetTotal);
                stringBuilder.AppendLine($"(0x{offsetTotal.ToString("x8")}) NextBlockAddress={nextBlockAddress.ToString("X8")}");
                offsetTotal += 4;


                for (var regionIndex = 0; regionIndex < regionsQuantity; regionIndex++)
                {
                    if (ignoreFreeRegions && !BitwiseOperations.IsBitOn(freeBlocks, regionIndex))
                        continue;

                    var regionContent = new byte[regionsSize];
                    Array.Copy(buffer, offsetTotal, regionContent, 0, regionsSize);
                    stringBuilder.AppendLine($"(0x{offsetTotal.ToString("x8")}) Region {regionIndex}: " + ByteArrayToHexStringConverter.ByteArrayToString(regionContent));
                    offsetTotal += (int)regionsSize;
                }

                if (nextBlockAddress == 0) break;
                count++;
                offsetTotal = (int)nextBlockAddress;
            }



            return stringBuilder.ToString();
        }

        public static int ReadString(byte[] buffer, int initialOffset, out string result)
        {
            var bytes = new List<byte>();
            while (true)
            {
                var @byte = buffer[initialOffset++];

                if (@byte == 0) break;

                bytes.Add(@byte);
            }

            result = Encoding.UTF8.GetString(bytes.ToArray());
            return initialOffset;
        }
    }
}
