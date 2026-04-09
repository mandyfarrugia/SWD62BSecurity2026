using DataAccess.Context;
using Domain.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;

        public EventsRepository(TicketDbContext context, IConfiguration configuration)
        {
            this._context = context;
            this._configuration = configuration;
        }

        /* This is a method we must ensure can be run by the lesser privileged database login.
         * This ensures that if an SQL Injection attack is successfully, the damage is limited as the attacker will be using a lesser privileged login where DELETE or CREATE are restricted.
         * limit damage especially if this is run by an anonymous user) */
        public IQueryable<Event> GetAllEvents()
        {
            string? userConnectionString = this._configuration.GetConnectionString("UserConnection");
            this._context.Database.SetConnectionString(userConnectionString);

            /* Querying or any command which is run on the database henceforth will be running with the least privileged login credentials
             * which is a good security practice to limit the damage of an SQL Injection attack. */
            return this._context.Events;
        }

        public void CreateEvent(Event newEvent)
        {
            this._context.Events.Add(newEvent);
            this._context.SaveChanges();
        }
    }
}
