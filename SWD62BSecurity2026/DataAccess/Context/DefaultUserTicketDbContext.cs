using Microsoft.EntityFrameworkCore;

namespace DataAccess.Context
{
    public class DefaultUserTicketDbContext : AbstractTicketDbContext
    {
        public DefaultUserTicketDbContext(DbContextOptions<DefaultUserTicketDbContext> options) : base(options) {}
    }
}