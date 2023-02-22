namespace PM.Core
{
    public abstract class PmPrimitiveArray<T>
        where T : struct
    {
        protected readonly PmCSharpDefinedTypes _cSharpDefinedPm;
        public int Length { get; private set; }

        public T this[int index]
        {
            get => Get(index);
            set => Set(index, value);
        }

        protected PmPrimitiveArray(IPm pm, int length)
        {
            Length = length;
            _cSharpDefinedPm = new PmCSharpDefinedTypes(pm);
        }

        private void Set(int index, T value)
        {
            if (index >= Length || index < 0) throw new IndexOutOfRangeException();
            InternalSet(index, value);
        }

        private T Get(int index)
        {
            if (index >= Length || index < 0) throw new IndexOutOfRangeException();
            return InternalGet(index);
        }

        public virtual void Clear()
        {
            _cSharpDefinedPm.DeleteFile();
            _cSharpDefinedPm.CreateFile();
            Length = 0;
        }

        protected abstract void InternalSet(int index, T value);
        protected abstract T InternalGet(int index);
    }

    public abstract class PmPrimitiveArray
    {
        public static PmPrimitiveArray<T> CreateNewArray<T>(IPm pm, int length)
            where T : struct
        {
            var type = typeof(T);
            CheckType(type);

            if (type == typeof(ulong))
            {
                var obj = new PmULongArray(pm, length);
                return obj as PmPrimitiveArray<T> ?? throw new ArgumentException($"Type {typeof(T)} not recongnized");
            }

            throw new ArgumentException($"Type {typeof(T)} not recongnized");
        }

        private static void CheckType(Type type)
        {
            if (!type.IsPrimitive &&
                type != typeof(decimal))
            {
                throw new ArgumentException($"Type T must be primitive");
            }
        }
    }
}
