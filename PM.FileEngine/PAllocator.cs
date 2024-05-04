using PM.Core.PMemory;
using PM.FileEngine.Transactions;

namespace PM.FileEngine
{
    public interface IPAllocatorAddBlock
    {
        void Addblock(PersistentBlockLayout persistentBlockLayout);
    }

    public interface IPAllocatorRemoveBlock
    {
        void Removeblock(PersistentBlockLayout persistentBlockLayout);
    }


    internal class PAllocatorEngineRemove : IPAllocatorRemoveBlock
    {
        private PmCSharpDefinedTypes _originalFile;
        private TransactionFile _transactionFile;
        private PersistentBlockLayout _firstPersistentBlockLayout;

        public PAllocatorEngineRemove(
            PmCSharpDefinedTypes originalFile,
            TransactionFile transactionFile,
            // Used to get address
            PersistentBlockLayout firstPersistentBlockLayout)
        {
            _originalFile = originalFile;
            _transactionFile = transactionFile;
            _firstPersistentBlockLayout = firstPersistentBlockLayout;
        }

        public void Removeblock(PersistentBlockLayout persistentBlockLayout)
        {
            var removedBlock = _firstPersistentBlockLayout;
            PersistentBlockLayout? beforeBlock = null;
            PersistentBlockLayout? afterBlock = null;
            while (removedBlock.NextBlock != null)
            {
                if (removedBlock == persistentBlockLayout) break;
                beforeBlock = removedBlock;
                removedBlock = removedBlock.NextBlock;
            }

            _transactionFile.AddRemoveBlockLayout(new RemoveBlockLayout(
                beforeBlockOffset: beforeBlock?.BlockOffset ?? 0,
                removedBlockOffset: persistentBlockLayout.BlockOffset,
                afterBlockOffset: afterBlock?.BlockOffset ?? 0));

            _transactionFile.ApplyPendingTransaction();
        }
    }

    internal class PAllocatorEngineAdd : IPAllocatorAddBlock
    {
        private readonly PmCSharpDefinedTypes _originalFile;
        private readonly TransactionFile _transactionFile;

        private PersistentBlockLayout _lastBlock;

        public PAllocatorEngineAdd(
            PmCSharpDefinedTypes originalFile,
            TransactionFile transactionFile,
            // Used to get address
            PersistentBlockLayout firstPersistentBlockLayout)
        {
            _originalFile = originalFile;
            _transactionFile = transactionFile;

            var block = firstPersistentBlockLayout;
            while (block.NextBlock != null) { block = block.NextBlock; }
            _lastBlock = block;
        }

        public void Addblock(PersistentBlockLayout persistentBlockLayout)
        {
            var newBlockOffset = _lastBlock.BlockOffset + _lastBlock.TotalSizeBytes;
            _transactionFile.AddNewBlockLayout(
                new AddBlockLayout(
                    startBlockOffset: newBlockOffset,
                    regionsQtty: persistentBlockLayout.RegionsQuantity,
                    regionsSize: persistentBlockLayout.RegionsSize
                ));

            // This operation modify original file
            _lastBlock.NextBlockOffset = newBlockOffset;

            _transactionFile.ApplyPendingTransaction();
        }
    }

    public class PAllocator
    {

        public PmCSharpDefinedTypes PersistentMemory { get; }

        private TransactionFile _transactionFile;

        public PAllocator(PmCSharpDefinedTypes persistentMemory, PmCSharpDefinedTypes transactionFile)
        {
            PersistentMemory = persistentMemory;
            _transactionFile = new TransactionFile(transactionFile, this);
        }

        internal void WriteBlockLayout(uint value, PersistentBlockLayout block)
        {
            throw new NotImplementedException();
        }
    }
}
