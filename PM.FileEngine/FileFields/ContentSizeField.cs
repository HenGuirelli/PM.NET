namespace PM.FileEngine.FileFields
{
    public class ContentSizeField : UInt32Filed
    {
        public ContentSizeField(int offset)
        {
            Offset = offset;
        }
    }
}
