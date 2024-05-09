using PM.AutomaticManager.Configs;
using PM.AutomaticManager.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM.AutomaticManager
{
    public interface IPersistentFactory
    {
        T CreateRootObject<T>(string pmFilename);
    }

    public class PersistentFactory : IPersistentFactory
    {
        static PMemoryManager _pManager = new PMemoryManager(
            new FileEngine.PAllocator(
                PmFactory.CreatePmCSharpDefinedTypes(PmGlobalConfiguration.PmMemoryFilePath),
                PmFactory.CreatePmCSharpDefinedTypes(PmGlobalConfiguration.PmMemoryFileTransactionPath)
            ));

        public T CreateRootObject<T>(string objectID)
        {
            if (_pManager.ObjectExists(objectID))
            {
                // Load object, proxy.. etc
            }
            else
            {
                // Create proxy etc etc..
                var obj = CreateObject(_pManager);
                _pManager.AddNewObject(obj);
            }
            throw new NotImplementedException();
        }
    }
}
