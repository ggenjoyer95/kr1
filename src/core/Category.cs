using System;

namespace kr1.core
{
    public enum CategoryType
    {
        Income,
        Expense
    }

    public class Category
    {
        public Guid Id { get; private set; }
        public CategoryType Type { get; set; }
        public string Label { get; set; }

        public Category(CategoryType type, string label)
        {
            if (string.IsNullOrWhiteSpace(label)) {
                throw new ArgumentException("Название не может быть пустым.", nameof(label));
            }
            Id = Guid.NewGuid();
            Type = type;
            Label = label;
        }
    }
}
