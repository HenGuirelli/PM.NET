namespace PM.FileEngine.FileFields
{
    public class NewFreeBlocksField : UInt64Filed
    {
        public NewFreeBlocksField(int offset)
        {
            Offset = offset;
        }
    }
}
