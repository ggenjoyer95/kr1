using System.Collections.Generic;
using kr1.core;

namespace kr1.store
{
    public class DataStore
    {
        public List<Account> Accts { get; } = new List<Account>();
        public List<Category> Cats { get; } = new List<Category>();
        public List<Transaction> Trans { get; } = new List<Transaction>();

        public Account? FindAccount(Guid id) => Accts.Find(a => a.Id == id);
        public Category? FindCategory(Guid id) => Cats.Find(c => c.Id == id);
        public Transaction? FindTransaction(Guid id) => Trans.Find(t => t.Id == id);
    }
}
