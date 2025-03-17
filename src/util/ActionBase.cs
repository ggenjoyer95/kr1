using System;
using System.Diagnostics;
using kr1.svc;

namespace kr1.util
{
    public interface IAction
    {
        void Execute();
    }

    public class TimeDecorator : IAction
    {
        private readonly IAction _action;
        public TimeDecorator(IAction action)
        {
            _action = action;
        }
        public void Execute()
        {
            var sw = Stopwatch.StartNew();
            _action.Execute();
            sw.Stop();
            Console.WriteLine($"Время выполнения: {sw.ElapsedMilliseconds} мс");
        }
    }

    public class AddAccountAction : IAction
    {
        private readonly AccountManager _accMgr;
        private readonly string _name;
        private readonly decimal _balance;

        public AddAccountAction(AccountManager accMgr, string name, decimal balance)
        {
            _accMgr = accMgr;
            _name = name;
            _balance = balance;
        }

        public void Execute()
        {
            var a = _accMgr.CreateAccount(_name, _balance);
            Console.WriteLine($"Счет создан: {a.Id} | {_name} | Баланс: {a.Balance}");
        }
    }
}
