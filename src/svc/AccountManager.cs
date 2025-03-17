using System;
using System.Collections.Generic;
using kr1.core;
using kr1.store;
using kr1.util;

namespace kr1.svc
{
    public class AccountManager
    {
        private readonly DataStore _store;
        public AccountManager(DataStore store)
        {
            _store = store;
        }

        public Account CreateAccount(string name, decimal initBalance)
        {
            var a = Creator.NewAccount(name, initBalance);
            _store.Accts.Add(a);
            return a;
        }

        public IEnumerable<Account> GetAccounts() => _store.Accts;

        public Account? GetAccountById(Guid id) => _store.FindAccount(id);

        public bool UpdateAccount(Guid id, string newName)
        {
            var a = GetAccountById(id);
            if (a == null) {
                return false;
            }
            if (string.IsNullOrWhiteSpace(newName)) {
                throw new ArgumentException("Имя не может быть пустым.", nameof(newName));
            }
            a.Name = newName;
            return true;
        }

        public bool DeleteAccount(Guid id)
        {
            var a = GetAccountById(id);
            if (a == null) {
                return false;
            }
            return _store.Accts.Remove(a);
        }

        public bool SetBalance(Guid id, decimal newBalance)
        {
            var a = GetAccountById(id);
            if (a == null) {
                return false;
            }
            a.UpdateBalance(newBalance);
            return true;
        }
    }
}
