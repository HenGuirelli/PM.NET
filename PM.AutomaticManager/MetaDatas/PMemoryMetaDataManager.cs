using PM.AutomaticManager.Proxies;
using PM.AutomaticManager.Tansactions;
using PM.Core.PMemory;
using PM.FileEngine;

namespace PM.AutomaticManager.MetaDatas
{
    internal class PMemoryMetadataManager
    {
        private readonly PAllocator _allocator;
        private readonly PersistentRegion _metadataRegion;
        private int _nextMetadataStructureInternalOffset;
        private readonly MetadataReader _metadataReader;

        // Caches
        readonly Dictionary<string, MetadataStructure> _metaDataStructureByObjectUserID = new();
        readonly Dictionary<MetadataType, MetadataStructure> _metaDataStructureByType = new();

        public PMemoryMetadataManager(PAllocator allocator)
        {
            _allocator = allocator;
            if (!allocator.HasAnyBlocks)
            {
                // Reserve first block for metadata
                // 25000 pointers capacity
                _metadataRegion = allocator.Alloc(100_000);
                _metadataReader = new MetadataReader(_metadataRegion);
            }
            else
            {
                _metadataRegion = _allocator.FirstPersistentBlockLayout!.Regions[0];
                _metadataReader = new MetadataReader(_metadataRegion);

                while (_metadataReader.TryGetNext(out var metadataStructure))
                {
                    if (metadataStructure is TransactionMetaDataStructure transactionMetaDataStructure &&
                        transactionMetaDataStructure.TransactionState == TransactionState.Commited)
                    {
                        TransactionManager.ApplyPendingTransaction(_allocator, transactionMetaDataStructure);
                    }

                    if (metadataStructure is ObjectMetaDataStructure objectMetaDataStructure)
                    {
                        _metaDataStructureByObjectUserID.Add(objectMetaDataStructure.ObjectUserID, metadataStructure);
                    }

                    _metaDataStructureByType.Add(metadataStructure.Type, metadataStructure);
                    _nextMetadataStructureInternalOffset += metadataStructure.Size;
                }
            }
        }

        internal TransactonRegionReturn CreateNewTransactionRegion(object obj, uint objectSize)
        {
            if (!CastleManager.TryGetCastleProxyInterceptor(obj, out var pmInterceptor))
            {
                throw new ApplicationException("Transaction need occur in persistent object");
            }

            var transactionRegion = _allocator.Alloc(objectSize);
            var trasactionStructure = new TransactionMetaDataStructure
            {
                IsValid = true,
                BlockID = transactionRegion.BlockID,
                RegionIndex = transactionRegion.RegionIndex,
                OffsetInnerRegion = 0,
                ObjectSize = objectSize,
                TransactionBlockIDTarget = pmInterceptor.PersistentRegion.BlockID,
                TransactionRegionIndexTarget = pmInterceptor.PersistentRegion.RegionIndex
            };


            // Write on regions and add to caches
            trasactionStructure.WriteTo(_metadataRegion, _nextMetadataStructureInternalOffset);
            _nextMetadataStructureInternalOffset += trasactionStructure.Size;

            _metaDataStructureByType.Add(trasactionStructure.Type, trasactionStructure);

            return new TransactonRegionReturn(trasactionStructure, transactionRegion);
        }

        internal PersistentRegion AllocRootObjectByType(string objectUserID, ObjectPropertiesInfoMapper objectPropertiesInfoMapper)
        {
            var objectLength = objectPropertiesInfoMapper.GetTypeSize();
            PersistentRegion objectRegion = _allocator.Alloc(objectLength);
            var regionIndex = objectRegion.RegionIndex;

            var objectStructure = new ObjectMetaDataStructure
            {
                IsValid = true,
                BlockID = objectRegion.BlockID,
                RegionIndex = regionIndex,
                OffsetInnerRegion = 0,
                ObjectSize = objectLength,
                ObjectUserID = objectUserID
            };

            objectStructure.WriteTo(_metadataRegion, _nextMetadataStructureInternalOffset);
            _nextMetadataStructureInternalOffset += objectStructure.Size;

            // Add to cache
            _metaDataStructureByObjectUserID.Add(objectUserID, objectStructure);

            return objectRegion;
        }

        internal bool ObjectExists(string objectUserID)
        {
            return _metaDataStructureByObjectUserID.ContainsKey(objectUserID);
        }

        internal MetadataStructure? GetByObjectUserID(string objectUserID)
        {
            _metaDataStructureByObjectUserID.TryGetValue(objectUserID, out var metadataStructure);
            return metadataStructure;
        }
    }
}
