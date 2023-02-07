namespace PM.Transactions
{
    public class PmTransactionException : ApplicationException
    {
        public PmTransactionException(string cause) : base(cause) { }
    }
}