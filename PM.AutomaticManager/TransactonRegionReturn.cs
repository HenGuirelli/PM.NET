using PM.AutomaticManager.MetaDatas;
using PM.Core.PMemory;

namespace PM.AutomaticManager
{
    internal class TransactonRegionReturn
    {
        public TransactionMetaDataStructure TransactionMetaDataStructure { get; }
        public PersistentRegion TransactionRegion { get; }

        public TransactonRegionReturn(TransactionMetaDataStructure transactionMetaDataStructure, PersistentRegion transactionRegion)
        {
            TransactionMetaDataStructure = transactionMetaDataStructure;
            TransactionRegion = transactionRegion;
        }
    }
}
