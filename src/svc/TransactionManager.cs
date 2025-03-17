using System;
using System.Collections.Generic;
using kr1.core;
using kr1.store;
using kr1.util;

namespace kr1.svc
{
    public class TransactionManager
    {
        private readonly DataStore _store;
        public TransactionManager(DataStore store)
        {
            _store = store;
        }

        public void AddTransaction(Transaction t)
        {
            if (t == null) {
                throw new ArgumentNullException(nameof(t));
            }
            _store.Trans.Add(t);
        }

        public IEnumerable<Transaction> GetTransactions() => _store.Trans;

        public Transaction? GetTransactionById(Guid id) => _store.FindTransaction(id);

        public bool UpdateTransaction(Transaction newT)
        {
            if (newT == null)
                throw new ArgumentNullException(nameof(newT));
            int idx = _store.Trans.FindIndex(t => t.Id == newT.Id);
            if (idx < 0) return false;
            _store.Trans[idx] = newT;
            return true;
        }

        public bool DeleteTransaction(Guid id)
        {
            var t = GetTransactionById(id);
            if (t == null) {
                return false;
            }
            return _store.Trans.Remove(t);
        }
    }
}
