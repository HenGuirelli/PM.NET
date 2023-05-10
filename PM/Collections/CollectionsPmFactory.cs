using PM.Configs;
using PM.Core;
using PM.Factories;
using PM.Managers;

namespace PM.Collections
{
    internal class CollectionsPmFactory
    {
        readonly static PointersToPersistentObjects _pointsToPersistentObjects = new();

        internal static PmPrimitiveArray<ulong> CreateULongArray(
            string filename,
            int length)
        {
            var pm = FileHandlerManager.CreateHandler(filename, length * sizeof(ulong));
            return PmPrimitiveArray.CreateNewArray<ulong>(pm.FileBasedStream);
        }
    }
}
