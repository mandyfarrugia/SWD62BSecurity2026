using DataAccess.Repositories;
using Domain.CustomExceptions;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.ActionFilters;
using Presentation.Models.ViewModels;

namespace Presentation.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventsRepository _eventsRepository;

        public EventsController(EventsRepository eventsRepository)
        {
            this._eventsRepository = eventsRepository;
        }

        public IActionResult Index()
        {
            List<EventListViewModel> eventListViewModel = this._eventsRepository
                .GetAllEvents()
                .Where(@event => @event.Public)
                .Select(@event => new EventListViewModel()
                    {
                        Id = @event.Id,
                        Name = @event.Name,
                        Price = @event.Price,
                        Public = @event.Public,
                        MaximumTickets = @event.MaximumTickets,
                        FilePath = @event.FilePath,
                    })
                .ToList();

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
        public IActionResult Create(CreateEventViewModel createEventViewModel, IFormFile file, [FromServices] IWebHostEnvironment host)
        {
            try
            {
                ModelState.Remove("FilePath");
                ModelState.Remove("Organiser");

                if (!ModelState.IsValid)
                {
                    return View(createEventViewModel);
                }

                if (file != null)
                {
                    if (file.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("File", "File cannot exceed 5MB!");
                        return View(createEventViewModel);
                    }

                    string[] whiteListOfFileExtensions = new string[] { ".jpg", ".jpeg", ".png" };
                    string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                    if (!whiteListOfFileExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("File", "Only .jpg, .jpeg, and .png files are allowed!");
                        return View(createEventViewModel);
                    }

                    ReadOnlySpan<byte> jpgFileSignature = stackalloc byte[] { 255, 216 };
                    ReadOnlySpan<byte> pngFileSignature = stackalloc byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };

                    bool jpgSignatureMatch;
                    bool pngSignatureMatch;

                    using (Stream fileStream = file.OpenReadStream())
                    {
                        Span<byte> fileSignature = new byte[8];
                        int bytesRead = fileStream.Read(fileSignature);

                        jpgSignatureMatch = bytesRead >= jpgFileSignature.Length && fileSignature[..jpgFileSignature.Length].SequenceEqual(jpgFileSignature);
                        pngSignatureMatch = bytesRead >= pngFileSignature.Length && fileSignature[..pngFileSignature.Length].SequenceEqual(pngFileSignature);

                        if (!jpgSignatureMatch && !pngSignatureMatch)
                        {
                            ModelState.AddModelError("File", "The file content does not match the expected file format from JPG and PNG.");
                            return View(createEventViewModel);
                        }
                    }

                    string uploadsFolder = createEventViewModel.Public ? Path.Combine(host.WebRootPath, "uploads") : Path.Combine(host.ContentRootPath, "uploads");

                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    createEventViewModel.FilePath = $"/uploads/{uniqueFileName}";
                }

                //Regex
                string regex = "^[A-Za-z0-9\\s]+$"; //User can input zero or more characters between a-z and A-Z including a space (each word must be capitalised), but not numbers or special characters.
                if (!System.Text.RegularExpressions.Regex.IsMatch(createEventViewModel.Name, regex))
                {
                    ModelState.AddModelError("Name", "Event name contains invalid characters!");
                    return View(createEventViewModel);
                }

                //To mitigate Cross-Site Scripting, forbid any input containing tags to prevent any injection of JavaScript code.
                if (createEventViewModel.Name.Contains("<") || createEventViewModel.Name.Contains(">"))
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
            }
            catch(DuplicateEventEntryException deee)
            {
                ModelState.AddModelError("Name", deee.Message);
                return View(createEventViewModel);
            }
            catch
            {
                //Log the exception details.
                ModelState.AddModelError("", "An error occurred while creating the event. Please try again.");
                return View(createEventViewModel);
            }
            
            return RedirectToAction(nameof(Index));
        }

        //An action filter is required in case you need to verify whether the user who is an organiser is actually the organiser of the event to be deleted, then you have to use and apply an authorisation filter.
        [HasEventOrganiserPermission]
        [Authorize(Roles = "organiser")] //Distinguish between an authenticated user and an anonymous user.
        public IActionResult Delete(int id)
        {
            this._eventsRepository.DeleteEvent(id);
            TempData["success"] = "Event deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return RedirectToAction(nameof(Index));

            List<EventListViewModel> eventListViewModels = this._eventsRepository
                .GetEventsByName(searchTerm)
                .Select(@event => new EventListViewModel()
                    {
                        Id = @event.Id,
                        Name = @event.Name,
                        Price = @event.Price,
                        Public = @event.Public,
                        FilePath = @event.FilePath,
                        MaximumTickets = @event.MaximumTickets
                }).ToList();

            ViewBag.SearchTerm = searchTerm;
            return View(nameof(Index), eventListViewModels);
        }
    }
}