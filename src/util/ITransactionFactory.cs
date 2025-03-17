using System;
using kr1.core;

namespace kr1.util
{
    public interface ITransactionFactory
    {
        Transaction CreateTransaction(TransactionType type, Guid accountId, 
            decimal amount, DateTime date, Guid categoryId, string? note = null);
    }
}
