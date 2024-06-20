using PM.AutomaticManager;
using PM.AutomaticManager.MetaDatas;
using PM.Common;
using PM.Core.PMemory;
using PM.FileEngine;
using PM.FileEngine.Transactions;
using System.Reflection;
using System.Text;

namespace PM.Defraggler.Defragglers
{
    internal class Defraggler_V1 : IDefraggler
    {
        private readonly PmCSharpDefinedTypes _originalFile;
        private readonly PAllocator _allocator;
        private readonly TransactionFile _transactionFile;
        private readonly PMemoryMetadataManager _pMemoryMetadataManager;
        private uint _startBlockOffset;
        private readonly HashSet<RegionsInUse> _allUsedRegions = new();
        private readonly List<ObjectMetaDataStructure> _rootMetadataStrcutureObjects = new();
        private readonly List<Node> _rootNodes = new();


        public Defraggler_V1(PmCSharpDefinedTypes originalFile, PmCSharpDefinedTypes transactionFile)
        {
            _originalFile = originalFile;
            _allocator = new PAllocator(originalFile, transactionFile);
            _transactionFile = _allocator.TransactionFile;
            _pMemoryMetadataManager = new PMemoryMetadataManager(_allocator);
        }

        public void Defrag()
        {
            _startBlockOffset = _originalFile.ReadUInt(1);

            GetAllUsedRegions();
            CreateTreeReference();

            // Assert all used region is used
            MarkIsolatedRegionsToFree();

            // Merge two blocks with the same size
            MergeBlocks();

            // Verify if has block totally empty
            TreatEmptyBlocks();

        }

        private void MergeBlocks()
        {
            // TODO: Merge blocks 
        }

        private void MarkIsolatedRegionsToFree()
        {
            foreach (var usedRegion in _allUsedRegions)
            {
                var hasReference = false;
                foreach (var node in _rootNodes)
                {
                    if (node.GetChild(usedRegion) != null)
                    {
                        hasReference = true;
                    }
                }

                if (!hasReference)
                {
                    var region = _allocator.GetRegion(usedRegion.BlockId, usedRegion.RegionIndex);
                    region.Free();
                }
            }
        }

        private void TreatEmptyBlocks()
        {
            var blockLayout = _allocator.FirstPersistentBlockLayout;
            var emptyBlocks = new List<PersistentBlockLayout>();

            while (blockLayout != null)
            {
                if (blockLayout.FreeBlocks == 0)
                {
                    emptyBlocks.Add(blockLayout);
                }

                blockLayout = blockLayout.NextBlock;
            }

            foreach (var block in emptyBlocks)
            {
                _allocator.RemoveBlock(block);
            }
        }

        private void CreateTreeReference()
        {
            if (_allocator.FirstPersistentBlockLayout is null) return;

            var firstRegion = _allocator.FirstPersistentBlockLayout.Regions[0];
            var metadataReader = new MetadataReader(firstRegion);
            // Get all root objects
            while (metadataReader.TryGetNext(out var metadataStructure))
            {
                if (metadataStructure is ObjectMetaDataStructure objectMetaDataStructure)
                {
                    _rootMetadataStrcutureObjects.Add(objectMetaDataStructure);
                }
            }

            // Create a tree reference of all roots
            foreach (var root in _rootMetadataStrcutureObjects)
            {
                CreateTreeReference(root);
            }
        }

        private void CreateTreeReference(ObjectMetaDataStructure root)
        {
            var node = new Node();

            var assembly = Assembly.Load(root.AssemblyFullName);
            var type = assembly.GetType(root.ClassTypeName) ?? throw new ApplicationException($"type {root.ClassTypeName} not found for assembly {assembly.FullName}");
            var objectMapper = new ObjectPropertiesInfoMapper(type);
            foreach (var property in type.GetProperties())
            {
                if (!property.PropertyType.IsPrimitive)
                {
                    var region = _allocator.GetRegion(root.BlockID, root.RegionIndex);
                    var propertyOffset = objectMapper.GetPropertyOffset(property);
                    var blockID = BitConverter.ToUInt32(region.Read(count: sizeof(uint), offset: propertyOffset));

                    if (blockID == 0) continue; // null value

                    propertyOffset += sizeof(uint);
                    var regionIndex = region.Read(count: 1, offset: propertyOffset)[0];

                    node.RegionsReference.Add(new RegionsInUse(blockID, regionIndex, property.PropertyType));
                }
            }

            ReadChildren(node);

            _rootNodes.Add(node);
        }

        private void ReadChildren(Node node)
        {
            foreach (var regionReference in node.RegionsReference)
            {
                var childrenNode = new Node();

                var type = regionReference.RegionType;
                var objectMapper = new ObjectPropertiesInfoMapper(type);
                foreach (var property in type.GetProperties())
                {
                    var propType = property.PropertyType;
                    if (!propType.IsPrimitive)
                    {
                        var region = _allocator.GetRegion(regionReference.BlockId, regionReference.RegionIndex);
                        var propertyOffset = objectMapper.GetPropertyOffset(property);
                        var blockID = BitConverter.ToUInt32(region.Read(count: sizeof(uint), offset: propertyOffset));

                        if (blockID == 0) continue; // null value

                        propertyOffset += sizeof(uint);
                        var regionIndex = region.Read(count: 1, offset: propertyOffset)[0];

                        childrenNode.RegionsReference.Add(new RegionsInUse(blockID, regionIndex, property.PropertyType));
                        ReadChildren(childrenNode);
                    }
                }

                node.AddChild(childrenNode);
            }
        }

        private static string ReadString(PmCSharpDefinedTypes pmCsharpDefinedTypes, int offset)
        {
            var stringBytes = new List<byte>();
            while (true)
            {
                var @byte = pmCsharpDefinedTypes.ReadByte(offset);

                if (@byte == 0)
                {
                    break;
                }

                stringBytes.Add(@byte);
                offset += sizeof(byte);
            }
            return Encoding.UTF8.GetString(stringBytes.ToArray());
        }

        // Get All used regions and bockIds
        private void GetAllUsedRegions()
        {
            PersistentBlockLayout blockLayout = PersistentBlockLayout.LoadBlockLayoutFromPm(_startBlockOffset, _originalFile, _transactionFile);
            while (blockLayout != null)
            {
                if (!_pMemoryMetadataManager.IsMetadataBlock(blockLayout))
                {
                    foreach (var regionInUse in GetSetBits(blockLayout.FreeBlocks))
                    {
                        _allUsedRegions.Add(new RegionsInUse(blockLayout.BlockOffset, regionInUse, null));
                    }
                }
                if (blockLayout.NextBlockOffset == 0) break;
                blockLayout = PersistentBlockLayout.LoadBlockLayoutFromPm(blockLayout.NextBlockOffset, _originalFile, _transactionFile);
            }
        }

        public static List<byte> GetSetBits(ulong number)
        {
            List<byte> setBits = new();
            for (byte i = 0; i < sizeof(ulong) * 8; i++)
            {
                if ((number & (1UL << i)) != 0)
                {
                    setBits.Add(i);
                }
            }
            return setBits;
        }
    }
}
