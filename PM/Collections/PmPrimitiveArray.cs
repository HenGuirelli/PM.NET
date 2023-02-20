using PM.Core;
using PM.Factories;

namespace PM.Collections
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

        protected PmPrimitiveArray(string filepath, int capacity)
        {
            Length = capacity;
            var pm = PmFactory.CreatePm(new PmMemoryMappedFileConfig(filepath, sizeof(ulong) * capacity));
            _cSharpDefinedPm = new PmCSharpDefinedTypes(pm);
        }

        private void Set(int index, T value)
        {
            if (index > Length) throw new IndexOutOfRangeException();
            InternalSet(index, value);
        }

        private T Get(int index)
        {
            if (index > Length) throw new IndexOutOfRangeException();
            return InternalGet(index);
        }

        public virtual void Clear()
        {
            Length = 0;
        }

        protected abstract void InternalSet(int index, T value);
        protected abstract T InternalGet(int index);
    }

    public abstract class PmPrimitiveArray
    {
        public static PmPrimitiveArray<T> CreateNewArray<T>(string filepath, int length)
            where T : struct
        {
            var type = typeof(T);
            CheckType(type);

            if (type == typeof(ulong))
            {
                var obj = new PmULongArray(filepath, length);
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
