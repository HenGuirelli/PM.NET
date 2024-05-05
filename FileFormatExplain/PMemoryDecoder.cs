using System.Text;

namespace FileFormatExplain
{
    public class PMemoryDecoder
    {
        public static string DecodeHex(byte[] buffer)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(ByteArrayToHexStringConverter.ByteArrayToString(buffer));

            stringBuilder.AppendLine($"CommitByte={buffer[0].ToString("X2")}");
            stringBuilder.AppendLine($"StartBlocksOffset={BitConverter.ToUInt32(buffer, 1).ToString("X8")}");
            var count = 0;
            var offsetTotal = 5;
            while (true)
            {
                stringBuilder.AppendLine($"========Block {count}========");
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
                    var regionContent = new byte[regionsSize];
                    Array.Copy(buffer, offsetTotal, regionContent, 0, regionsSize);
                    stringBuilder.AppendLine($"Region {regionIndex}: " + ByteArrayToHexStringConverter.ByteArrayToString(regionContent));
                    offsetTotal += (int)regionsSize;
                }

                if (nextBlockAddress == 0) break;
                count++;
            }



            return stringBuilder.ToString();
        }
    }
}
