using Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Context
{
    public class LibraryDbContext : IdentityDbContext<CustomUser>
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Permission> Permissions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Category>()
                .HasIndex(category => category.Name)
                .IsUnique();

            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Fiction" },
                new Category { Id = 2, Name = "History" },
                new Category { Id = 3, Name = "Science" });

            builder.Entity<IdentityRole>().HasData(
               new IdentityRole { Id = "1", Name = "User", NormalizedName = "USER" },
               new IdentityRole { Id = "2", Name = "Librarian", NormalizedName = "LIBRARIAN" },
               new IdentityRole { Id = "3", Name = "Admin", NormalizedName = "ADMIN" });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }
    }
}
