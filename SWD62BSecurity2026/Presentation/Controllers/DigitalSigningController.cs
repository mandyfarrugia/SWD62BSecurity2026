using Microsoft.AspNetCore.Mvc;
using Presentation.Helpers.Cryptography.AsymmetricEncryption;
using Presentation.Helpers.Cryptography.DigitalSigning;

namespace Presentation.Controllers
{
    public class DigitalSigningController : Controller
    {
        public IActionResult TestDigitalSigningV2([FromServices] IWebHostEnvironment webHostEnvironment)
        {
            string pathToFile = Path.Combine(webHostEnvironment.ContentRootPath, "uploads", Path.GetFileName("demo1.txt"));
            AsymmetricParameters asymmetricParameters = AsymmetricEncryptionHelper.GenerateKeys();
            MemoryStream digitallySignedFileStream = new MemoryStream();

            using (FileStream fileStream = new FileStream(pathToFile, FileMode.Open, FileAccess.Read))
            {
                fileStream.CopyTo(digitallySignedFileStream);
            }

            string signature = DigitalSigningHelperV2.DigitallySignData(digitallySignedFileStream, asymmetricParameters.PrivateKey);
            bool isUntouched = DigitalSigningHelperV2.VerifyData(digitallySignedFileStream, signature, asymmetricParameters.PublicKey);
            return Content($"File Path: {pathToFile}\nSignature: {signature}\nIs the data untouched?: {isUntouched}");
        }

        public IActionResult TestDigitalSigningV3([FromServices] IWebHostEnvironment webHostEnvironment)
        {
            string pathToFile = Path.Combine(webHostEnvironment.ContentRootPath, "uploads", Path.GetFileName("demo1.txt"));
            AsymmetricParameters asymmetricParameters = AsymmetricEncryptionHelper.GeneratePemKeys();
            MemoryStream digitallySignedFileStream = new MemoryStream();

            using (FileStream fileStream = new FileStream(pathToFile, FileMode.Open, FileAccess.Read))
            {
                fileStream.CopyTo(digitallySignedFileStream);
            }

            string signature = DigitalSigningHelperV3.DigitallySignData(digitallySignedFileStream, asymmetricParameters.PrivateKey);
            bool isUntouched = DigitalSigningHelperV3.VerifyData(digitallySignedFileStream, signature, asymmetricParameters.PublicKey);
            return Content($"File Path: {pathToFile}\nSignature: {signature}\nIs the data untouched?: {isUntouched}");
        }
    }
}
