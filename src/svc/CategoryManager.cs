using System;
using System.Collections.Generic;
using kr1.core;
using kr1.store;
using kr1.util;

namespace kr1.svc
{
    public class CategoryManager
    {
        private readonly DataStore _store;
        public CategoryManager(DataStore store)
        {
            _store = store;
        }

        public Category CreateCategory(CategoryType type, string label)
        {
            var c = Creator.NewCategory(type, label);
            _store.Cats.Add(c);
            return c;
        }

        public IEnumerable<Category> GetCategories() => _store.Cats;

        public Category? GetCategoryById(Guid id) => _store.FindCategory(id);

        public bool UpdateCategory(Guid id, CategoryType newType, string newLabel)
        {
            var c = GetCategoryById(id);
            if (c == null) {
                return false;
            }
            if (string.IsNullOrWhiteSpace(newLabel)) {
                throw new ArgumentException("Название не может быть пустым.", nameof(newLabel));
            }
            c.Type = newType;
            c.Label = newLabel;
            return true;
        }

        public bool DeleteCategory(Guid id)
        {
            var c = GetCategoryById(id);
            if (c == null) {
                return false;
            }
            return _store.Cats.Remove(c);
        }
    }
}
