using DataAccess.Repositories;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Create(CreateEventViewModel createEventViewModel, IFormFile file, [FromServices] IWebHostEnvironment host)
        {
            if (file != null)
            {
                if (file.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("File", "File cannot exceed 5MB!");
                    return View(createEventViewModel);
                }

                string[] whiteListOfFileExtensions = new string[] { ".jpg", ".jpeg", ".png" };
                bool failedCheck = false;
                int counter = 0;

                do
                {
                    if (Path.GetExtension(file.FileName).ToLower() == whiteListOfFileExtensions[counter])
                    {
                        failedCheck = false;
                        break;
                    }
                    else
                    {
                        failedCheck = true;
                    }

                    counter++;
                }
                while (failedCheck && counter < whiteListOfFileExtensions.Length);

                if (failedCheck)
                {
                    ModelState.AddModelError("File", "Only .jpg, .jpeg, and .png files are allowed!");
                    return View(createEventViewModel);
                }

                byte[] jpgFileSignature = new byte[] { 255, 216 };
                byte[] pngFileSignature = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };

                bool jpgSignatureMatch = true;
                bool pngSignatureMatch = true;

                using (Stream fileStream = file.OpenReadStream())
                {
                    byte[] fileSignature = new byte[jpgFileSignature.Length];
                    fileStream.Read(fileSignature, 0, fileSignature.Length);

                    if (!fileSignature.SequenceEqual(jpgFileSignature))
                    {
                        jpgSignatureMatch = false;
                        //ModelState.AddModelError("File", "The file content does match the expected file signature.");
                        //return View(createEventViewModel);
                    }

                    fileStream.Position = 0;

                    byte[] fileSignature2 = new byte[pngFileSignature.Length];
                    fileStream.Read(fileSignature2, 0, fileSignature2.Length);

                    if (!fileSignature2.SequenceEqual(pngFileSignature))
                    {
                        pngSignatureMatch = false;
                        //ModelState.AddModelError("File", "The file content does match the expected file signature.");
                        //return View(createEventViewModel);
                    }
                }

                if (!jpgSignatureMatch && !pngSignatureMatch)
                {
                    ModelState.AddModelError("File", "The file content does match the expected file format from JPG and PNG.");
                    return View(createEventViewModel);
                }

                string uploadsFolder = string.Empty;
                if (createEventViewModel.Public)
                {
                    uploadsFolder = Path.Combine(host.WebRootPath, "uploads");
                }
                else
                {
                    uploadsFolder = Path.Combine(host.ContentRootPath, "uploads");
                }

                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                createEventViewModel.FilePath = "/uploads/" + uniqueFileName;
            }

            ModelState.Remove("FilePath");
            ModelState.Remove("Organiser");

            if (!ModelState.IsValid)
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
            return RedirectToAction(nameof(Index));
        }
    }
}