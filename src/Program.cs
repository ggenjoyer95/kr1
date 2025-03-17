using System;
using Microsoft.Extensions.DependencyInjection;
using kr1.store;
using kr1.core;
using kr1.svc;
using kr1.util;
using kr1.val;
using YamlDotNet.Serialization;

namespace kr1
{
    class Program
    {
        static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<DataStore>();
            services.AddSingleton<DataProxy>();
            services.AddSingleton<AccountManager>();
            services.AddSingleton<CategoryManager>();
            services.AddSingleton<TransactionManager>();
            services.AddSingleton<Analytics>();
            services.AddSingleton<JsonImporter>();
            services.AddSingleton<CsvImporter>();
            services.AddSingleton<YamlImporter>();
            services.AddSingleton<Exporter>();
            services.AddSingleton<FinanceManager>();
            services.AddSingleton<IValService, ValService>();
            services.AddSingleton<ITransactionFactory, TransactionFactory>();
        }

        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            var provider = services.BuildServiceProvider();
            var store = provider.GetRequiredService<DataStore>();
            var accMgr = provider.GetRequiredService<AccountManager>();
            var catMgr = provider.GetRequiredService<CategoryManager>();
            var tranMgr = provider.GetRequiredService<TransactionManager>();
            var analytics = provider.GetRequiredService<Analytics>();
            var jsonImp = provider.GetRequiredService<JsonImporter>();
            var csvImp = provider.GetRequiredService<CsvImporter>();
            var yamlImp = provider.GetRequiredService<YamlImporter>();
            var exporter = provider.GetRequiredService<Exporter>();
            var finMgr = provider.GetRequiredService<FinanceManager>();

