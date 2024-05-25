using PM.AutomaticManager.Proxies;
using PM.AutomaticManager.Tansactions;
using PM.Core.PMemory;
using PM.FileEngine;
using System.Diagnostics.CodeAnalysis;

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
        readonly Dictionary<uint, Dictionary<byte, ObjectMetaDataStructure>> _rootObjectmetaDataStructureByBlockIdAndRegionID = new();

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
                        AddRootObjectmetaDataStructureByBlockIdAndRegionIDCache(objectMetaDataStructure);
                    }

                    _nextMetadataStructureInternalOffset += metadataStructure.Size;
                }
            }
        }

        private void AddRootObjectmetaDataStructureByBlockIdAndRegionIDCache(ObjectMetaDataStructure objectMetaDataStructure)
        {
            if (!_rootObjectmetaDataStructureByBlockIdAndRegionID.ContainsKey(objectMetaDataStructure.BlockID))
            {
                _rootObjectmetaDataStructureByBlockIdAndRegionID[objectMetaDataStructure.BlockID] = new();
            }

            Dictionary<byte, ObjectMetaDataStructure> blockIdDict = _rootObjectmetaDataStructureByBlockIdAndRegionID[objectMetaDataStructure.BlockID];

            blockIdDict[objectMetaDataStructure.RegionIndex] = objectMetaDataStructure;
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
                ObjectUserID = objectUserID,
                ClassTypeName = objectPropertiesInfoMapper.ObjectType.FullName ?? throw new ApplicationException($"Type {objectPropertiesInfoMapper.ObjectType} need a FullName")
            };

            objectStructure.WriteTo(_metadataRegion, _nextMetadataStructureInternalOffset);
            _nextMetadataStructureInternalOffset += objectStructure.Size;

            // Add to cache
            AddRootObjectmetaDataStructureByBlockIdAndRegionIDCache(objectStructure);
            _metaDataStructureByObjectUserID.Add(objectUserID, objectStructure);

            return objectRegion;
        }

        // You can only access root objects by metadata
        internal bool TryGetRootObjectByBlockIdAndRegionIndex(uint blockId, byte regionIndex, [NotNullWhen(true)] out ObjectMetaDataStructure? result)
        {
            if (_rootObjectmetaDataStructureByBlockIdAndRegionID.TryGetValue(blockId, out var objectByRegionIndex) &&
                objectByRegionIndex.TryGetValue(regionIndex, out var @object))
            {
                result = @object;
                return true;
            }
            result = null;
            return false;
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
