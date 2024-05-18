namespace PM.AutomaticManager.Tansactions
{
    public static class TransactionExtensions
    {
        private static readonly object _transactionLock = new object();

        public static void Transaction(
            this object obj,
            PMemoryManager pMemoryManager,
            Action transactionMethod)
        {
            TransactionState _ = default;
            obj.Transaction(pMemoryManager, transactionMethod, ref _);
        }

        public static void Transaction(
            this object obj,
            PMemoryManager pMemoryManager,
            Action transactionMethod,
            ref TransactionState state)
        {
            lock (_transactionLock)
            {
                var transaction = new TransactionManager(pMemoryManager, obj);
                state = transaction.State;
                try
                {
                    transaction.Begin();
                    state = transaction.State;

                    transactionMethod.Invoke();

                    transaction.Commit();
                    state = transaction.State;
                }
                catch
                {
                    transaction.RollBack();
                    state = transaction.State;
                    throw;
                }
            }
        }
    }
}