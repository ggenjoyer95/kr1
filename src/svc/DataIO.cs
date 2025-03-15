using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using kr1.core;
using kr1.store;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace kr1.svc
{
    public abstract class DataImporter
    {
        protected DataStore Store;
        public DataImporter(DataStore store)
        {
            Store = store;
        }
        public void Import(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Файл не найден", path);
            string content = File.ReadAllText(path);
            var data = Parse(content);
            Load(data);
        }
        protected abstract ImportData Parse(string content);
        protected virtual void Load(ImportData data)
        {
            Store.Accts.AddRange(data.Accts);
            Store.Cats.AddRange(data.Cats);
            Store.Trans.AddRange(data.Trans);
        }
    }

    public class ImportData
    {
        public List<Account> Accts { get; set; } = new List<Account>();
        public List<Category> Cats { get; set; } = new List<Category>();
        public List<Transaction> Trans { get; set; } = new List<Transaction>();
    }

    public class JsonImporter : DataImporter
    {
        public JsonImporter(DataStore store) : base(store) { }
        protected override ImportData Parse(string content)
        {
            try
            {
                var data = JsonSerializer.Deserialize<ImportData>(content);
                if (data == null)
                    throw new Exception("JSON десериализация не удалась");
                return data;
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка парсинга JSON: " + ex.Message);
            }
        }
    }

    public class CsvImporter : DataImporter
    {
        public CsvImporter(DataStore store) : base(store) { }
        protected override ImportData Parse(string content)
        {
            var data = new ImportData();
            using (var reader = new StringReader(content))
            {
                string? line;
                string section = "";
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line)) {
                        continue;
                    }
                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        section = line.Trim('[', ']');
                        continue;
                    }
                    var parts = line.Split(',');
                    switch (section)
                    {
                        case "Accts":
                            if (parts.Length >= 2)
                            {
                                string name = parts[0];
                                decimal initBal = decimal.Parse(parts[1]);
                                var a = new Account(name, initBal);
                                data.Accts.Add(a);
                            }
                            break;
                        case "Cats":
                            if (parts.Length >= 2)
                            {
                                if (!Enum.TryParse<CategoryType>(parts[0], true, out var type))
                                    type = CategoryType.Expense;
                                string label = parts[1];
                                var cat = new Category(type, label);
                                data.Cats.Add(cat);
                            }
                            break;
                        case "Trans":
                            if (parts.Length >= 6)
                            {
                                if (!Enum.TryParse<TransactionType>(parts[0], true, out var tt))
                                    tt = TransactionType.Expense;
                                Guid acctId = Guid.Parse(parts[1]);
                                decimal amt = decimal.Parse(parts[2]);
                                DateTime dt = DateTime.Parse(parts[3]);
                                string note = parts[4];
                                Guid catId = Guid.Parse(parts[5]);
                                var t = new Transaction(tt, acctId, amt, dt, catId, note);
                                data.Trans.Add(t);
                            }
                            break;
                    }
                }
            }
            return data;
        }
    }

    public class YamlImporter : DataImporter
    {
        public YamlImporter(DataStore store) : base(store) { }
        protected override ImportData Parse(string content)
        {
            try
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                var data = deserializer.Deserialize<ImportData>(content);
                if (data == null)
                    throw new Exception("YAML десериализация не удалась");
                return data;
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка парсинга YAML: " + ex.Message);
            }
        }
    }

    public interface IExpVisitor
    {
        void VisitAccount(Account a);
        void VisitCategory(Category c);
        void VisitTransaction(Transaction t);
    }

    public class JsonExp : IExpVisitor
    {
        private readonly List<Account> _accts = new List<Account>();
        private readonly List<Category> _cats = new List<Category>();
        private readonly List<Transaction> _trans = new List<Transaction>();

        public void VisitAccount(Account a) => _accts.Add(a);
        public void VisitCategory(Category c) => _cats.Add(c);
        public void VisitTransaction(Transaction t) => _trans.Add(t);

        public string GetJson()
        {
            var data = new ImportData { Accts = _accts, Cats = _cats, Trans = _trans };
            return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    public class CsvExp : IExpVisitor
    {
        private readonly StringBuilder _sb = new StringBuilder();
        public CsvExp()
        {
            _sb.AppendLine("[Accts]");
            _sb.AppendLine("name,initBal");
            _sb.AppendLine("[Cats]");
            _sb.AppendLine("type,label");
            _sb.AppendLine("[Trans]");
            _sb.AppendLine("type,acctId,amt,date,note,catId");
        }
        public void VisitAccount(Account a)
        {
            _sb.AppendLine($"{a.Name},{a.InitialBalance}");
        }
        public void VisitCategory(Category c)
        {
            _sb.AppendLine($"{c.Type},{c.Label}");
        }
        public void VisitTransaction(Transaction t)
        {
            _sb.AppendLine($"{t.Type},{t.AccountId},{t.Amount},{t.Date:yyyy-MM-dd},{t.Note},{t.CategoryId}");
        }
        public string GetCsv() => _sb.ToString();
    }

    public class YamlExp : IExpVisitor
    {
        private readonly ImportData _data = new ImportData();
        public void VisitAccount(Account a) => _data.Accts.Add(a);
        public void VisitCategory(Category c) => _data.Cats.Add(c);
        public void VisitTransaction(Transaction t) => _data.Trans.Add(t);
        public string GetYaml()
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            return serializer.Serialize(_data);
        }
    }

    public class Exporter
    {
        private readonly DataStore _store;
        public Exporter(DataStore store)
        {
            _store = store;
        }
        public void Export(IExpVisitor visitor)
        {
            foreach (var a in _store.Accts)
                visitor.VisitAccount(a);
            foreach (var c in _store.Cats)
                visitor.VisitCategory(c);
            foreach (var t in _store.Trans)
                visitor.VisitTransaction(t);
        }
    }
}
