using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;

namespace Presentation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            this._logger.Log(LogLevel.Information, "Went into Home/Index method.");

            try
            {
                throw new Exception("Demo exception.");
            }
            catch(Exception exception)
            {
                this._logger.LogError(exception, "An error occurred in Home/Index");
            }

            this._logger.LogWarning("This is a warning message post error generation.");
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //Status code will be passed automatically during the runtime.
        public IActionResult StatusError(int code) 
        {
            switch (code)
            {
                case 404:
                    return Content("Page found - URL type is not correct!");
            }

            return Content("Unhandled error occurred, we logged it. Please try again later!");
        }
    }
}
