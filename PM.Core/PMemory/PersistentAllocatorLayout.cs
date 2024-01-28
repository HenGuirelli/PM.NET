using Serilog;
using System.Drawing;

namespace PM.Core.PMemory
{
    public class PersistentAllocatorLayout
    {
        private readonly Dictionary<int, PersistentBlockLayout> _blocksBySize = new();
        private readonly Dictionary<int, PersistentBlockLayout> _blocksByOffset = new();
        public IEnumerable<PersistentBlockLayout> Blocks => _blocksBySize.Values.ToList().AsReadOnly();

        private PmCSharpDefinedTypes? _pmCSharpDefinedTypes;
        public PmCSharpDefinedTypes? PmCSharpDefinedTypes
        {
            get => _pmCSharpDefinedTypes;
            set
            {
                _pmCSharpDefinedTypes = value;
                foreach (PersistentBlockLayout block in _blocksBySize.Values)
                {
                    block.PersistentMemory = value;
                }
            }
        }

        public int TotalSizeBytes => _blocksBySize.Sum(x => x.Value.TotalSizeBytes);

        public byte DefaultRegionQuantityPerBlock { get; set; } = 8;

        // Start with 1 to skip the commit byte
        private int _blocksOffset = 1;

        public void AddBlock(PersistentBlockLayout persistentBlockLayout)
        {
            _blocksBySize.Add(persistentBlockLayout.RegionsSize, persistentBlockLayout);
            _blocksByOffset.Add(_blocksOffset, persistentBlockLayout);

            persistentBlockLayout.BlockOffset = _blocksOffset;
            persistentBlockLayout.PersistentMemory = PmCSharpDefinedTypes;

            _blocksOffset += persistentBlockLayout.TotalSizeBytes;
        }

        public PersistentBlockLayout GetOrCreateBlockBySize(int size)
        {
            if (_blocksBySize.TryGetValue(size, out var block))
            {
                return block;
            }

            var newBlock = new PersistentBlockLayout(size, DefaultRegionQuantityPerBlock);
            AddBlock(newBlock);
            newBlock.Configure();
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
            foreach (PersistentBlockLayout block in _blocksBySize.Values)
                block.Configure();
        }

        internal PersistentBlockLayout GetBlockByID(int blockID)
        {
            if (_blocksByOffset.TryGetValue(blockID, out var block))
            {
                return block;
            }
            throw new InvalidOperationException($"blockId={blockID} dont exist");
        }
    }
}
