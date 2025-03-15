using System.Collections.Generic;
using kr1.core;

namespace kr1.store
{
    public class DataProxy
    {
        private readonly DataStore _store;
        public DataProxy(DataStore store)
        {
            _store = store;
        }
        public List<Account> Accts => _store.Accts;
        public List<Category> Cats => _store.Cats;
        public List<Transaction> Transactions => _store.Trans;
    }
}
