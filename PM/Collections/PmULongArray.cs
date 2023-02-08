namespace PM.Collections
{
    internal class PmULongArray : PmPrimitiveArray<ulong>
    {
        public PmULongArray(string filepath, int capacity)
            : base(filepath, capacity)
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
