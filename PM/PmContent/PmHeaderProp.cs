namespace PM.PmContent
{
    public class PmHeaderProp
    {
        // Start with 8 because de size header and hash.
        private static int _offset = 8;

        public int ID { get; }
        public int Offset { get; }

        public PmHeaderProp(int id)
        {
            ID = id;
            _offset += 4;
            Offset = _offset;
        }
    }
}
