using System;
using OOAD_Project.Domain;
using OOAD_Project.Patterns.Repository;

namespace OOAD_Project.Patterns.Command
{
    public class AddCategoryCommand : ICommand
    {
        private readonly Category _category;
        private readonly IRepository<Category> _repository;
        private int _insertedId = -1;

        public AddCategoryCommand(Category category, IRepository<Category> repository)
        {
            _category = category ?? throw new ArgumentNullException(nameof(category));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public void Execute()
        {
            try
            {
                _insertedId = _repository.Add(_category);
                _category.CategoryId = _insertedId;
                Console.WriteLine($"[AddCategoryCommand] Added category '{_category.CategoryName}' with ID {_insertedId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddCategoryCommand] Error executing: {ex.Message}");
                throw;
            }
        }

        public void Undo()
        {
            if (_insertedId > 0)
            {
                try
                {
                    _repository.Delete(_insertedId);
                    Console.WriteLine($"[AddCategoryCommand] Undid addition of category ID {_insertedId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AddCategoryCommand] Error undoing: {ex.Message}");
                    throw;
                }
            }
        }

        public string GetDescription()
        {
            return $"Add Category: {_category.CategoryName}";
        }
    }

    public class UpdateCategoryCommand : ICommand
    {
        private readonly Category _newCategory;
        private readonly IRepository<Category> _repository;
        private Category _oldCategory;

        public UpdateCategoryCommand(Category newCategory, IRepository<Category> repository)
        {
            _newCategory = newCategory ?? throw new ArgumentNullException(nameof(newCategory));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public void Execute()
        {
            try
            {
                _oldCategory = _repository.GetById(_newCategory.CategoryId);

                if (_oldCategory == null)
                {
                    throw new InvalidOperationException($"Category with ID {_newCategory.CategoryId} not found");
                }

                _repository.Update(_newCategory);
                Console.WriteLine($"[UpdateCategoryCommand] Updated category ID {_newCategory.CategoryId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateCategoryCommand] Error executing: {ex.Message}");
                throw;
            }
        }

        public void Undo()
        {
            if (_oldCategory != null)
            {
                try
                {
                    _repository.Update(_oldCategory);
                    Console.WriteLine($"[UpdateCategoryCommand] Restored category ID {_oldCategory.CategoryId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[UpdateCategoryCommand] Error undoing: {ex.Message}");
                    throw;
                }
            }
        }

        public string GetDescription()
        {
            return $"Update Category: {_newCategory.CategoryName}";
        }
    }

    public class DeleteCategoryCommand : ICommand
    {
        private readonly int _categoryId;
        private readonly IRepository<Category> _repository;
        private Category _deletedCategory;

        public DeleteCategoryCommand(int categoryId, IRepository<Category> repository)
        {
            _categoryId = categoryId;
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public void Execute()
        {
            try
            {
                _deletedCategory = _repository.GetById(_categoryId);

                if (_deletedCategory == null)
                {
                    throw new InvalidOperationException($"Category with ID {_categoryId} not found");
                }

                _repository.Delete(_categoryId);
                Console.WriteLine($"[DeleteCategoryCommand] Deleted category '{_deletedCategory.CategoryName}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteCategoryCommand] Error executing: {ex.Message}");
                throw;
            }
        }

        public void Undo()
        {
            if (_deletedCategory != null)
            {
                try
                {
                    _repository.Add(_deletedCategory);
                    Console.WriteLine($"[DeleteCategoryCommand] Restored category '{_deletedCategory.CategoryName}'");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DeleteCategoryCommand] Error undoing: {ex.Message}");
                    throw;
                }
            }
        }

        public string GetDescription()
        {
            return _deletedCategory != null
                ? $"Delete Category: {_deletedCategory.CategoryName}"
                : $"Delete Category ID: {_categoryId}";
        }
    }
}