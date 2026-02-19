using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Context
{
    public class TicketDbContext : IdentityDbContext
    {
        public TicketDbContext(DbContextOptions<TicketDbContext> options) : base(options) {}

        public DbSet<Event> Events { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
    }
}
