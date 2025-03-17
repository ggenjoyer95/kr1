using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;
using kr1.core;
using kr1.store;
using kr1.svc;
using kr1.util;
using kr1.val;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace kr1.Tests
{
    public class Tests
    {

        [Fact]
        public void Account_Creation_SetsPropertiesCorrectly()
        {
            var acc = new Account("TestAccount", 1000);
            Assert.Equal("TestAccount", acc.Name);
            Assert.Equal(1000, acc.Balance);
            Assert.Equal(1000, acc.InitialBalance);
        }

        [Fact]
        public void Account_CreditAndDebit_WorkCorrectly()
        {
            var acc = new Account("Test", 1000);
            acc.Credit(200);
            Assert.Equal(1200, acc.Balance);
            acc.Debit(300);
            Assert.Equal(900, acc.Balance);
        }

        [Fact]
        public void Category_Creation_WorksCorrectly()
        {
            var cat = new Category(CategoryType.Income, "Salary");
            Assert.Equal("Salary", cat.Label);
            Assert.Equal(CategoryType.Income, cat.Type);
        }

        [Fact]
        public void Transaction_Creation_WorksCorrectly()
        {
            var acc = new Account("Test", 1000);
            var cat = new Category(CategoryType.Expense, "Food");
            var tran = new Transaction(TransactionType.Expense, acc.Id, 150, DateTime.Now, cat.Id, "Lunch");
            Assert.Equal(150, tran.Amount);
            Assert.Equal(TransactionType.Expense, tran.Type);
        }


        [Fact]
        public void ValService_IsAmountValid_ReturnsTrueForPositive()
        {
            var valSvc = new ValService();
            Assert.True(valSvc.IsAmountValid(50));
        }

        [Fact]
        public void ValService_IsAmountValid_ReturnsFalseForNegative()
        {
            var valSvc = new ValService();
            Assert.False(valSvc.IsAmountValid(-10));
        }


        [Fact]
        public void TransactionFactory_CreatesTransaction_WhenAmountValid()
        {
            var valSvc = new ValService();
            var factory = new TransactionFactory(valSvc);
            var acc = new Account("Acc", 1000);
            var cat = new Category(CategoryType.Income, "Bonus");
            var tran = factory.CreateTransaction(TransactionType.Income, acc.Id, 250, DateTime.Now, cat.Id, "Monthly bonus");
            Assert.Equal(250, tran.Amount);
            Assert.Equal(TransactionType.Income, tran.Type);
        }

        [Fact]
        public void TransactionFactory_ThrowsException_WhenAmountInvalid()
        {
            var valSvc = new ValService();
            var factory = new TransactionFactory(valSvc);
            var acc = new Account("Acc", 1000);
            var cat = new Category(CategoryType.Expense, "Food");
            Assert.Throws<ArgumentException>(() => factory.CreateTransaction(TransactionType.Expense, acc.Id, -50, DateTime.Now, cat.Id, "Invalid"));
        }


        [Fact]
        public void AccountManager_CreateAccount_AddsToStore()
        {
            var store = new DataStore();
            var mgr = new AccountManager(store);
            var acc = mgr.CreateAccount("TestAcc", 1000);
            Assert.Contains(store.Accts, x => x.Id == acc.Id);
        }

        [Fact]
        public void AccountManager_UpdateAccount_ChangesName()
        {
            var store = new DataStore();
            var mgr = new AccountManager(store);
            var acc = mgr.CreateAccount("OldName", 1000);
            bool updated = mgr.UpdateAccount(acc.Id, "NewName");
            Assert.True(updated);
            Assert.Equal("NewName", acc.Name);
        }

        [Fact]
        public void AccountManager_DeleteAccount_RemovesFromStore()
        {
            var store = new DataStore();
            var mgr = new AccountManager(store);
            var acc = mgr.CreateAccount("TestAcc", 1000);
            bool deleted = mgr.DeleteAccount(acc.Id);
            Assert.True(deleted);
            Assert.DoesNotContain(store.Accts, x => x.Id == acc.Id);
        }


        [Fact]
        public void CategoryManager_CreateCategory_AddsToStore()
        {
            var store = new DataStore();
            var mgr = new CategoryManager(store);
            var cat = mgr.CreateCategory(CategoryType.Expense, "Food");
            Assert.Contains(store.Cats, x => x.Id == cat.Id);
        }

        [Fact]
        public void CategoryManager_UpdateCategory_ChangesProperties()
        {
            var store = new DataStore();
            var mgr = new CategoryManager(store);
            var cat = mgr.CreateCategory(CategoryType.Income, "Salary");
            bool updated = mgr.UpdateCategory(cat.Id, CategoryType.Expense, "Groceries");
            Assert.True(updated);
            Assert.Equal(CategoryType.Expense, cat.Type);
            Assert.Equal("Groceries", cat.Label);
        }

        [Fact]
        public void CategoryManager_DeleteCategory_RemovesFromStore()
        {
            var store = new DataStore();
            var mgr = new CategoryManager(store);
            var cat = mgr.CreateCategory(CategoryType.Income, "Salary");
            bool deleted = mgr.DeleteCategory(cat.Id);
            Assert.True(deleted);
            Assert.DoesNotContain(store.Cats, x => x.Id == cat.Id);
        }


        [Fact]
        public void TransactionManager_AddTransaction_AddsToStore()
        {
            var store = new DataStore();
            var mgr = new TransactionManager(store);
            var acc = new Account("Acc", 1000);
            var cat = new Category(CategoryType.Expense, "Food");
            var tran = new Transaction(TransactionType.Expense, acc.Id, 150, DateTime.Now, cat.Id, "Lunch");
            mgr.AddTransaction(tran);
            Assert.Contains(store.Trans, x => x.Id == tran.Id);
        }

        [Fact]
        public void TransactionManager_UpdateTransaction_ChangesData()
        {
            var store = new DataStore();
            var mgr = new TransactionManager(store);
            var acc = new Account("Acc", 1000);
            var cat = new Category(CategoryType.Income, "Bonus");
            var tran = new Transaction(TransactionType.Income, acc.Id, 300, DateTime.Now, cat.Id, "Bonus");
            mgr.AddTransaction(tran);
            var updatedTran = new Transaction(TransactionType.Income, acc.Id, 400, DateTime.Now, cat.Id, "Updated Bonus");
            typeof(Transaction).GetProperty("Id").SetValue(updatedTran, tran.Id);
            bool updated = mgr.UpdateTransaction(updatedTran);
            Assert.True(updated);
            var tFromStore = mgr.GetTransactionById(tran.Id);
            Assert.NotNull(tFromStore);
            Assert.Equal(400, tFromStore!.Amount);
        }

        [Fact]
        public void TransactionManager_DeleteTransaction_RemovesFromStore()
        {
            var store = new DataStore();
            var mgr = new TransactionManager(store);
            var acc = new Account("Acc", 1000);
            var cat = new Category(CategoryType.Expense, "Food");
            var tran = new Transaction(TransactionType.Expense, acc.Id, 150, DateTime.Now, cat.Id, "Lunch");
            mgr.AddTransaction(tran);
            bool deleted = mgr.DeleteTransaction(tran.Id);
            Assert.True(deleted);
            Assert.DoesNotContain(store.Trans, x => x.Id == tran.Id);
        }


        [Fact]
        public void Analytics_CalculateNet_ReturnsCorrectValue()
        {
            var store = new DataStore();
            var analytics = new Analytics(store);
            var acc = new Account("Acc", 1000);
            var cat = new Category(CategoryType.Income, "Salary");
            store.Trans.Add(new Transaction(TransactionType.Income, acc.Id, 500, new DateTime(2023, 1, 10), cat.Id, "Income"));
            store.Trans.Add(new Transaction(TransactionType.Expense, acc.Id, 200, new DateTime(2023, 1, 15), cat.Id, "Expense"));
            store.Trans.Add(new Transaction(TransactionType.Income, acc.Id, 300, new DateTime(2023, 2, 10), cat.Id, "Income"));
            var net = analytics.CalculateNet(new DateTime(2023, 1, 1), new DateTime(2023, 1, 31));
            Assert.Equal(300, net);
        }

        [Fact]
        public void Analytics_GroupByCategory_ReturnsCorrectGrouping()
        {
            var store = new DataStore();
            var analytics = new Analytics(store);
            var acc = new Account("Acc", 1000);
            var cat1 = new Category(CategoryType.Income, "Salary");
            var cat2 = new Category(CategoryType.Expense, "Food");
            store.Cats.Add(cat1);
            store.Cats.Add(cat2);
            store.Trans.Add(new Transaction(TransactionType.Income, acc.Id, 500, DateTime.Now, cat1.Id, "Income"));
            store.Trans.Add(new Transaction(TransactionType.Expense, acc.Id, 200, DateTime.Now, cat2.Id, "Expense"));
            store.Trans.Add(new Transaction(TransactionType.Income, acc.Id, 300, DateTime.Now, cat1.Id, "Income"));
            var groups = analytics.GroupByCategory();
            Assert.True(groups.TryGetValue("Salary", out var sum1));
            Assert.Equal(800, sum1);
            Assert.True(groups.TryGetValue("Food", out var sum2));
            Assert.Equal(-200, sum2);
        }


        [Fact]
        public void FinanceManager_RecalculateBalance_ComputesCorrectly()
        {
            var store = new DataStore();
            var proxy = new DataProxy(store);
            var accMgr = new AccountManager(store);
            var catMgr = new CategoryManager(store);
            var tranMgr = new TransactionManager(store);
            var analytics = new Analytics(store);
            var valSvc = new ValService();
            var tranFactory = new TransactionFactory(valSvc);
            var finMgr = new FinanceManager(accMgr, catMgr, tranMgr, analytics, proxy, tranFactory);

            var a = finMgr.AddAccount("FinTest", 1000);
            var c = finMgr.AddCategory(CategoryType.Income, "Salary");
            finMgr.AddTransaction(TransactionType.Income, a.Id, 500, DateTime.Now, c.Id, "Income");
            var newBal = finMgr.RecalculateBalance(a.Id);
            Assert.Equal(1500, newBal);
        }


        [Fact]
        public void JsonImporter_And_JsonExp_WorkTogether()
        {
            var testData = new ImportData
            {
                Accts = { new Account("TestAcc", 1000) },
                Cats = { new Category(CategoryType.Expense, "Food") },
                Trans = { new Transaction(TransactionType.Expense, Guid.NewGuid(), 200, DateTime.Now, Guid.NewGuid(), "Lunch") }
            };
            string json = JsonSerializer.Serialize(testData, new JsonSerializerOptions { WriteIndented = true });

            string path = "testdata.json";
            File.WriteAllText(path, json);

            var store = new DataStore();
            var importer = new JsonImporter(store);
            importer.Import(path);
            Assert.True(store.Accts.Count > 0);
            File.Delete(path);

            var exp = new JsonExp();
            var exporter = new kr1.svc.Exporter(store);
            exporter.Export(exp);
            string exportedJson = exp.GetJson();
            Assert.Contains("TestAcc", exportedJson);
        }

        [Fact]
        public void CsvImporter_And_CsvExp_WorkTogether()
        {
            string csvData = @"
                            [Accts]
                            TestAcc,1000
                            [Cats]
                            Expense,Food
                            [Trans]
                            Expense,00000000-0000-0000-0000-000000000001,200,2023-01-01,Lunch,00000000-0000-0000-0000-000000000002
                            ";
            string path = "testdata.csv";
            File.WriteAllText(path, csvData.Trim());
            var store = new DataStore();
            var importer = new CsvImporter(store);
            importer.Import(path);
            Assert.True(store.Accts.Count == 1);
            File.Delete(path);

            var csvExp = new CsvExp();
            var exporter = new kr1.svc.Exporter(store);
            exporter.Export(csvExp);
            string outCsv = csvExp.GetCsv();
            Assert.Contains("TestAcc", outCsv);
        }

        [Fact]
        public void AddAccountAction_WithTimeDecorator_WorksCorrectly()
        {
            var store = new DataStore();
            var mgr = new AccountManager(store);
            int initialCount = store.Accts.Count;
            IAction action = new AddAccountAction(mgr, "CmdAcc", 750);
            IAction timed = new TimeDecorator(action);
            timed.Execute();
            Assert.Equal(initialCount + 1, store.Accts.Count);
        }
    }
}
