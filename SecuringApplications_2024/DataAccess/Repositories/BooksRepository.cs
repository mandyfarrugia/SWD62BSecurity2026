using Common.Models;
using DataAccess.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class BooksRepository
    {
        private LibraryDbContext _context;

        public BooksRepository(LibraryDbContext context)
        {
            this._context = context;
        }

        public IQueryable<Book> GetAllBooks()
        {
            return this._context.Books;
        }

        public Book GetBook(int id)
        {
            return this._context.Books.SingleOrDefault(book => book.Id == id);
        }

        public void AddBook(Book book)
        {
            this._context.Add(book);
            this._context.SaveChanges();
        }

        public void DeleteBook(Book book)
        {
            this._context.Remove(book);
            this._context.SaveChanges();
        }

        public IQueryable<Permission> GetBookPermissions(int id)
        {
            return this._context.Permissions.Where(permission => permission.BookIdFK == id);
        }
    }
}