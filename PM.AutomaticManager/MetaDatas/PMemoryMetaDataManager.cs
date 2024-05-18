using PM.AutomaticManager.Proxies;
using PM.AutomaticManager.Tansactions;
using PM.Core.PMemory;
using PM.FileEngine;
using System.Text;

namespace PM.AutomaticManager.MetaDatas
{
    internal class PMemoryMetaDataManager
    {
        private readonly PAllocator _allocator;
        private PersistentRegion _metadataRegion;
        private int _nextMetadataStructureInternalOffset;

        // Caches
        readonly Dictionary<string, MetaDataStructure> _metaDataStructureByObjectUserID = new();
        readonly Dictionary<MetadataType, MetaDataStructure> _metaDataStructureByType = new();

        public PMemoryMetaDataManager(PAllocator allocator)
        {
            _allocator = allocator;
            if (!allocator.HasAnyBlocks)
            {
                // Reserve first block for metadata
                // 25000 pointers capacity
                _metadataRegion = allocator.Alloc(100_000);
            }
            else
            {
                _metadataRegion = Read();
            }
        }

        public PersistentRegion Read()
        {
            _metadataRegion = _allocator.FirstPersistentBlockLayout!.Regions[0];
            _nextMetadataStructureInternalOffset = 0;
            while (true)
            {
                var initialMetadatastructureOffset = _nextMetadataStructureInternalOffset;

                var metadataType = _metadataRegion.Read(count: 1, offset: _nextMetadataStructureInternalOffset)[0]; // Read First metadataStructure
                _nextMetadataStructureInternalOffset += sizeof(byte);
                var isMetadataValid = Convert.ToBoolean(_metadataRegion.Read(count: 1, offset: _nextMetadataStructureInternalOffset)[0]);
                _nextMetadataStructureInternalOffset += sizeof(byte);
                if (metadataType != 0 && isMetadataValid) // Have value!!
                {
                    if (metadataType == (byte)MetadataType.Object)
                    {
                        ReadObjectMetadata();
                    }

                    if (metadataType == (byte)MetadataType.Transaction)
                    {
                        ReadTransactionMetadata(initialMetadatastructureOffset);
                    }
                }
                else { break; }
            }

            return _metadataRegion;
        }

        private void ReadTransactionMetadata(int initialMetadatastructureOffset)
        {
            var blockId = BitConverter.ToUInt32(_metadataRegion.Read(count: 4, offset: _nextMetadataStructureInternalOffset));
            _nextMetadataStructureInternalOffset += sizeof(uint);
            var regionIndex = _metadataRegion.Read(count: 1, offset: _nextMetadataStructureInternalOffset)[0];
            _nextMetadataStructureInternalOffset += sizeof(byte);
            var offsetInnerRegion = BitConverter.ToUInt16(_metadataRegion.Read(count: 2, offset: _nextMetadataStructureInternalOffset));
            _nextMetadataStructureInternalOffset += sizeof(ushort);
            var objectSize = BitConverter.ToUInt32(_metadataRegion.Read(count: 4, offset: _nextMetadataStructureInternalOffset));
            _nextMetadataStructureInternalOffset += sizeof(uint);
            var transactionState = (TransactionState)_metadataRegion.Read(count: 1, offset: _nextMetadataStructureInternalOffset)[0];
            _nextMetadataStructureInternalOffset += sizeof(byte);
            var transactionBlockIDTarget = BitConverter.ToUInt32(_metadataRegion.Read(count: 4, offset: _nextMetadataStructureInternalOffset));
            _nextMetadataStructureInternalOffset += sizeof(uint);
            var transactionRegionIndexTarget = _metadataRegion.Read(count: 1, offset: _nextMetadataStructureInternalOffset)[0];
            _nextMetadataStructureInternalOffset += sizeof(byte);

            var transactionMetaDataStructure = new TransactionMetaDataStructure(_metadataRegion, (uint)initialMetadatastructureOffset)
            {
                BlockID = blockId,
                RegionIndex = regionIndex,
                OffsetInnerRegion = offsetInnerRegion,
                ObjectSize = objectSize,
                TransactionState = transactionState,
                TransactionblockIDTarget = transactionBlockIDTarget,
                TransactionRegionIndexTarget = transactionRegionIndexTarget
            };
            TransactionManager.ApplyPendingTransaction(_allocator, transactionMetaDataStructure);
            var metadataStructure = new MetaDataStructure
            {
                MetadataType = MetadataType.Transaction,
                TransactionMetaDataStructure = transactionMetaDataStructure
            };
            _metaDataStructureByType.Add(metadataStructure.MetadataType, metadataStructure);
        }

