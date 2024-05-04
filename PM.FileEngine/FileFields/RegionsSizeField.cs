using PM.Common;

namespace PM.FileEngine.FileFields
{
    public class RegionsSizeField : UInt32Filed
    {
        public override uint Value
        {
            get => base.Value;
            internal set
            {
                if (!BitwiseOperations.IsPowerOfTwo(value)) throw new ArgumentException($"RegionSize must be power of two");
                base.Value = value;
            }
        }

        public RegionsSizeField(int offset)
        {
            Offset = offset;
        }
    }
}
