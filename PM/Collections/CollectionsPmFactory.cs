using PM.Configs;
using PM.Core;
using PM.Factories;
using PM.Managers;

namespace PM.Collections
{
    internal class CollectionsPmFactory
    {
        readonly static PointersToPersistentObjects _pointsToPersistentObjects = new();

        public static Stream CreateNewPm(string filepath, int length)
        {
            string pmfilename = filepath;
            
            if (PmTargets.FileBasedTarget.HasFlag(PmGlobalConfiguration.PmTarget))
            {
                var pointer = _pointsToPersistentObjects.GetNext().ToString();
                PmFileSystem.CreateSymbolicLink(filepath, pointer);
                pmfilename = pointer;
            }

            return PmFactory.CreatePm(
                            pmfilename,
                            sizeof(ulong) * (length + 1));
        }

        internal static PmPrimitiveArray<ulong> CreateULongArray(
            string filename,
            int length)
        {
            var pm = PmFactory.CreatePm(filename, length * sizeof(ulong));
            return PmPrimitiveArray.CreateNewArray<ulong>(pm);
        }
    }
}
