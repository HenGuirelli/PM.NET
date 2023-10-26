namespace PM.Transactions
{
    public static class TransactionExtensions
    {
        private static readonly object _transactionLock = new object();

        public static void Transaction<T>(
            this T obj,
            Action transactionMethod)
        where T : class, new()
        {
            TransactionState _ = default;
            obj.RunTransaction(transactionMethod, ref _);
        }

        public static void RunTransaction<T>(
            this T obj,
            Action transactionMethod,
            ref TransactionState state)
        where T : class, new()
        {
            lock (_transactionLock)
            {
                var transaction = new TransactionManager<T>(obj, true);
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