using System;
using System.Collections.Generic;
using System.Linq;
using kr1.core;
using kr1.store;

namespace kr1.svc
{
    public class Analytics
    {
        private readonly DataStore _store;
        public Analytics(DataStore store)
        {
            _store = store;
        }

        public decimal CalculateNet(DateTime start, DateTime end)
        {
            var trans = _store.Trans.Where(t => t.Date >= start && t.Date <= end);
            decimal sumIn = trans.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            decimal sumOut = trans.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            return sumIn - sumOut;
        }

        public Dictionary<string, decimal> GroupByCategory()
        {
            var dict = new Dictionary<string, decimal>();
            foreach(var t in _store.Trans)
            {
                var cat = _store.Cats.FirstOrDefault(c => c.Id == t.CategoryId);
                string key = cat?.Label ?? "Нет";
                if (!dict.ContainsKey(key)) {
                    dict[key] = 0;
                }
                if(t.Type == TransactionType.Income) {
                    dict[key] += t.Amount;
                }
                else if(t.Type == TransactionType.Expense) {
                    dict[key] -= t.Amount;
                }
            }
            return dict;
        }
    }
}
