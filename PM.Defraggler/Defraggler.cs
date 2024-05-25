using PM.Common;

namespace PM.Defraggler
{
    public class Defraggler
    {
        private PmCSharpDefinedTypes _originalFile;
        private PmCSharpDefinedTypes _transactionFile;

        public Defraggler(PmCSharpDefinedTypes originalFile, PmCSharpDefinedTypes transactionFile)
        {
            _originalFile = originalFile;
            _transactionFile = transactionFile;

            var version = GetFileVersion();
            var defragglerFactory = new DefragglerFactory(filePath);
            var internalDefreggler =
        }

        private object GetFileVersion()
        {
            _originalFile.read
        }

        public void Defrag()
        {
            var allReferences = GetallReferences();
            var treeReference = CreateTreeReference();
        }
    }
}
