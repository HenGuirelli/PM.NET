namespace PM.Core.PMemory.FileFields
{
    public class ContentSizeField : UInt32Filed
    {
        public ContentSizeField(int offset)
        {
            Offset = offset;
        }
    }
}
