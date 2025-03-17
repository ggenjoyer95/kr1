using System;

namespace kr1.core
{
    public class Account
    {
        public Guid Id { get; private set; }
        public string Name { get; set; }
        public decimal Balance { get; private set; }
        public decimal InitialBalance { get; private set; }

        public Account(string name, decimal initialBalance = 0)
        {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Имя не может быть пустым.", nameof(name));
            }
            if (initialBalance < 0) {
                throw new ArgumentException("Начальный баланс не может быть отрицательным.", nameof(initialBalance));
            }

            Id = Guid.NewGuid();
            Name = name;
            Balance = initialBalance;
            InitialBalance = initialBalance;
        }

        public void Credit(decimal amount)
        {
            if (amount < 0) {
                throw new ArgumentException("Сумма должна быть положительной.", nameof(amount));
            }
            Balance += amount;
        }

        public void Debit(decimal amount)
        {
            if (amount < 0) {
                throw new ArgumentException("Сумма должна быть положительной.", nameof(amount));
            }
            if (Balance < amount) {
                throw new InvalidOperationException("Недостаточно средств.");
            }
            Balance -= amount;
        }

        public void UpdateBalance(decimal newBalance)
        {
            Balance = newBalance;
        }
    }
}
