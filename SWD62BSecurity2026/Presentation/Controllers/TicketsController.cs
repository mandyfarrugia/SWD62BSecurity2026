using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models.ViewModels;

namespace Presentation.Controllers
{
    public class TicketsController : Controller
    {
        [HttpGet]
        public IActionResult Buy(int id)
        {
            CreateTicketViewModel createTicketViewModel = new CreateTicketViewModel();
            createTicketViewModel.EventFK = id;

            /* A check to return the event's data, as well as ticket's data for the event 
             * and check whether the purchased tickets exceed the maximum tickets for the event, 
             * in which case you should not allow the user to continue buying tickets because it is sold out. */

            return View(createTicketViewModel);
        }

        [HttpPost]
        public IActionResult Buy(CreateTicketViewModel createTicketViewModel)
        {
            if(!ModelState.IsValid)
                return View(createTicketViewModel);

            Ticket ticket = new Ticket
            {
                EventFK = createTicketViewModel.EventFK,
                Name = createTicketViewModel.Name,
                Surname = createTicketViewModel.Surname,
                IdCard = createTicketViewModel.IdCard,
                Quantity = createTicketViewModel.Quantity,
                Status = Status.Paid
            };

            //Save the ticket to the database.
            return View(createTicketViewModel);
        }
    }
}
