using DataAccess.Repositories;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models.ViewModels;

namespace Presentation.Controllers
{
    public class EventController : Controller
    {
        private readonly EventsRepository _eventsRepository;

        public EventController(EventsRepository eventsRepository)
        {
            this._eventsRepository = eventsRepository;
        }

        public IActionResult Index()
        {
            List<EventListViewModel> eventListViewModel = this._eventsRepository.GetAllEvents().Where(e => e.Public).Select(e => new EventListViewModel()
            {
                Id = e.Id,
                Name = e.Name,
                Price = e.Price,
                Public = e.Public,
                MaximumTickets = e.MaximumTickets,
                FilePath = e.FilePath,
            }).ToList();

            return View(eventListViewModel);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            CreateEventViewModel createEventViewModel = new CreateEventViewModel();
            return View(createEventViewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken] //Generate a server-side nonce token and validate it with the token that has to be generated on the client-side as well.
        public IActionResult Create(CreateEventViewModel createEventViewModel)
        {
            if(!ModelState.IsValid)
            {
                return View(createEventViewModel);
            }

            //Regex
            //string regex = "^[A-Z][a-z]*$"; //User can input a character between a-z and A-Z, but not numbers or special characters.
            //if (!System.Text.RegularExpressions.Regex.IsMatch(newEvent.Name, regex))
            //{
            //    ModelState.AddModelError("Name", "Event name contains invalid characters!");
            //    return View(newEvent);
            //}

            if(createEventViewModel.Name.Contains("<") || createEventViewModel.Name.Contains(">"))
            {
                ModelState.AddModelError("Name", "Event name cannot contain HTML tags!");
                return View(createEventViewModel);
            }

            Event newEvent = new Event()
            {
                Name = createEventViewModel.Name,
                Price = createEventViewModel.Price,
                Public = createEventViewModel.Public,
                MaximumTickets = createEventViewModel.MaximumTickets,
                FilePath = createEventViewModel.FilePath,
                Organiser = User.Identity.Name
            };

            this._eventsRepository.CreateEvent(newEvent);
            TempData["success"] = "Event created successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
