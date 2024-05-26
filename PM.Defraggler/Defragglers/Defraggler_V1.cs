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
        private readonly Assembly _assembly;
        private uint _startBlockOffset;

        private List<ObjectMetaDataStructure> _rootMetadataStrcutureObjects = new();

        public Defraggler_V1(PmCSharpDefinedTypes originalFile, PmCSharpDefinedTypes transactionFile)
        {
            _originalFile = originalFile;
            _allocator = new PAllocator(originalFile, transactionFile);
            _transactionFile = _allocator.TransactionFile;
            _assembly = Assembly.Load(ReadAssemblyName());
        }

        public void Defrag()
        {
            _startBlockOffset = _originalFile.ReadUInt(1);

            var allReferences = GetAllReferences();
            CreateTreeReference();
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

            var type = _assembly.GetType(root.ClassTypeName) ?? throw new ApplicationException($"type {root.ClassTypeName} not found for assembly {_assembly.FullName}");
            var objectMapper = new ObjectPropertiesInfoMapper(type);
            foreach (var property in type.GetProperties())
            {
                var propType = property.PropertyType;
                if (!propType.IsPrimitive)
                {
                    var propertyOffset = objectMapper.GetPropertyOffset(property);
                    var blockID = _originalFile.ReadUInt(propertyOffset);
                    propertyOffset += sizeof(uint);
                    var regionIndex = _originalFile.ReadByte(propertyOffset);

                    node.RegionsReference.Add(new RegionsInUse(blockID, regionIndex, property.PropertyType));
                }
            }
            ReadChildren(node);
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
                        propertyOffset += sizeof(uint);
                        var regionIndex = region.Read(count: 1, offset: propertyOffset)[0];

                        childrenNode.RegionsReference.Add(new RegionsInUse(blockID, regionIndex, property.PropertyType));
                        ReadChildren(childrenNode);
                    }
                }

                node.AddChild(childrenNode);
            }
        }

        private string ReadAssemblyName()
        {
            return ReadString(_originalFile, 9);
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
        private HashSet<RegionsInUse> GetAllReferences()
        {
            var result = new HashSet<RegionsInUse>();

            PersistentBlockLayout blockLayout = PersistentBlockLayout.LoadBlockLayoutFromPm(_startBlockOffset, _originalFile, _transactionFile);
            while (blockLayout != null)
            {
                foreach (var regionInUse in GetSetBits(blockLayout.FreeBlocks))
                {
                    result.Add(new RegionsInUse(blockLayout.BlockOffset, regionInUse, null));
                }
                if (blockLayout.NextBlockOffset == 0) break;
                blockLayout = PersistentBlockLayout.LoadBlockLayoutFromPm(blockLayout.NextBlockOffset, _originalFile, _transactionFile);
            }

            return result;
        }

        public static List<byte> GetSetBits(ulong number)
        {
            List<byte> setBits = new();
            for (byte i = 0; i < sizeof(ulong); i++)
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
