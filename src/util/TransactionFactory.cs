using System;
using kr1.core;
using kr1.val;

namespace kr1.util
{
    public class TransactionFactory : ITransactionFactory
    {
        private readonly IValService _valService;
        public TransactionFactory(IValService valService)
        {
            _valService = valService ?? throw new ArgumentNullException(nameof(valService));
        }

        public Transaction CreateTransaction(TransactionType type, Guid accountId, decimal amount, DateTime date, Guid categoryId, string? note = null)
        {
            if (!_valService.IsAmountValid(amount))
                throw new ArgumentException("Сумма должна быть положительной.", nameof(amount));
            return new Transaction(type, accountId, amount, date, categoryId, note);
        }
    }
}
