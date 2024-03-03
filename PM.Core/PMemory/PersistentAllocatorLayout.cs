using Serilog;

namespace PM.Core.PMemory
{
    public class PersistentAllocatorLayout
    {
        private readonly Dictionary<int, List<PersistentBlockLayout>> _blocksBySize = new();
        private readonly Dictionary<uint, PersistentBlockLayout> _blocksByOffset = new();
        public IEnumerable<PersistentBlockLayout> Blocks => _blocksBySize.Values.SelectMany(x => x).ToList().AsReadOnly();

        private PmCSharpDefinedTypes? _pmCSharpDefinedTypes;
        public PmCSharpDefinedTypes? PmCSharpDefinedTypes
        {
            get => _pmCSharpDefinedTypes;
            set
            {
                _pmCSharpDefinedTypes = value;
                foreach (PersistentBlockLayout block in _blocksBySize.Values.SelectMany(x => x))
                {
                    block.PersistentMemory = value;
                }
            }
        }

        public uint TotalSizeBytes => (uint)(_blocksBySize.Values.SelectMany(x => x).Sum(x => x.TotalSizeBytes));

        public byte DefaultRegionQuantityPerBlock { get; set; } = 8;

        // Start with 1 to skip the commit byte.
        // This field is used to set block offset.
        private uint _blocksOffset = 1;

        // Used to set NextBlock property when next block is added.
        private PersistentBlockLayout? _lastBlock;

        public void AddBlock(PersistentBlockLayout persistentBlockLayout)
        {
            if (!_blocksBySize.TryGetValue(persistentBlockLayout.RegionsSize, out var blocksBySizeList))
            {
                blocksBySizeList = new List<PersistentBlockLayout>();
                _blocksBySize.Add(persistentBlockLayout.RegionsSize, blocksBySizeList);
            }
            blocksBySizeList.Add(persistentBlockLayout);

            _blocksByOffset.Add(_blocksOffset, persistentBlockLayout);

            persistentBlockLayout.BlockOffset = _blocksOffset;
            persistentBlockLayout.PersistentMemory = PmCSharpDefinedTypes;

            _blocksOffset += persistentBlockLayout.TotalSizeBytes;

            if (_lastBlock != null)
            {
                _lastBlock.NextBlock = persistentBlockLayout;
            }
            _lastBlock = persistentBlockLayout;
        } 
        
        internal void AddLoadedBlock(PersistentBlockLayout persistentBlockLayout, uint offset)
        {
            if (!_blocksBySize.TryGetValue(persistentBlockLayout.RegionsSize, out var blocksBySizeList))
            {
                blocksBySizeList = new List<PersistentBlockLayout>();
                _blocksBySize.Add(persistentBlockLayout.RegionsSize, blocksBySizeList);
            }
            blocksBySizeList.Add(persistentBlockLayout);

            _blocksByOffset.Add(persistentBlockLayout.BlockOffset, persistentBlockLayout);

            persistentBlockLayout.PersistentMemory = PmCSharpDefinedTypes;

            _blocksOffset = offset;
        }

        public PersistentBlockLayout GetOrCreateBlockBySize(int size)
        {
            if (_blocksBySize.TryGetValue(size, out var blocks))
            {
                foreach(var block in blocks)
                {
                    if (!block.IsFull)
                    {
                        return block;
                    }
                }
            }

            Log.Debug("Block of size {size} no found. Creating a new one...", size);
            var newBlock = new PersistentBlockLayout(size, DefaultRegionQuantityPerBlock);
            AddBlock(newBlock);
            newBlock.Configure();
            newBlock.WriteBlockLayoutOnPm();
            Log.Debug(
                "Created new pmemory block in offset: {blockOffset} " +
                "(region size: {regionsSize}, " +
                "region quantity: {regionsQuantity})",
                newBlock.BlockOffset,
                newBlock.RegionsSize,
                newBlock.RegionsQuantity);

            return newBlock;
        }

        internal void Configure()
        {
            foreach (PersistentBlockLayout block in _blocksBySize.Values.SelectMany(x => x))
            {
                block.Configure();
            }
        }

        internal PersistentBlockLayout GetBlockByID(uint blockID)
        {
            if (_blocksByOffset.TryGetValue(blockID, out var block))
            {
                return block;
            }
            throw new InvalidOperationException($"blockId={blockID} dont exist");
        }
    }
}
