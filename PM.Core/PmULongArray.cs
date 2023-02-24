namespace PM.Core
{
    public class PmULongArray : PmPrimitiveArray<ulong>
    {
        public PmULongArray(IPm pm, int length)
            : base(pm, length)
        {
            int count = 0;
            var fileSize = pm.FileSize();
            if (pm.FileExists())
            {
                try
                {
                    while ((sizeof(ulong) * count) < fileSize && InternalGet(count) != 0)
                    {
                        count++;
                    }
                }
                catch (Exception ex)
                {

                }
                if (count > length)
                    Length = count;
            }
            else
            {
                Length = length;
            }
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
