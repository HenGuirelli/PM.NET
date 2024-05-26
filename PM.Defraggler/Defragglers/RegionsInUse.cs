namespace PM.Defraggler.Defragglers
{
    internal class RegionsInUse : IComparable<RegionsInUse>, IEquatable<RegionsInUse>
    {
        public uint BlockId { get; }
        public byte RegionIndex { get; }
        public Type RegionType { get; }

        public RegionsInUse(uint blockId, byte regionIndex, Type type)
        {
            BlockId = blockId;
            RegionIndex = regionIndex;
            RegionType = type;
        }

        public int CompareTo(RegionsInUse? other)
        {
            if (other == null) return 1;

            int blockIdComparison = BlockId.CompareTo(other.BlockId);
            if (blockIdComparison != 0)
            {
                return blockIdComparison;
            }

            return RegionIndex.CompareTo(other.RegionIndex);
        }

        public bool Equals(RegionsInUse? other)
        {
            if (other == null) return false;

            return BlockId == other.BlockId && RegionIndex == other.RegionIndex;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is RegionsInUse other) return Equals(other);
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + BlockId.GetHashCode();
                hash = hash * 23 + RegionIndex.GetHashCode();
                return hash;
            }
        }
    }
}
