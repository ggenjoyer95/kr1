using System;
using System.Collections.Generic;
using kr1.core;
using kr1.store;
using kr1.util;

namespace kr1.svc
{
    public class FinanceManager
    {
        private readonly AccountManager _accMgr;
        private readonly CategoryManager _catMgr;
        private readonly TransactionManager _tranMgr;
        private readonly Analytics _analytics;
        private readonly DataProxy _proxy;
        private readonly ITransactionFactory _tranFactory;

        public FinanceManager(AccountManager accMgr, CategoryManager catMgr, TransactionManager tranMgr, Analytics analytics, DataProxy proxy, ITransactionFactory tranFactory)
        {
            _accMgr = accMgr;
            _catMgr = catMgr;
            _tranMgr = tranMgr;
            _analytics = analytics;
            _proxy = proxy;
            _tranFactory = tranFactory;
        }

        public Account AddAccount(string name, decimal initBalance)
        {
            return _accMgr.CreateAccount(name, initBalance);
        }

        public Category AddCategory(CategoryType type, string label)
        {
            return _catMgr.CreateCategory(type, label);
        }

        public Transaction AddTransaction(TransactionType type, Guid accId, decimal amount, DateTime date, Guid catId, string? note = null)
        {
            var tran = _tranFactory.CreateTransaction(type, accId, amount, date, catId, note);
            _tranMgr.AddTransaction(tran);
            return tran;
        }

        public bool RemoveAccount(Guid id) => _accMgr.DeleteAccount(id);
        public bool RemoveCategory(Guid id) => _catMgr.DeleteCategory(id);
        public bool RemoveTransaction(Guid id) => _tranMgr.DeleteTransaction(id);

        public bool RenameAccount(Guid id, string newName) => _accMgr.UpdateAccount(id, newName);
        public bool RenameCategory(Guid id, CategoryType newType, string newLabel) => _catMgr.UpdateCategory(id, newType, newLabel);

        public decimal RecalculateBalance(Guid accId)
        {
            var a = _proxy.Accts.Find(x => x.Id == accId);
            if (a == null) throw new ArgumentException("Счет не найден", nameof(accId));
            decimal newBal = a.InitialBalance;
            foreach (var t in _proxy.Transactions)
            {
                if (t.AccountId == accId)
                {
                    if (t.Type == TransactionType.Income) {
                        newBal += t.Amount;
                    }
                    else if (t.Type == TransactionType.Expense) {
                        newBal -= t.Amount;
                    }
                }
            }
            _accMgr.SetBalance(accId, newBal);
            return newBal;
        }

        public decimal GetNetDifference(DateTime start, DateTime end)
        {
            return _analytics.CalculateNet(start, end);
        }

        public Dictionary<string, decimal> GetGroupByCategory()
        {
            return _analytics.GroupByCategory();
        }
    }
}