        private void ReadObjectMetadata()
        {
            var blockId = BitConverter.ToUInt32(_metadataRegion.Read(count: 4, offset: _nextMetadataStructureInternalOffset));
            _nextMetadataStructureInternalOffset += sizeof(uint);
            var regionIndex = _metadataRegion.Read(count: 1, offset: _nextMetadataStructureInternalOffset)[0];
            _nextMetadataStructureInternalOffset += sizeof(byte);
            var offsetInnerRegion = BitConverter.ToUInt16(_metadataRegion.Read(count: 2, offset: _nextMetadataStructureInternalOffset));
            _nextMetadataStructureInternalOffset += sizeof(ushort);
            var objectSize = BitConverter.ToUInt32(_metadataRegion.Read(count: 4, offset: _nextMetadataStructureInternalOffset));
            _nextMetadataStructureInternalOffset += sizeof(uint);
            var stringBytes = new List<byte>();
            while (true)
            {
                var @byte = _metadataRegion.Read(count: 1, offset: _nextMetadataStructureInternalOffset)[0];

                if (@byte == 0) break;

                stringBytes.Add(@byte);
                _nextMetadataStructureInternalOffset += 1;
            }

            var objectUserID = Encoding.UTF8.GetString(stringBytes.ToArray());
            var metadataStructure = new MetaDataStructure
            {
                MetadataType = MetadataType.Object,
                ObjectMetaDataStructure = new ObjectMetaDataStructure
                {
                    BlockID = blockId,
                    RegionIndex = regionIndex,
                    OffsetInnerRegion = offsetInnerRegion,
                    ObjectSize = objectSize,
                    ObjectUserID = objectUserID
                }
            };

            _metaDataStructureByObjectUserID.Add(objectUserID, metadataStructure);
            _metaDataStructureByType.Add(metadataStructure.MetadataType, metadataStructure);
        }

        internal TransactonRegionReturn CreateNewTransactionRegion(object obj, uint objectSize)
        {
            var buffer = new byte[19];
            var bufferOffset = 0;
            // MetadataType
            buffer[bufferOffset] = (byte)MetadataType.Transaction;
            bufferOffset += sizeof(byte);
            // Valid
            buffer[bufferOffset] = BitConverter.GetBytes(true)[0];
            bufferOffset += sizeof(byte);
            // BlockId
            if (!CastleManager.TryGetCastleProxyInterceptor(obj, out var pmInterceptor))
            {
                throw new ApplicationException("Transaction need occur in persistent object");
            }
            var originalBlockId = BitConverter.GetBytes(pmInterceptor!.PersistentRegion.BlockID);
            Array.Copy(sourceArray: originalBlockId, sourceIndex: 0, destinationArray: buffer, destinationIndex: bufferOffset, length: originalBlockId.Length);
            bufferOffset += sizeof(uint);
            // region index
            var originalRegionIndex = pmInterceptor.PersistentRegion.RegionIndex;
            buffer[bufferOffset] = originalRegionIndex;
            bufferOffset += sizeof(byte);
            // offsetInnerRegion
            var offsetInnerRegionBytes = BitConverter.GetBytes((ushort)0); // Always zero
            Array.Copy(sourceArray: offsetInnerRegionBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: bufferOffset, length: offsetInnerRegionBytes.Length);
            bufferOffset += sizeof(ushort);
            // objectSize
            var objectSizeBytes = BitConverter.GetBytes(objectSize);
            Array.Copy(sourceArray: objectSizeBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: bufferOffset, length: objectSizeBytes.Length);
            bufferOffset += sizeof(uint);
            // TansactionState
            byte transactionStateBytes = 0; // Init always 0
            buffer[bufferOffset] = transactionStateBytes;
            bufferOffset += sizeof(byte);
            var transactionRegion = _allocator.Alloc(objectSize);
            // TransactionBlockIDTarget
            var transactionBlockIDTarget = BitConverter.GetBytes(transactionRegion.BlockID);
            Array.Copy(sourceArray: transactionBlockIDTarget, sourceIndex: 0, destinationArray: buffer, destinationIndex: bufferOffset, length: transactionBlockIDTarget.Length);
            bufferOffset += sizeof(uint);
            // TransactionRegionIndexTarget
            buffer[bufferOffset] = transactionRegion.RegionIndex;
            bufferOffset += sizeof(byte);


            _metadataRegion.Write(buffer, _nextMetadataStructureInternalOffset);


            // Add to cache
            var transactionMetaDataStructure = new TransactionMetaDataStructure(_metadataRegion, (uint)_nextMetadataStructureInternalOffset)
            {
                BlockID = transactionRegion.BlockID,
                RegionIndex = transactionRegion.RegionIndex,
                OffsetInnerRegion = 0,
                ObjectSize = objectSize,
                TransactionState = TransactionState.NotStarted,
                TransactionblockIDTarget = pmInterceptor!.PersistentRegion.BlockID,
                TransactionRegionIndexTarget = originalRegionIndex
            };
            _nextMetadataStructureInternalOffset += buffer.Length;
            var metadataStructure = new MetaDataStructure
            {
                MetadataType = MetadataType.Transaction,
                TransactionMetaDataStructure = transactionMetaDataStructure
            };
            _metaDataStructureByType.Add(metadataStructure.MetadataType, metadataStructure);

            return new TransactonRegionReturn(transactionMetaDataStructure, transactionRegion);
        }

