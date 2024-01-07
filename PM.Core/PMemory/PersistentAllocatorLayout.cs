using Serilog;

namespace PM.Core.PMemory
{
    public class PersistentAllocatorLayout
    {
        private readonly Dictionary<int, PersistentBlockLayout> _blocks = new();
        public IEnumerable<PersistentBlockLayout> Blocks => _blocks.Values.ToList().AsReadOnly();

        private PmCSharpDefinedTypes? _pmCSharpDefinedTypes;
        public PmCSharpDefinedTypes? PmCSharpDefinedTypes
        {
            get => _pmCSharpDefinedTypes;
            set
            {
                _pmCSharpDefinedTypes = value;
                foreach (PersistentBlockLayout block in _blocks.Values)
                {
                    block.PersistentMemory = value;
                }
            }
        }

        public int TotalSizeBytes => _blocks.Sum(x => x.Value.TotalSizeBytes);

        public byte DefaultRegionQuantityPerBlock { get; set; } = 8;

        // Start with 1 to skip the commit byte
        private int _blocksOffset = 1;

        public void AddBlock(PersistentBlockLayout persistentBlockLayout)
        {
            _blocks.Add(persistentBlockLayout.RegionsSize, persistentBlockLayout);

            persistentBlockLayout.BlockOffset = _blocksOffset;
            persistentBlockLayout.PersistentMemory = _pmCSharpDefinedTypes;

            _blocksOffset += persistentBlockLayout.TotalSizeBytes;
        }

        public PersistentBlockLayout GetOrCreateBlockBySize(int size)
        {
            if (_blocks.TryGetValue(size, out var block))
            {
                return block;
            }

            var newBlock = _blocks[size] = new PersistentBlockLayout(size, DefaultRegionQuantityPerBlock);
            newBlock.Configure();
            Log.Verbose($"Created new pmemory block (region size: {size}, region quantity: {DefaultRegionQuantityPerBlock}");

            return newBlock;
        }

        internal void Configure()
        {
            foreach (PersistentBlockLayout block in _blocks.Values)
                block.Configure();
        }
    }
}
