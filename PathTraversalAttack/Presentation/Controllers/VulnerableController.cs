using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace Presentation.Controllers
{
    public class VulnerableController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetFile(string filename, [FromServices] IWebHostEnvironment host)
        {
            string absolutePath = Path.GetFullPath(Path.Combine(host.ContentRootPath, "uploads", filename));

            string regexPattern = "^[a-zA-Z0-9.]$";

            if (Regex.IsMatch(filename, regexPattern))
                return BadRequest("Filename contains invalid characters.");

            string relativePath = Path.Combine("uploads", Path.GetFileName(filename)); //Path.GetFilename() ignores the prefix ../.

            byte[] bytes = System.IO.File.ReadAllBytes(absolutePath);
            return File(bytes, "application/octet-stream", filename);
        }
    }
}
