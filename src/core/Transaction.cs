using System;

namespace kr1.core
{
    public enum TransactionType
    {
        Income,
        Expense
    }

    public class Transaction
    {
        public Guid Id { get; private set; }
        public TransactionType Type { get; set; }
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Note { get; set; }
        public Guid CategoryId { get; set; }

        public Transaction(TransactionType type, Guid accountId, decimal amount, DateTime date, Guid categoryId, string? note = null)
        {
            if (amount <= 0) {
                throw new ArgumentException("Сумма должна быть больше нуля.", nameof(amount));
            }
            Id = Guid.NewGuid();
            Type = type;
            AccountId = accountId;
            Amount = amount;
            Date = date;
            CategoryId = categoryId;
            Note = note;
        }
    }
}
