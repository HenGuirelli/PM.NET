using PM.Common;
using PM.Defraggler.Defragglers;

namespace PM.Defraggler
{
    internal class DefragglerFactory
    {
        private readonly PmCSharpDefinedTypes _originalFile;
        private readonly PmCSharpDefinedTypes _transactionFile;

        public DefragglerFactory(PmCSharpDefinedTypes originalFile, PmCSharpDefinedTypes transactionFile)
        {
            _originalFile = originalFile;
            _transactionFile = transactionFile;
        }

        internal IDefraggler Create()
        {
            var version = _originalFile.ReadUInt(offset: 5);
            return CreateByVersion(version);
        }

        private IDefraggler CreateByVersion(uint version)
        {
            return version switch
            {
                1 => new Defraggler_V1(_originalFile, _transactionFile),
                _ => throw new ApplicationException($"File version {version} not supported for this defraggler"),
            };
        }
    }
}