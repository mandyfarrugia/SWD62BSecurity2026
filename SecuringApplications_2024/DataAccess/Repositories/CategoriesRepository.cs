using Common.Models;
using DataAccess.Context;

namespace DataAccess.Repositories
{
    public class CategoriesRepository
    {
        private LibraryDbContext _context;

        public CategoriesRepository(LibraryDbContext context)
        {
            this._context = context;
        }

        public IQueryable<Category> GetCategories()
        {
            return this._context.Categories;
        }

        public Category GetCategoryById(int id)
        {
            return this._context.Categories.FirstOrDefault(c => c.Id == id);
        }

        public void AddCategory(Category category)
        {
            this._context.Categories.Add(category);
            this._context.SaveChanges();
        }
    }
}
