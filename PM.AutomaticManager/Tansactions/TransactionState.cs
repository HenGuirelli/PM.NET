namespace PM.AutomaticManager.Tansactions
{
    public enum TransactionState : byte
    {
        NotStarted = 0,
        Running = 1,
        Commited = 2,
        RollBacked = 3
    }
}