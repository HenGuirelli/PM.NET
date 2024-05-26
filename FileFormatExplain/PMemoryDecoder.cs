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

            stringBuilder.AppendLine($"CommitByte={buffer[0].ToString("X2")}");
            stringBuilder.AppendLine($"StartBlocksOffset={BitConverter.ToUInt32(buffer, 1).ToString("X8")}");
            stringBuilder.AppendLine($"Version={BitConverter.ToUInt32(buffer, 5).ToString("X8")}");
            var offsetTotal = 9;
            var count = 0;
            while (true)
            {
                stringBuilder.AppendLine($"========Block {count} (0x{offsetTotal.ToString("x8")})========");
                var regionsQuantity = buffer[offsetTotal];
                offsetTotal += 1;
                var regionsSize = BitConverter.ToUInt32(buffer, offsetTotal);
                offsetTotal += 4;
                var freeBlocks = BitConverter.ToUInt64(buffer, offsetTotal);
                offsetTotal += 8;
                var nextBlockAddress = BitConverter.ToUInt32(buffer, offsetTotal);
                offsetTotal += 4;

                stringBuilder.AppendLine($"RegionsQuantity={regionsQuantity.ToString("X2")}");
                stringBuilder.AppendLine($"RegionsSize={regionsSize.ToString("X8")}");
                stringBuilder.AppendLine($"FreeBlocks={freeBlocks.ToString("X16")}");
                stringBuilder.AppendLine($"NextBlockAddress={nextBlockAddress.ToString("X8")}");

                for (var regionIndex = 0; regionIndex < regionsQuantity; regionIndex++)
                {
                    if (ignoreFreeRegions && !BitwiseOperations.IsBitOn(freeBlocks, regionIndex))
                        continue;

                    var regionContent = new byte[regionsSize];
                    Array.Copy(buffer, offsetTotal, regionContent, 0, regionsSize);
                    stringBuilder.AppendLine($"Region {regionIndex}: " + ByteArrayToHexStringConverter.ByteArrayToString(regionContent));
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
