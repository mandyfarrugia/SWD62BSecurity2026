using DataAccess.Repositories;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Presentation.ActionFilters
{
    public class EventOrganiserPermissionFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            //Checking whether the user who is authenticated is the organiser of the event to be deleted, and if not, return a 403 Forbidden status code.
            string userEmail = context.HttpContext.User.Identity?.Name ?? string.Empty;
            int eventId = int.Parse(context.RouteData.Values["id"]?.ToString() ?? "0");

            //Query the database to get the event with the eventId and check whether the organiser of the event is the same as the userEmail, if not, return a 403 Forbidden status code.
            EventsRepository eventsRepository = context.HttpContext.RequestServices.GetRequiredService<EventsRepository>();

            Event? eventToDelete = eventsRepository.GetAllEvents().SingleOrDefault(@event => @event.Id == eventId);

            if (!eventToDelete.Organiser.Equals(userEmail))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}
