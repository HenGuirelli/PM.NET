using PM.Core.PMemory.FileFields;
using System;
using System.Text;

namespace FileFormatExplain
{
    public interface IDecoder
    {
        string ExplainDec(byte[] buffer);
        string ExplainHex(byte[] buffer);
    }

    public class ByteArrayToHexStringConverter
    {
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }

    public class MemoryLayoutTransactionDecoder : IDecoder
    {
        public string ExplainHex(byte[] buffer)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(ByteArrayToHexStringConverter.ByteArrayToString(buffer));
            stringBuilder.AppendLine($"CommitByte={buffer[0].ToString("X2")}");
            stringBuilder.AppendLine($"AddBlockOffset={BitConverter.ToUInt16(buffer, 1).ToString("X4")}");
            stringBuilder.AppendLine($"AddBlocksQtty={BitConverter.ToUInt16(buffer, 3).ToString("X4")}");
            stringBuilder.AppendLine($"RemoveBlockOffset={BitConverter.ToUInt16(buffer, 5).ToString("X4")}");
            stringBuilder.AppendLine($"RemoveBlocksQtty={BitConverter.ToUInt16(buffer, 7).ToString("X4")}");
            stringBuilder.AppendLine($"UpdateContentOffset={BitConverter.ToUInt16(buffer, 9).ToString("X4")}");
            stringBuilder.AppendLine($"UpdateContentQtty={BitConverter.ToUInt16(buffer, 11).ToString("X4")}");
            stringBuilder.Append(DecodeAddBlocksHex(buffer));
            return stringBuilder.ToString();
        }

        internal static string DecodeAddBlocksHex(byte[] buffer)
        {
            var stringBuilder = new StringBuilder();
            var addBlockOffset = BitConverter.ToUInt16(buffer, 1);
            var addBlockQtty = BitConverter.ToUInt16(buffer, 3);
            for (int i = 0; i < addBlockQtty; i++)
            {
                stringBuilder.AppendLine($"===========Addblock===========");
                stringBuilder.AppendLine($"CommitByte={buffer[addBlockOffset].ToString("X2")}");
                addBlockOffset += CommitByteField.Size;
                stringBuilder.AppendLine($"Order={BitConverter.ToUInt16(buffer, addBlockOffset).ToString("X4")}");
                addBlockOffset += 2;
                stringBuilder.AppendLine($"StartBlockOffset={BitConverter.ToUInt32(buffer, addBlockOffset).ToString("X8")}");
                addBlockOffset += 4;
                stringBuilder.AppendLine($"RegionsQtty={buffer[addBlockOffset].ToString("X2")}");
                addBlockOffset += 1;
                stringBuilder.AppendLine($"RegionsSize={BitConverter.ToUInt16(buffer, addBlockOffset).ToString("X4")}");
                addBlockOffset += 4;
            }
            return stringBuilder.ToString();
        }

        public string ExplainDec(byte[] buffer)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(ByteArrayToHexStringConverter.ByteArrayToString(buffer));
            stringBuilder.AppendLine($"CommitByte={buffer[0]}");
            stringBuilder.AppendLine($"AddBlockOffset={BitConverter.ToUInt16(buffer, 1)}");
            stringBuilder.AppendLine($"AddBlocksQtty={BitConverter.ToUInt16(buffer, 1)}");
            stringBuilder.AppendLine($"RemoveBlockOffset={BitConverter.ToUInt16(buffer, 3)}");
            stringBuilder.AppendLine($"RemoveBlocksQtty={BitConverter.ToUInt16(buffer, 5)}");
            stringBuilder.AppendLine($"UpdateContentOffset={BitConverter.ToUInt16(buffer, 7)}");
            stringBuilder.AppendLine($"UpdateContentQtty={BitConverter.ToUInt16(buffer, 9)}");
            return stringBuilder.ToString();
        }
    }
}
