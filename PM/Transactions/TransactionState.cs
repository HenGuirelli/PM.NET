namespace PM.Transactions
{
    public enum TransactionState
    {
        NotStarted,
        Running,
        Commited,
        RollBacked,

        Finished = Commited | RollBacked
    }
}