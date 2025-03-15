using System;
using kr1.core;

namespace kr1.util
{
    public static class Creator
    {
        public static Account NewAccount(string name, decimal initBalance = 0)
        {
            return new Account(name, initBalance);
        }

        public static Category NewCategory(kr1.core.CategoryType type, string label)
        {
            return new Category(type, label);
        }

        public static Transaction NewTransaction(TransactionType type, Guid accountId, decimal amount, DateTime date, Guid categoryId, string? note = null)
        {
            return new Transaction(type, accountId, amount, date, categoryId, note);
        }
    }
}
