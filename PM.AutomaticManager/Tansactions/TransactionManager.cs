using PM.AutomaticManager.MetaDatas;
using PM.AutomaticManager.Proxies;
using PM.FileEngine;

namespace PM.AutomaticManager.Tansactions
{
    internal class TransactionManager
    {
        private readonly object _obj;
        private readonly PMemoryManager _pMemoryManager;
        private readonly ObjectPropertiesInfoMapper _objectMapper;
        private TransactionState _state = TransactionState.NotStarted;
        private TransactonRegionReturn? _transactionReturn;

        public TransactionState State
        {
            get => _state;
            set => ChangeState(value);
        }

        public TransactionManager(PMemoryManager pMemoryManager, object obj)
        {
            _obj = obj;
            _pMemoryManager = pMemoryManager;
            if (!CastleManager.TryGetCastleProxyInterceptor(_obj, out var pmInterceptor))
            {
                throw new ApplicationException("Object must be a persistent object to running transactions");
            }

            _objectMapper = new ObjectPropertiesInfoMapper(pmInterceptor!.TargetType);
        }

        private void ChangeState(TransactionState value)
        {
            _transactionReturn?.TransactionMetaDataStructure?.ChangeState(value);
            _state = value;
        }

        internal void Begin()
        {
            _transactionReturn = _pMemoryManager.CreateNewTransactionRegion(_obj, _objectMapper.GetTypeSize());
            State = TransactionState.Running;
            PmInterceptor.TransactionPersistentRegion = _transactionReturn.TransactionRegion;
        }

        internal void Commit()
        {
            if (_transactionReturn is null)
            {
                throw new ApplicationException($"{nameof(_transactionReturn.TransactionRegion)} cannot be null. Call Begin() method before Commit()");
            }

            if (CastleManager.TryGetCastleProxyInterceptor(_obj, out var pmInterceptor))
            {
                var regionContent = _transactionReturn.TransactionRegion.Read(count: _transactionReturn.TransactionRegion.Size, offset: 0);
                pmInterceptor!.PersistentRegion.Write(regionContent, offset: 0);
            }
            PmInterceptor.TransactionPersistentRegion = null;
            _transactionReturn.TransactionMetaDataStructure.Invalidate();
            State = TransactionState.Commited;
        }

        internal void RollBack()
        {
            var block = _pMemoryManager.Allocator.GetBlock(_transactionReturn!.TransactionMetaDataStructure.BlockID);
            block.MarkRegionAsFree(_transactionReturn.TransactionMetaDataStructure.RegionIndex);
            PmInterceptor.TransactionPersistentRegion = null;
            _transactionReturn.TransactionMetaDataStructure.Invalidate();
        }

        internal static void ApplyPendingTransaction(PAllocator allocator, TransactionMetaDataStructure transactionMetaDataStructure)
        {
            var transactionRegion = allocator.GetRegion(transactionMetaDataStructure.TransactionBlockIDTarget, transactionMetaDataStructure.TransactionRegionIndexTarget);
            var originalRegion = allocator.GetRegion(transactionMetaDataStructure.BlockID, transactionMetaDataStructure.RegionIndex);

            var content = transactionRegion.Read(count: transactionRegion.Size, offset: 0);
            originalRegion.Write(content, offset: 0);

            transactionMetaDataStructure.Invalidate();
        }
    }
}