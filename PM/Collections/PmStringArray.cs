using PM.Configs;
using PM.Core;
using PM.Factories;
using PM.Managers;
using System.Reflection;

namespace PM.Collections
{
    public class PmStringArray
    {
        private readonly PmULongArray _pmULongArray;
        private static readonly PointersToPersistentObjects _pointersToPersistentObjects = new();

        public int Length => _pmULongArray.Length;

        public string this[int index]
        {
            get => Get(index);
            set => Set(index, value);
        }

        public PmStringArray(string filepath, int length)
        {
            var pm = PmFactory.CreatePm(
                new PmMemoryMappedFileConfig(
                    filepath,
                    sizeof(char) * (length + 1)));

            _pmULongArray = new PmULongArray(pm, length);
        }

        private void Set(int index, string value)
        {
            if (index > Length) throw new IndexOutOfRangeException();
            if (value is null)
            {
                _pmULongArray[index] = 0;
                return;
            }

            var pointer = _pointersToPersistentObjects.GetNext();
            var pm = PmFactory.CreatePm(
                new PmMemoryMappedFileConfig(
                    Path.Combine(PmGlobalConfiguration.PmInternalsFolder, pointer.ToString()),
                    sizeof(char) * (value.Length + 1)));

            var cSharpDefinedPm = new PmCSharpDefinedTypes(pm);
            cSharpDefinedPm.WriteString(value);

            _pmULongArray[index] = pointer;
        }

        private string Get(int index)
        {
            if (index > Length) throw new IndexOutOfRangeException();

            var pointer = _pmULongArray[index];
            var pmFile = Path.Combine(PmGlobalConfiguration.PmInternalsFolder, pointer.ToString());
            var pm = PmFactory.CreatePm(
                new PmMemoryMappedFileConfig(
                    pmFile));

            var cSharpDefinedPm = new PmCSharpDefinedTypes(pm);

            return cSharpDefinedPm.ReadString();
        }

        public void Clear()
        {
            _pmULongArray.Clear();
        }
    }
}
