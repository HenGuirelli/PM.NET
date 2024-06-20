namespace PM.FileEngine.FileFields
{
    public class StartBlockOffsetField : UInt32Filed
    {
        public StartBlockOffsetField(int offset)
        {
            Offset = offset;
        }
    }
}
