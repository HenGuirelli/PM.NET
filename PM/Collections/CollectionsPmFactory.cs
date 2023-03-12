using PM.Configs;
using PM.Core;
using PM.Factories;
using PM.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PM.Collections
{
    internal class CollectionsPmFactory
    {
        readonly static PointersToPersistentObjects _pointsToPersistentObjects = new();

        public static IPm CreateNewPm(string filepath, int length)
        {
            string pmfilename = filepath;
            
            if (PmTargets.FileBasedTarget.HasFlag(PmGlobalConfiguration.PmTarget))
            {
                var pointer = _pointsToPersistentObjects.GetNext().ToString();
                FileBasedStorage.CreateSymbolicLink(filepath, pointer);
                pmfilename = pointer;
            }

            return PmFactory.CreatePm(
                        new PmMemoryMappedFileConfig(
                            pmfilename,
                            sizeof(ulong) * (length + 1)));
        }
    }
}
