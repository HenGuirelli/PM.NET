namespace PM.FileEngine.Transactions
{
    public interface IBlockLayout
    {
        void ApplyInOriginalFile(PmCSharpDefinedTypes transactionFile, PAllocator pAllocator);
        void WriteTo(PmCSharpDefinedTypes transactionFile);
    }
}
