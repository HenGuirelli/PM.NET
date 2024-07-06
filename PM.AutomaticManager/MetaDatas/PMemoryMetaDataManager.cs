using PM.AutomaticManager.Proxies;
using PM.AutomaticManager.Tansactions;
using PM.Core.PMemory;
using PM.FileEngine;
using Serilog;
using System.Diagnostics.CodeAnalysis;

namespace PM.AutomaticManager.MetaDatas
{
    internal class MetadataPersistentRegion
    {
        public PersistentRegion PersistentRegion { get; }
        public uint FreeBytesQty { get; private set; }
        public uint UsedBytesQty
        {
            get => _usedBytesQty;
            set
            {
                _usedBytesQty = value;
                FreeBytesQty = _metadataStructureSize - _usedBytesQty;
            }
        }
        private uint _usedBytesQty;
        private readonly uint _metadataStructureSize;

        public uint Offset => UsedBytesQty;

        public MetadataPersistentRegion(PersistentRegion persistentRegion, uint metadataStructureSize, uint usedBytesQty)
        {
            PersistentRegion = persistentRegion;
            _usedBytesQty = usedBytesQty;
            _metadataStructureSize = metadataStructureSize;
            FreeBytesQty = metadataStructureSize - usedBytesQty;
        }
    }

    internal class PMemoryMetadataManager
    {
        private readonly PAllocator _allocator;
        private readonly List<MetadataPersistentRegion> _metadataRegions = new();
        private List<MetadataReader> _metadataReaders = new();

        // Caches
        readonly Dictionary<string, MetadataStructure> _metaDataStructureByObjectUserID = new();
        readonly Dictionary<uint, Dictionary<byte, ObjectMetaDataStructure>> _rootObjectmetaDataStructureByBlockIdAndRegionID = new();
        readonly HashSet<UInt32> _metadataBlockIds = new();

        // 25000 pointers capacity
        public uint MetadataRegionSize { get; set; } = 100_000;

        public PMemoryMetadataManager(PAllocator allocator)
        {
            _allocator = allocator;
            if (!allocator.HasAnyBlocks)
            {
                // Reserve first block for metadata
                var metadataRegion = allocator.Alloc(MetadataRegionSize);
                _metadataRegions.Add(new MetadataPersistentRegion(metadataRegion, MetadataRegionSize, usedBytesQty: 0));
                _metadataBlockIds.Add(metadataRegion.BlockID);
                _metadataReaders.Add(new MetadataReader(metadataRegion));
            }
            else
            {
                var metadataRegion = _allocator.FirstPersistentBlockLayout!.Regions[0];

                var metadataReader = new MetadataReader(metadataRegion);
                _metadataReaders.Add(metadataReader);

                Queue<OtherMetadataRegionPointerStructure> othersMetadataRegions = new();
                do
                {
                    _metadataBlockIds.Add(metadataRegion.BlockID);
                    var metadataPersistentRegion = new MetadataPersistentRegion(metadataRegion, MetadataRegionSize, usedBytesQty: 0);
                    _metadataRegions.Add(metadataPersistentRegion);
                    uint regionUsedSize = 0;
                    while (metadataReader.TryGetNext(out var metadataStructure))
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

                        if (metadataStructure is OtherMetadataRegionPointerStructure otherMetadataRegionPointerStructure)
                        {
                            othersMetadataRegions.Enqueue(otherMetadataRegionPointerStructure);
                        }

                        metadataPersistentRegion.UsedBytesQty += metadataStructure.Size;
                        regionUsedSize += metadataStructure.Size;
                    }

                    if (othersMetadataRegions.TryDequeue(out var otherMetadataRegionPointerStructureDequed))
                    {
                        metadataRegion = _allocator.GetRegion(otherMetadataRegionPointerStructureDequed.BlockID, otherMetadataRegionPointerStructureDequed.RegionIndex);
                        metadataReader = new MetadataReader(metadataRegion);
                        _metadataRegions.Add(metadataPersistentRegion);
                    }
                    else
                    {
                        metadataReader = null;
                    }
                } while (metadataReader != null);
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
            var metadataRegion = GetFreeMetadataPersistentRegion(trasactionStructure.Size);
            trasactionStructure.WriteTo(metadataRegion.PersistentRegion, (int)metadataRegion.Offset);
            metadataRegion.UsedBytesQty += trasactionStructure.Size;

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
                ClassTypeName = objectPropertiesInfoMapper.ObjectType.FullName
                    ?? throw new ApplicationException($"Type {objectPropertiesInfoMapper.ObjectType} need a FullName"),
                AssemblyFullName = objectPropertiesInfoMapper.ObjectType.Assembly.FullName
                    ?? throw new ApplicationException($"Asembly {objectPropertiesInfoMapper.ObjectType.Assembly} need a FullName")
            };

            var metadataRegion = GetFreeMetadataPersistentRegion(objectStructure.Size);
            objectStructure.WriteTo(metadataRegion.PersistentRegion, (int)metadataRegion.Offset);
            metadataRegion.UsedBytesQty += objectStructure.Size;

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

        internal MetadataPersistentRegion GetFreeMetadataPersistentRegion(uint requiredSize)
        {
            foreach (var metadataRegion in _metadataRegions)
            {
                if (metadataRegion.FreeBytesQty >= OtherMetadataRegionPointerStructure.SizeBytes + requiredSize) return metadataRegion;
            }

            // If reaches here, need create one more region of metadatas
            Log.Debug("All metadata regions is full, creating new one");
            var metadataRegionToCreatePointer = GetFreeMetadataRegiontoCreatePointer();
            var newMetadataPointerRegion = _allocator.Alloc(MetadataRegionSize);
            var metadataPointerStructure = new OtherMetadataRegionPointerStructure
            {
                BlockID = newMetadataPointerRegion.BlockID,
                RegionIndex = newMetadataPointerRegion.RegionIndex,
            };
            metadataPointerStructure.WriteTo(metadataRegionToCreatePointer.PersistentRegion, (int)metadataRegionToCreatePointer.Offset);
            metadataRegionToCreatePointer.UsedBytesQty += metadataPointerStructure.Size;

            Log.Debug("New metadata region created. BlockId={blockid}, Region Index={regionIndex}", newMetadataPointerRegion.BlockID, newMetadataPointerRegion.RegionIndex);
            var metadataPersistentRegion = new MetadataPersistentRegion(newMetadataPointerRegion, MetadataRegionSize, 0);
            _metadataRegions.Add(metadataPersistentRegion);
            return metadataPersistentRegion;
        }

        private MetadataPersistentRegion GetFreeMetadataRegiontoCreatePointer()
        {
            foreach (var metadataRegion in _metadataRegions)
            {
                if (metadataRegion.FreeBytesQty >= OtherMetadataRegionPointerStructure.SizeBytes) return metadataRegion;
            }

            throw new ApplicationException($"Not enought space to create a new metadata region");
        }

        internal MetadataStructure? GetByObjectUserID(string objectUserID)
        {
            _metaDataStructureByObjectUserID.TryGetValue(objectUserID, out var metadataStructure);
            return metadataStructure;
        }

        internal bool IsMetadataBlock(PersistentBlockLayout blockLayout)
        {
            return _metadataBlockIds.Contains(blockLayout.BlockOffset);
        }
    }
}
