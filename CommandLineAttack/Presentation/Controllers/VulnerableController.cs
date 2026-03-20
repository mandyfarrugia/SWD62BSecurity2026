using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Presentation.Controllers
{
    public class VulnerableController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Resize(string file)
        {
            string command = $"/c echo Starting conversion of {file} && echo Conversion finished";

            string[] whiteList = { "echo", "xcopy" };

            foreach(string s in file.Split(new char[] { ' ' }))
            {
                if (whiteList.Contains(s))
                    continue;
                else
                    return Content("Invalid injected commands.")
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = command,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = true,
                CreateNoWindow = false
            };

            using (Process? process = Process.Start(processStartInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                return Content("Output: " + output + "\nError: " + error);
            }
        }
    }
}
