using Common.Models;
using DataAccess.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class PermissionsRepository
    {
        private LibraryDbContext _context;

        public PermissionsRepository(LibraryDbContext context) 
        {
            this._context = context;
        }

        public void AddPermissions(Permission permission)
        {
            this._context.Add(permission);
            this._context.SaveChanges();
        }

        public void DeletePermissions(Permission permission)
        {
            this._context.Remove(permission);
            this._context.SaveChanges();
        }

        public IQueryable<Permission> GetPermissions(int bookId)
        {
            return this._context.Permissions.Where(permission => permission.BookIdFK == bookId);
        }
    }
}
