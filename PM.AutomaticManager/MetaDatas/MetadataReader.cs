using PM.Core.PMemory;

namespace PM.AutomaticManager.MetaDatas
{
    internal class MetadataReader
    {
        private readonly PersistentRegion _metadataRegion;
        private int _nextMetadataStructureInternalOffset;

        public MetadataReader(PersistentRegion metadataRegion)
        {
            _metadataRegion = metadataRegion;
            _nextMetadataStructureInternalOffset = 0;
        }

        public bool TryGetNext(out MetadataStructure metadataStructure)
        {
            try
            {
                metadataStructure = MetadataStructure.CreateFrom(_metadataRegion, _nextMetadataStructureInternalOffset);
                _nextMetadataStructureInternalOffset += metadataStructure.Size;
                return true;
            }
            catch
            {
                metadataStructure = null!;
                return false;
            }
        }
    }
}
