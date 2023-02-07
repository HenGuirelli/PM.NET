using PM.Configs;
using PM.Core;
using PM.Factories;

namespace PM.Managers
{
    /// <summary>
    /// This class represents the file that contains the internal generation to
    /// pointers (ulong) to anothers complex objects.
    /// 
    /// When the file is created the file structure is |first ulong|second ulong|integrity byte|file created status|
    /// 
    /// file created status:
    ///     Value 1 when the initial structure of the file is completely written.
    /// integrity byte:
    ///     The integrity byte represents which ulong is OK, if 0 the first is OK,
    ///     if 1 the second is OK.
    /// first ulong:
    ///     Pointer.
    /// second ulong:
    ///     Pointer copy.
    ///     
    /// Example:
    ///     18446744073709551615|18446744073709551615|1|1 <- inital file state
    ///     18446744073709551615|18446744073709551615|0|1 <- begin write new pointer, mark integrity byte = 0
    ///     18446744073709551615|18446744073709551614|0|1 <- write next pointer at second ulong
    ///     18446744073709551615|18446744073709551614|1|1 <- set integrity byte = 1
    ///     18446744073709551614|18446744073709551614|1|1 <- copy next pointer at second ulong into first ulong
    /// </summary>
    public class PointersToPersistentObjects
    {
        private static readonly object _lock = new();
        private readonly PmCSharpDefinedTypes _pmCSharpDefinedTypes;
        public const string PmFileName = $"__{nameof(PointersToPersistentObjects)}";

        public const byte FirstPointerAvailable = 0;
        public const byte SecondPointerAvailable = 1;
        public const byte FileCreatedSuccesfully = 1;

        public PointersToPersistentObjects()
        {
            var path = Path.Combine(PmGlobalConfiguration.PmInternalsFolder, PmFileName);
            var pm = PmFactory.CreatePm(new PmMemoryMappedFileConfig(path, (sizeof(ulong) + sizeof(byte)) * 2));
            _pmCSharpDefinedTypes = new PmCSharpDefinedTypes(pm);

            if (!pm.FileExists() || GetFileCreatedSuccesfullyByte() != FileCreatedSuccesfully)
            {
                CreateInitialFileContent();
            }
            else
            {
                var integrityByte = GetIntegrityByte();
                if (integrityByte == FirstPointerAvailable)
                {
                    SetSecondULong(GetFirstULong());
                }
                if (integrityByte == SecondPointerAvailable)
                {
                    SetFirstULong(GetSecondULong());
                }
                SetIntegrityByte(FirstPointerAvailable);
            }
        }

        private void CreateInitialFileContent()
        {
            SetFirstULong(ulong.MaxValue);
            SetSecondULong(ulong.MaxValue);
            SetIntegrityByte(FirstPointerAvailable);
            SetFileCreatedSuccesfullyByte();
        }

        public ulong GetNext()
        {
            lock (_lock)
            {
                var pointer = GetFirstULong();
                var nextPointer = pointer - 1;
                SetIntegrityByte(FirstPointerAvailable);
                SetSecondULong(nextPointer);
                SetFirstULong(nextPointer);
                SetIntegrityByte(SecondPointerAvailable);
                return pointer;
            }
        }

        private void SetFileCreatedSuccesfullyByte()
        {
            _pmCSharpDefinedTypes.WriteByte(FileCreatedSuccesfully, (sizeof(ulong) * 2) + sizeof(byte));
        }

        private byte GetFileCreatedSuccesfullyByte()
        {
            return _pmCSharpDefinedTypes.ReadByte((sizeof(ulong) * 2) + sizeof(byte));
        }

        private void SetIntegrityByte(byte value)
        {
            _pmCSharpDefinedTypes.WriteByte(value, offset: sizeof(ulong) * 2);
        }

        private byte GetIntegrityByte()
        {
            return _pmCSharpDefinedTypes.ReadByte(offset: sizeof(ulong) * 2);
        }

        private void SetSecondULong(ulong value)
        {
            _pmCSharpDefinedTypes.WriteULong(value, offset: sizeof(ulong));
        }

        private ulong GetSecondULong()
        {
            return _pmCSharpDefinedTypes.ReadULong(offset: sizeof(ulong));
        }

        private void SetFirstULong(ulong value)
        {
            _pmCSharpDefinedTypes.WriteULong(value);
        }

        private ulong GetFirstULong()
        {
            return _pmCSharpDefinedTypes.ReadULong();
        }
    }
}
