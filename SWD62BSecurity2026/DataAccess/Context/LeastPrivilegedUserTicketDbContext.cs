using Microsoft.EntityFrameworkCore;

namespace DataAccess.Context
{
    public class LeastPrivilegedUserTicketDbContext : AbstractTicketDbContext
    {
        public LeastPrivilegedUserTicketDbContext(DbContextOptions<LeastPrivilegedUserTicketDbContext> options) : base(options) {}
    }
}