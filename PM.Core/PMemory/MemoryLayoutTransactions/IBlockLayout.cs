using PM.Core.PMemory.FileFields;

namespace PM.Core.PMemory.MemoryLayoutTransactions
{
    public interface IBlockLayout
    {
        OrderField Order { get; }
        void ApplyInOriginalFile(PmCSharpDefinedTypes transactionFile, PAllocator pAllocator);
        void WriteTo(PmCSharpDefinedTypes transactionFile);
    }
}
