namespace PM.Core
{
    public class PmULongArray : PmPrimitiveArray<ulong>
    {
        public PmULongArray(IPm pm, int length)
            : base(pm, length)
        {
        }

        protected override void InternalSet(int index, ulong value)
        {
            _cSharpDefinedPm.WriteULong(value, sizeof(ulong) * index);
        }

        protected override ulong InternalGet(int index)
        {
            return _cSharpDefinedPm.ReadULong(sizeof(ulong) * index);
        }
    }
}