        internal PersistentRegion AllocRootObjectByType(Type type, string objectUserID, ObjectPropertiesInfoMapper objectPropertiesInfoMapper)
        {
            var objectLength = objectPropertiesInfoMapper.GetTypeSize();
            PersistentRegion objectRegion = _allocator.Alloc(objectLength);
            var blockId = BitConverter.GetBytes(objectRegion.BlockID);
            var regionIndex = objectRegion.RegionIndex;
            ushort offsetInnerRegion = 0;
            var offsetInnerRegionBytes = BitConverter.GetBytes(offsetInnerRegion); // Always zero
            var objectSizeBytes = BitConverter.GetBytes(objectLength);

            // 12 = metadata size + object user id length + \0 string byte
            var buffer = new byte[13 + objectUserID.Length + 1];
            var bufferOffset = 0;
            buffer[bufferOffset] = (byte)MetadataType.Object;
            bufferOffset += sizeof(byte);
            buffer[bufferOffset] = BitConverter.GetBytes(true)[0];
            bufferOffset += sizeof(byte);
            Array.Copy(sourceArray: blockId, sourceIndex: 0, destinationArray: buffer, destinationIndex: bufferOffset, length: blockId.Length);
            bufferOffset += sizeof(uint);
            buffer[bufferOffset] = regionIndex;
            bufferOffset += sizeof(byte);
            Array.Copy(sourceArray: offsetInnerRegionBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: bufferOffset, length: offsetInnerRegionBytes.Length);
            bufferOffset += sizeof(ushort);
            Array.Copy(sourceArray: objectSizeBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: bufferOffset, length: objectSizeBytes.Length);
            bufferOffset += sizeof(uint);
            var idBytes = Encoding.UTF8.GetBytes(objectUserID);
            Array.Copy(sourceArray: idBytes, sourceIndex: 0, destinationArray: buffer, destinationIndex: bufferOffset, length: idBytes.Length);
            // Put \0 character in end of string
            Array.Copy(sourceArray: new byte[] { (byte)'\0' }, sourceIndex: 0, destinationArray: buffer, destinationIndex: bufferOffset + idBytes.Length, length: 1);

            _metadataRegion.Write(buffer, _nextMetadataStructureInternalOffset);
            _nextMetadataStructureInternalOffset += buffer.Length;

            // Add to cache
            _metaDataStructureByObjectUserID.Add(objectUserID, new MetaDataStructure
            {
                MetadataType = MetadataType.Object,
                ObjectMetaDataStructure = new ObjectMetaDataStructure
                {
                    BlockID = objectRegion.BlockID,
                    RegionIndex = regionIndex,
                    OffsetInnerRegion = offsetInnerRegion,
                    ObjectSize = objectLength,
                    ObjectUserID = objectUserID
                }
            });

            return objectRegion;
        }

        internal bool ObjectExists(string objectUserID)
        {
            return _metaDataStructureByObjectUserID.ContainsKey(objectUserID);
        }

        internal MetaDataStructure GetByObjectUserID(string objectUserID)
        {
            return _metaDataStructureByObjectUserID[objectUserID];
        }
    }
}
