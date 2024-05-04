namespace PM.FileEngine.Transactions
{
    public class TransacionFileVersionException : ApplicationException
    {
        public TransacionFileVersionException(ushort expectedVersion, ushort actualVersion)
        : base ($"Transaction file version invalid. Expected: {expectedVersion}. Actual: {actualVersion}"){ }
    }
}
