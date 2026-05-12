using Microsoft.EntityFrameworkCore;

namespace DataAccess.Context
{
    public class TicketDbContext : AbstractTicketDbContext
    {
        public TicketDbContext(DbContextOptions<TicketDbContext> options) : base(options) {}
    }
}