            while (true)
            {
                Console.WriteLine("Меню:");
                Console.WriteLine("1) Операции со счетами");
                Console.WriteLine("2) Операции с категориями");
                Console.WriteLine("3) Операции с транзакциями");
                Console.WriteLine("4) Аналитика");
                Console.WriteLine("5) Импорт/Экспорт данных");
                Console.WriteLine("6) Команда: Добавить счет (замер времени)");
                Console.WriteLine("7) Пересчитать баланс счета");
                Console.WriteLine("8) Демонстрация работы FinanceManager");
                Console.WriteLine("9) Выход");
                Console.Write("Ваш выбор: ");
                var choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        MenuAccounts(accMgr);
                        break;
                    case "2":
                        MenuCategories(catMgr);
                        break;
                    case "3":
                        MenuTransactions(tranMgr);
                        break;
                    case "4":
                        MenuAnalytics(analytics);
                        break;
                    case "5":
                        MenuDataIO(jsonImp, csvImp, yamlImp, exporter);
                        break;
                    case "6":
                        RunAddAccountAction(accMgr);
                        break;
                    case "7":
                        RecalcBalance(accMgr, store);
                        break;
                    case "8":
                        FinMgrDemo(finMgr);
                        break;
                    case "9":
                        return;
                    default:
                        Console.WriteLine("Неверный выбор. Попробуйте еще раз.\n");
                        break;
                }
            }
        }

        static void MenuAccounts(AccountManager mgr)
        {
            Console.WriteLine("1) Добавить счет");
            Console.WriteLine("2) Показать все счета");
            Console.WriteLine("3) Удалить счет");
            Console.WriteLine("4) Назад");
            Console.Write("Ваш выбор: ");
            var c = Console.ReadLine();
            Console.WriteLine();
            switch (c)
            {
                case "1":
                    Console.Write("Введите имя счета: ");
                    var name = Console.ReadLine() ?? "";
                    Console.Write("Введите начальный баланс: ");
                    decimal.TryParse(Console.ReadLine(), out decimal bal);
                    var acc = mgr.CreateAccount(name, bal);
                    Console.WriteLine($"Счет добавлен: {acc.Id} | {acc.Name} | Баланс: {acc.Balance}\n");
                    break;
                case "2":
                    var list = mgr.GetAccounts();
                    Console.WriteLine("=== СЧЕТА ===");
                    foreach (var a in list)
                        Console.WriteLine($"{a.Id} | {a.Name} | Баланс: {a.Balance}");
                    Console.WriteLine();
                    break;
                case "3":
                    Console.Write("Введите ID счета для удаления: ");
                    var idStr = Console.ReadLine();
                    if (Guid.TryParse(idStr, out Guid id))
                    {
                        bool res = mgr.DeleteAccount(id);
                        Console.WriteLine(res ? "Счет удален.\n" : "Счет не найден.\n");
                    }
                    else
                        Console.WriteLine("Неверный формат ID.\n");
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Неверный выбор.\n");
                    break;
            }
        }

        static void MenuCategories(CategoryManager mgr)
        {
            Console.WriteLine("1) Добавить категорию");
            Console.WriteLine("2) Показать все категории");
            Console.WriteLine("3) Удалить категорию");
            Console.WriteLine("4) Назад");
            Console.Write("Ваш выбор: ");
            var c = Console.ReadLine();
            Console.WriteLine();
            switch (c)
            {
                case "1":
                    Console.Write("Введите тип категории (Доход/Расход): ");
                    var tStr = Console.ReadLine();
                    if (!Enum.TryParse(tStr, true, out CategoryType ct))
                    {
                        Console.WriteLine("Неверный тип.\n");
                        return;
                    }
                    Console.Write("Введите название категории: ");
                    var label = Console.ReadLine() ?? "";
                    var cat = mgr.CreateCategory(ct, label);
                    Console.WriteLine($"Категория добавлена: {cat.Id} | {cat.Type} | {cat.Label}\n");
                    break;
                case "2":
                    var list = mgr.GetCategories();
                    Console.WriteLine("=== КАТЕГОРИИ ===");
                    foreach (var citem in list)
                        Console.WriteLine($"{citem.Id} | {citem.Type} | {citem.Label}");
                    Console.WriteLine();
                    break;
                case "3":
                    Console.Write("Введите ID категории для удаления: ");
                    var idStr = Console.ReadLine();
                    if (Guid.TryParse(idStr, out Guid id))
                    {
                        bool res = mgr.DeleteCategory(id);
                        Console.WriteLine(res ? "Категория удалена.\n" : "Категория не найдена.\n");
                    }
                    else
                        Console.WriteLine("Неверный формат ID.\n");
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Неверный выбор.\n");
                    break;
            }
        }

        static void MenuTransactions(TransactionManager mgr)
        {
            Console.WriteLine("1) Добавить транзакцию");
            Console.WriteLine("2) Показать все транзакции");
            Console.WriteLine("3) Удалить транзакцию");
            Console.WriteLine("4) Назад");
            Console.Write("Ваш выбор: ");
            var c = Console.ReadLine();
            Console.WriteLine();
            switch (c)
            {
                case "1":
                    Console.Write("Введите тип транзакции (Доход/Расход): ");
                    var tStr = Console.ReadLine();
                    if (!Enum.TryParse(tStr, true, out TransactionType tt))
                    {
                        Console.WriteLine("Неверный тип.\n");
                        return;
                    }
                    Console.Write("Введите сумму: ");
                    if (!decimal.TryParse(Console.ReadLine(), out decimal amt))
                    {
                        Console.WriteLine("Неверный формат суммы.\n");
                        return;
                    }
                    var field = mgr.GetType().GetField("_store", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var store = field?.GetValue(mgr) as DataStore;
                    if (store == null || store.Accts.Count == 0 || store.Cats.Count == 0)
                    {
                        Console.WriteLine("Сначала создайте счет и категорию.\n");
                        return;
                    }
                    var acc = store.Accts[0];
                    var cat = store.Cats[0];
                    Console.Write("Введите примечание (не обязательно): ");
                    var note = Console.ReadLine();
                    var tran = Creator.NewTransaction(tt, acc.Id, amt, DateTime.Now, cat.Id, note);
                    mgr.AddTransaction(tran);
                    Console.WriteLine("Транзакция добавлена.\n");
                    break;
                case "2":
                    var list = mgr.GetTransactions();
                    if (list == null || !list.Any())
                    {
                        Console.WriteLine("Нет транзакций.\n");
                        return;
                    }
                    Console.WriteLine("Транзакции:");
                    foreach (var t in list)
                        Console.WriteLine($"{t.Id} | {t.Type} | {t.Amount} | {t.Date} | {t.Note} | {t.CategoryId}");
                    Console.WriteLine();
                    break;
                case "3":
                    Console.Write("Введите ID транзакции для удаления: ");
                    var idStr = Console.ReadLine();
                    if (Guid.TryParse(idStr, out Guid id))
                    {
                        bool res = mgr.DeleteTransaction(id);
                        Console.WriteLine(res ? "Транзакция удалена.\n" : "Транзакция не найдена.\n");
                    }
                    else
                        Console.WriteLine("Неверный формат ID.\n");
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Неверный выбор.\n");
                    break;
            }
        }

        static void MenuAnalytics(Analytics stats)
        {
            Console.WriteLine("1) Рассчитать чистую разницу");
            Console.WriteLine("2) Группировка по категориям");
            Console.WriteLine("3) Назад");
            Console.Write("Ваш выбор: ");
            var c = Console.ReadLine();
            Console.WriteLine();
            switch (c)
            {
                case "1":
                    Console.Write("Введите дату начала (год-месяц-день 2001-04-05): ");
                    if (!DateTime.TryParse(Console.ReadLine(), out DateTime start))
                    {
                        Console.WriteLine("Неверный формат даты.\n");
                        return;
                    }
                    Console.Write("Введите дату окончания (год-месяц-день 2001-04-05): ");
                    if (!DateTime.TryParse(Console.ReadLine(), out DateTime end))
                    {
                        Console.WriteLine("Неверный формат даты.\n");
                        return;
                    }
                    var net = stats.CalculateNet(start, end);
                    Console.WriteLine($"Чистая разница: {net}\n");
                    break;
                case "2":
                    var groups = stats.GroupByCategory();
                    Console.WriteLine("группировка по категориям:");
                    foreach (var kvp in groups)
                        Console.WriteLine($"{kvp.Key}: {kvp.Value}");
                    Console.WriteLine();
                    break;
                case "3":
                    return;
                default:
                    Console.WriteLine("Неверный выбор.\n");
                    break;
            }
        }

        static void MenuDataIO(JsonImporter jImp, CsvImporter cImp, YamlImporter yImp, kr1.svc.Exporter exp)
        {
            Console.WriteLine("1) Импорт из JSON");
            Console.WriteLine("2) Импорт из CSV");
            Console.WriteLine("3) Импорт из YAML");
            Console.WriteLine("4) Экспорт в JSON");
            Console.WriteLine("5) Экспорт в CSV");
            Console.WriteLine("6) Экспорт в YAML");
            Console.WriteLine("7) Назад");
            Console.Write("Ваш выбор: ");
            var c = Console.ReadLine();
            Console.WriteLine();
            switch (c)
            {
                case "1":
                    Console.Write("Введите путь к JSON файлу: ");
                    var jp = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(jp))
                    {
                        Console.WriteLine("Путь не может быть пустым.\n");
                        return;
                    }
                    try { jImp.Import(jp); Console.WriteLine("Импорт из JSON выполнен успешно.\n"); }
                    catch (Exception ex) { Console.WriteLine("Ошибка импорта из JSON: " + ex.Message + "\n"); }
                    break;
                case "2":
                    Console.Write("Введите путь к CSV файлу: ");
                    var cp = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(cp))
                    {
                        Console.WriteLine("Путь не может быть пустым.\n");
                        return;
                    }
                    try { cImp.Import(cp); Console.WriteLine("Импорт из CSV выполнен успешно.\n"); }
                    catch (Exception ex) { Console.WriteLine("Ошибка импорта из CSV: " + ex.Message + "\n"); }
                    break;
                case "3":
                    Console.Write("Введите путь к YAML файлу: ");
                    var yp = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(yp))
                    {
                        Console.WriteLine("Путь не может быть пустым.\n");
                        return;
                    }
                    try { yImp.Import(yp); Console.WriteLine("Импорт из YAML выполнен успешно.\n"); }
                    catch (Exception ex) { Console.WriteLine("Ошибка импорта из YAML: " + ex.Message + "\n"); }
                    break;
                case "4":
                    var jExp = new JsonExp();
                    exp.Export(jExp);
                    var jOut = jExp.GetJson();
                    Console.WriteLine("экспорт в JSON");
                    Console.WriteLine(jOut);
                    Console.WriteLine();
                    break;
                case "5":
                    var cExp = new CsvExp();
                    exp.Export(cExp);
                    var cOut = cExp.GetCsv();
                    Console.WriteLine("экспорт в CSV");
                    Console.WriteLine(cOut);
                    Console.WriteLine();
                    break;
                case "6":
                    var yExp = new YamlExp();
                    exp.Export(yExp);
                    var yOut = yExp.GetYaml();
                    Console.WriteLine("экспорт в YAML");
                    Console.WriteLine(yOut);
                    Console.WriteLine();
                    break;
                case "7":
                    return;
                default:
                    Console.WriteLine("Неверный выбор.\n");
                    break;
            }
        }

        static void RunAddAccountAction(AccountManager mgr)
        {
            Console.Write("Введите имя нового счета: ");
            var name = Console.ReadLine() ?? "";
            Console.Write("Введите начальный баланс: ");
            decimal.TryParse(Console.ReadLine(), out decimal bal);
            var action = new AddAccountAction(mgr, name, bal);
            var timed = new TimeDecorator(action);
            timed.Execute();
            Console.WriteLine();
        }

        static void RecalcBalance(AccountManager mgr, DataStore store)
        {
            Console.Write("Введите ID счета для пересчета баланса: ");
            var idStr = Console.ReadLine();
            if (Guid.TryParse(idStr, out Guid id))
            {
                var a = mgr.GetAccountById(id);
                if (a == null)
                {
                    Console.WriteLine("Счет не найден.\n");
                    return;
                }
                decimal newBal = a.InitialBalance;
                foreach (var t in store.Trans)
                {
                    if (t.AccountId == id)
                    {
                        if (t.Type == TransactionType.Income) {
                            newBal += t.Amount;
                        }
                        else if (t.Type == TransactionType.Expense) {
                            newBal -= t.Amount;
                        }
                    }
                }
                bool res = mgr.SetBalance(id, newBal);
                Console.WriteLine(res ? $"Новый баланс: {newBal}\n" : "Ошибка пересчета.\n");
            }
            else {
                Console.WriteLine("Неверный формат ID.\n");
            }
        }

        static void FinMgrDemo(FinanceManager fm)
        {
            Console.WriteLine("Работа:");
            var a = fm.AddAccount("Демо счет", 1000);
            var c = fm.AddCategory(CategoryType.Income, "Демо зарплата");
            var t = fm.AddTransaction(TransactionType.Income, a.Id, 500, DateTime.Now, c.Id, "Демо доход");
            Console.WriteLine($"Счет: {a.Id} | {a.Name} | Баланс: {a.Balance}");
            Console.WriteLine($"Категория: {c.Id} | {c.Label} | {c.Type}");
            Console.WriteLine($"Транзакция: {t.Id} | {t.Type} | {t.Amount}");
            var nb = fm.RecalculateBalance(a.Id);
            Console.WriteLine($"Пересчитанный баланс: {nb}\n");
        }
    }
}
