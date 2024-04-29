using System.Text;

namespace FileFormatExplain
{
    public class TransactionFileDecoder
    {
        public static string Decode_HexResponse(byte[] buffer)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(ByteArrayToHexStringConverter.ByteArrayToString(buffer));
            stringBuilder.AppendLine($"CommitByte={buffer[0].ToString("X2")}");
            stringBuilder.AppendLine($"Version={BitConverter.ToUInt16(buffer, 1).ToString("X4")}");
            stringBuilder.AppendLine($"=================AddBlockLayout=================");
            stringBuilder.AppendLine($"CommitByte={buffer[3].ToString("X2")}");
            stringBuilder.AppendLine($"Order={BitConverter.ToUInt16(buffer, 4).ToString("X4")}");
            stringBuilder.AppendLine($"StartBlockOffset={BitConverter.ToUInt32(buffer, 6).ToString("X8")}");
            stringBuilder.AppendLine($"RegionsQtty={buffer[10].ToString("X2")}");
            stringBuilder.AppendLine($"RegionSize={BitConverter.ToUInt32(buffer, 11).ToString("X8")}");
            stringBuilder.AppendLine($"=================RemoveBlockLayout=================");
            stringBuilder.AppendLine($"CommitByte={buffer[15].ToString("X2")}");
            stringBuilder.AppendLine($"Order={BitConverter.ToUInt16(buffer, 16).ToString("X4")}");
            stringBuilder.AppendLine($"StartBlockOffset={BitConverter.ToUInt32(buffer, 18).ToString("X8")}");
            stringBuilder.AppendLine($"=================UpdateContentBlockLayout=================");
            stringBuilder.AppendLine($"CommitByte={buffer[22].ToString("X2")}");
            stringBuilder.AppendLine($"Order={BitConverter.ToUInt16(buffer, 23).ToString("X4")}");
            stringBuilder.AppendLine($"StartBlockOffset={BitConverter.ToUInt32(buffer, 25).ToString("X8")}");
            var contentSize = BitConverter.ToUInt32(buffer, 29);
            stringBuilder.AppendLine($"ContentSize={contentSize.ToString("X8")}");

            var content = new byte[contentSize];
            Array.Copy(buffer, 33, content, 0, contentSize);
            stringBuilder.AppendLine($"Content={ByteArrayToHexStringConverter.ByteArrayToString(content)}");
            return stringBuilder.ToString();
        }
    }
}
