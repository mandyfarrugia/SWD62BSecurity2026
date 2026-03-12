using DataAccess.Context;
using Domain.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class EventsRepository
    {
        private readonly TicketDbContext _context;

        public EventsRepository(TicketDbContext context)
        {
            this._context = context;
        }

        public IQueryable<Event> GetAllEvents()
        {
            return this._context.Events;
        }

        public void CreateEvent(Event newEvent)
        {
            this._context.Events.Add(newEvent);
            this._context.SaveChanges();
        }
    }
}
