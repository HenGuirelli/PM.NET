namespace PM.Core
{
    public class PmULongArray : PmPrimitiveArray<ulong>
    {
        public PmULongArray(IPm pm, int length)
            : base(pm, length)
        {
            if (pm.FileExists())
            {
                var fileSize = pm.FileSize();
                var oldArrayLength = (int)fileSize / sizeof(ulong);
                if (oldArrayLength != length)
                    throw new ArgumentException($"argument {nameof(length)}={length} invalid. Array alread defined with length={oldArrayLength}");
            }
            Length = length;
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
