namespace PM.Core.PMemory
{
    public interface IPersistentMemoryBlocksLayout
    {
        IEnumerable<IPersistentBlockLayout> Blocks { get; }
    }

    public class PersistentMemoryBlocksLayout : IPersistentMemoryBlocksLayout, IPersistentObject
    {
        public IEnumerable<IPersistentBlockLayout> Blocks { get; }

        public PersistentMemoryBlocksLayout()
        {
            Blocks = new List<IPersistentBlockLayout>();
        }

        public void Load()
        {
            throw new NotImplementedException();
        }
    }
}
