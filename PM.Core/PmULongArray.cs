namespace PM.Core
{
    public class PmULongArray : PmPrimitiveArray<ulong>
    {
        public PmULongArray(FileBasedStream pm, int length)
            : base(pm, length)
        {
            //var fileSize = pm.GetFileSize();
            //var oldArrayLength = (int)fileSize / sizeof(ulong);
            //if (oldArrayLength != length)
            //    throw new ArgumentException($"argument {nameof(length)}={length} invalid. Array already defined with length={oldArrayLength}");

            Length = length;
        }

        public override void Resize(int newLength)
        {
            _cSharpDefinedPm.Resize(newLength * sizeof(ulong));
            Length = newLength;
        }

        protected override void InternalSet(int index, ulong value)
        {
            _cSharpDefinedPm.WriteULong(value, (sizeof(ulong) * index));
        }

        protected override ulong InternalGet(int index)
        {
            return _cSharpDefinedPm.ReadULong(sizeof(ulong) * index);
        }
    }
}
