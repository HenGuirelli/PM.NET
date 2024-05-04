using PM.FileEngine.Transactions;
using System.Text;

namespace FileFormatExplain
{
    public class TransactionFileDecoder
    {
        public static string Decode_HexResponse(byte[] buffer)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(ByteArrayToHexStringConverter.ByteArrayToString(buffer));
            stringBuilder.AppendLine($"HeaderCommitByte={buffer[TransactionFileOffset.HeaderCommitByte].ToString("X2")}");
            stringBuilder.AppendLine($"Version={BitConverter.ToUInt16(buffer, TransactionFileOffset.HeaderVersion).ToString("X4")}");
            stringBuilder.AppendLine($"BlockType={buffer[TransactionFileOffset.HeaderBlockType].ToString("X2")}");
            if (buffer[3] == (byte)BlockLayoutType.AddBlock)
            {
                stringBuilder.AppendLine($"=================AddBlockLayout=================");
                stringBuilder.AppendLine($"CommitByte={buffer[TransactionFileOffset.AddBlockCommitByte].ToString("X2")}");
                stringBuilder.AppendLine($"StartBlockOffset={BitConverter.ToUInt32(buffer, TransactionFileOffset.AddBlockStartBlockOffset).ToString("X8")}");
                stringBuilder.AppendLine($"RegionsQtty={buffer[TransactionFileOffset.AddBlockRegionsQtty].ToString("X2")}");
                stringBuilder.AppendLine($"RegionSize={BitConverter.ToUInt32(buffer, TransactionFileOffset.AddBlockRegionSize).ToString("X8")}");
            }
            if (buffer[3] == (byte)BlockLayoutType.RemoveBlock)
            {
                stringBuilder.AppendLine($"=================RemoveBlockLayout=================");
                stringBuilder.AppendLine($"CommitByte={buffer[TransactionFileOffset.RemoveBlockCommitByte].ToString("X2")}");
                stringBuilder.AppendLine($"BeforeBlockOffset={BitConverter.ToUInt32(buffer, TransactionFileOffset.RemoveBlockBeforeBlockOffset).ToString("X8")}");
                stringBuilder.AppendLine($"RemovedBlockOffset={BitConverter.ToUInt32(buffer, TransactionFileOffset.RemoveBlockRemovedBlockOffset).ToString("X8")}");
                stringBuilder.AppendLine($"AfterBlockOffset={BitConverter.ToUInt32(buffer, TransactionFileOffset.RemoveBlockAfterBlockOffset).ToString("X8")}");
            }
            if (buffer[3] == (byte)BlockLayoutType.UpdateContentBlock)
            {
                stringBuilder.AppendLine($"=================UpdateContentBlockLayout=================");
                stringBuilder.AppendLine($"CommitByte={buffer[TransactionFileOffset.UpdateContentBlockCommitByte].ToString("X2")}");
                stringBuilder.AppendLine($"StartBlockOffset={BitConverter.ToUInt32(buffer, TransactionFileOffset.UpdateContentBlockStartBlockOffset).ToString("X8")}");
                var contentSize = BitConverter.ToUInt32(buffer, TransactionFileOffset.UpdateContentBlockContentSize);
                stringBuilder.AppendLine($"ContentSize={contentSize.ToString("X8")}");
                var content = new byte[contentSize];
                Array.Copy(buffer, TransactionFileOffset.UpdateContentBlockContent, content, 0, contentSize);
                stringBuilder.AppendLine($"Content={ByteArrayToHexStringConverter.ByteArrayToString(content)}");
            }

            return stringBuilder.ToString();
        }
    }
}
