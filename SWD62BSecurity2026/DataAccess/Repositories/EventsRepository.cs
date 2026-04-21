using DataAccess.Context;
using Domain.CustomExceptions;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DataAccess.Repositories
{
    public class EventsRepository
    {
        private readonly IDbContextFactory<DefaultUserTicketDbContext> _defaultContextFactory;
        private readonly IDbContextFactory<LeastPrivilegedUserTicketDbContext> _leastPrivilegedUserContextFactory;

        public EventsRepository(
            IDbContextFactory<DefaultUserTicketDbContext> defaultContextFactory,
            IDbContextFactory<LeastPrivilegedUserTicketDbContext> leastPrivilegedUserContextFactory)
        {
            _defaultContextFactory = defaultContextFactory;
            _leastPrivilegedUserContextFactory = leastPrivilegedUserContextFactory;
        }

        /* This is a method we must ensure can be run by the lesser privileged database login.
         * This ensures that if an SQL Injection attack is successfully, the damage is limited as the attacker will be using a lesser privileged login where DELETE or CREATE are restricted.
         * limit damage especially if this is run by an anonymous user) */
        public List<Event> GetAllEvents()
        {
            using LeastPrivilegedUserTicketDbContext? leastPrivilegedUser = this._leastPrivilegedUserContextFactory.CreateDbContext();
            /* Querying or any command which is run on the database henceforth will be running with the least privileged login credentials
             * which is a good security practice to limit the damage of an SQL Injection attack. */
            return leastPrivilegedUser.Events.AsNoTracking().ToList();
        }

        public void CreateEvent(Event newEvent)
        {
            using LeastPrivilegedUserTicketDbContext? leastPrivilegedUser = this._leastPrivilegedUserContextFactory.CreateDbContext();
            if (leastPrivilegedUser.Events.Any(@event => @event.Name.Equals(newEvent.Name)))
            {
                throw new DuplicateEventEntryException("Event name already exists!");
            }

            using DefaultUserTicketDbContext? defaultContext = this._defaultContextFactory.CreateDbContext();
            defaultContext.Events.Add(newEvent);
            defaultContext.SaveChanges();
        }

        public void DeleteEvent(int id)
        {
            using DefaultUserTicketDbContext? defaultContext = this._defaultContextFactory.CreateDbContext();
            Event? eventToDelete = defaultContext.Events.FirstOrDefault(@event => @event.Id == id);

            if(eventToDelete != null)
            {
                defaultContext.Events.Remove(eventToDelete);
                defaultContext.SaveChanges();
            }
        }

        public void Checkout(List<Ticket> tickets, int eventId)
        {
            using DefaultUserTicketDbContext? defaultContext = this._defaultContextFactory.CreateDbContext();
            IDbContextTransaction? transaction = defaultContext.Database.BeginTransaction();

            try
            {
                Event? @event = defaultContext.Events.FirstOrDefault(e => e.Id == eventId);
                if (@event.MaximumTickets >= tickets.Sum(ticket => ticket.Quantity))
                {
                    foreach (Ticket ticket in tickets)
                    {
                        defaultContext.Tickets.Add(ticket);
                    }
                }

                defaultContext.SaveChanges();
                transaction.Commit(); //Confirms intention to permanently store the tickets in the database.
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
