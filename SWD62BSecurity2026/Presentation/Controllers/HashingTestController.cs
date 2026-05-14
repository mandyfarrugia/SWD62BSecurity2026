using Microsoft.AspNetCore.Mvc;
using Presentation.Helpers.Cryptography.AsymmetricEncryption;
using Presentation.Helpers.Cryptography.DigitalSigning;
using Presentation.Helpers.Cryptography.Hashing;
using Presentation.Helpers.Cryptography.HybridEncryption;
using Presentation.Helpers.Cryptography.SymmetricEncryption;
using System.Security.Cryptography;

namespace Presentation.Controllers
{
    public class HashingTestController : Controller
    {

        public IActionResult TestAsymmetric()
        {
            string plainText = "Hello world";
            AsymmetricParameters asymmetricParameters = AsymmetricEncryptionHelper.GenerateKeys();
            string cipherText = AsymmetricEncryptionHelper.Encrypt(plainText, asymmetricParameters.PublicKey);
            string decryptedText = AsymmetricEncryptionHelper.Decrypt(cipherText, asymmetricParameters.PrivateKey);
            return Content($"Plain Text: {plainText}\nCipher Text: {cipherText}\nDecrypted Text: {decryptedText}");
        }

        public IActionResult TestHybridEncryption()
        {
            AsymmetricParameters asymmetricParameters = AsymmetricEncryptionHelper.GenerateKeys();
            string plainText = "Hello world";
            using MemoryStream cipher = HybridEncryptionHelper.Encrypt(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(plainText)), asymmetricParameters.PublicKey);
            using MemoryStream output = HybridEncryptionHelper.Decrypt(cipher, asymmetricParameters.PrivateKey);
            string encryptedText = Convert.ToBase64String(cipher.ToArray());
            string decryptedText = System.Text.Encoding.UTF8.GetString(output.ToArray());
            return Content($"Plain Text: {plainText}\nCipher Text: {encryptedText}\nDecrypted Text: {decryptedText}");
        }

        public IActionResult TestSymmetric()
        {
            string plainText = "Hello world";
            Aes aesAlgorithm = Aes.Create();
            SymmetricParameters keys = SymmetricEncryptionHelper.GenerateSymmetricParameters(aesAlgorithm);
            string cipherText = SymmetricEncryptionHelper.Encrypt(plainText, keys, aesAlgorithm);
            string decryptedText = SymmetricEncryptionHelper.Decrypt(cipherText, keys, aesAlgorithm);
            return Content($"Plain Text: {plainText}\nCipher Text: {cipherText}\nDecrypted Text: {decryptedText}");
        }

        public IActionResult TestEnhancedSymmetric()
        {
            string plainText = "Hello world";
            Aes aesAlgorithm = Aes.Create();
            SymmetricParameters symmetricParameters = EnhancedSymmetricEncryptionHelper.GetSymmetricParameters(aesAlgorithm);
            MemoryStream cipher = EnhancedSymmetricEncryptionHelper.Encrypt(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(plainText)), symmetricParameters, aesAlgorithm);
            MemoryStream output = EnhancedSymmetricEncryptionHelper.Decrypt(cipher, symmetricParameters, aesAlgorithm);
            string encryptedText = Convert.ToBase64String(cipher.ToArray());
            string decryptedText = System.Text.Encoding.UTF8.GetString(output.ToArray());
            return Content($"Plain Text: {plainText}\nCipher Text: {encryptedText}\nDecrypted Text: {decryptedText}");
        }

        public IActionResult TestEnhancedHybridEncryption()
        {
            string plainText = "Hello world";
            Aes aesAlgorithm = Aes.Create();
            AsymmetricParameters asymmetricParameters = AsymmetricEncryptionHelper.GenerateKeys();
            MemoryStream cipher = EnhancedHybridEncryptionHelper.Encrypt(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(plainText)), asymmetricParameters.PublicKey);
            MemoryStream output = EnhancedHybridEncryptionHelper.Decrypt(cipher, asymmetricParameters.PrivateKey);
            string encryptedText = Convert.ToBase64String(cipher.ToArray());
            string decryptedText = System.Text.Encoding.UTF8.GetString(output.ToArray());
            return Content($"Plain Text: {plainText}\nCipher Text: {encryptedText}\nDecrypted Text: {decryptedText}");
        }

        public IActionResult TestDigitalSigning([FromServices] IWebHostEnvironment host)
        {
            AsymmetricParameters asymmetricParameters = AsymmetricEncryptionHelper.GenerateKeys();
            MemoryStream fileData = new MemoryStream();
            string file = $@"{host.ContentRootPath}\uploads\demo1.txt";

            using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                fileStream.CopyTo(fileData);
            }

            fileData.Position = 0;

            string signature = DigitalSigningHelperV1.DigitallySignData(fileData, asymmetricParameters.PrivateKey);

            //System.IO.File.AppendAllText(file, "Oops, it seems like I tampered with the file..."); //<- Use this line of code to tamper with a text file.

            MemoryStream fileData2 = new MemoryStream();
            using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                fileStream.CopyTo(fileData2);
            }

            fileData2.Position = 0;

            bool isOriginal = DigitalSigningHelperV1.VerifyDigitalSignature(fileData2, signature, asymmetricParameters.PublicKey);
            return Content($"File: {file}\nSignature: {signature}\nVerification Result: {isOriginal}");
        }

        public IActionResult HashWorksheet()
        {
            //Never implement your own cryptographic/hashing algorithm.

            HashingHelper hashingHelper = new HashingHelper();
            string digest1 = hashingHelper.Hash("Transfer 250 EUR to account 458923");
            string digest2 = hashingHelper.Hash("Transfer 2500 EUR to account 458923");
            string digest3 = hashingHelper.Hash("Transfer 250 EUR to account 458923.");

            bool decision1 = digest1.Equals("b4Ff8n/2dWtxSGl/X8qzxF/DwfFabzEVmo23SftOw01RZNmkGQMUwCqCyb0cToKyjIEVNsoI0sWZqrUHsIg60A==");
            bool decision2 = digest2.Equals("XEWsgeU/mmxf5URp9hm4+Ae0IrH1MRMu0nwGZ3Kb4JaQfVUdXeZIxfiPtvxRv0Kp4Vm98DLBpx3wrGpNgmeTPg==");
            bool decision3 = digest3.Equals("fpMvTBCMXiaaCs6cKNvxZRcDwYIgsdjIZ8LEns7eiC3fPhaMmdbeJHtQ8nb0WNFmjGcuZd2o8GBdaV3egqD8mQ==");

            return Content($"Message A is {(decision1 ? "Original" : "Tampered")}, Message B is {(decision2 ? "Original" : "Tampered")}, Message C is {(decision3 ? "Original" : "Tampered")}");
        }

        public IActionResult TestWeakAlgorithm()
        {
            string input = "SECURITY";
            int sum = 0;

            foreach(char character in input)
            {
                sum += character;
            }

            int weakHash = sum % 256;

            int sum2 = 0;
            string input2 = "YTRUICES";

            foreach (char character in input)
            {
                sum2 += character;
            }

            int weakHash2 = sum % 256;

            HashingHelper hashingHelper = new HashingHelper();
            string? digest1 = hashingHelper.Hash(input);
            string? digest2 = hashingHelper.Hash(input2);

            return Content($"Weak Hash 1: {weakHash}, Weak Hash 2: {weakHash2}\nSHA-512 Digest 1: {digest1}\nSHA-512 Digest 2: {digest2}");
        }
    }
}
