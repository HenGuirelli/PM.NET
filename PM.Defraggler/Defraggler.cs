using PM.Common;
using PM.Defraggler.Defragglers;

namespace PM.Defraggler
{
    public class Defraggler
    {
        private IDefraggler _internalDefreggler;

        public Defraggler(PmCSharpDefinedTypes originalFile, PmCSharpDefinedTypes transactionFile)
        {
            var defragglerFactory = new DefragglerFactory(originalFile, transactionFile);
            _internalDefreggler = defragglerFactory.Create();
        }

        public void Defrag()
        {
            _internalDefreggler.Defrag();
        }
    }
}
