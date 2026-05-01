using Microsoft.AspNetCore.Mvc;
using Presentation.Helpers;

namespace Presentation.Controllers
{
    public class HashingTestController : Controller
    {
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
