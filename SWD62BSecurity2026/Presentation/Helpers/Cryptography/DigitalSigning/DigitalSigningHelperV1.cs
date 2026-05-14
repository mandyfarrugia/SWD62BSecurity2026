using System.Security.Cryptography;

namespace Presentation.Helpers.Cryptography.DigitalSigning
{
    public class DigitalSigningHelperV1
    {
        /* Use same asymmetric algorithm (for instance RSA), 
         * this avoids having to create a method which generates the asymmetric parameters
         * since the method from AsymmetricEncryptionHelper class can be reused. */

        //Dealing with the digital signature of files (although in reality, anything can be digitally signed, not just files).
        public static string DigitallySignData(MemoryStream dataToSign, string privateKey)
        {
            //1. Create the algorithm.
            RSA rsaAlgorithm = RSA.Create();

            //2. Import the private key.
            rsaAlgorithm.FromXmlString(privateKey);

            /* Reset the position to the beginning of the stream before hashing it. 
             * This is important because if the position is not reset,
             * the hash will be computed from the current position to the end of the stream,
             * which may not include all the data that needs to be signed. */
            dataToSign.Position = 0;

            //3. Hash the data.
            using(SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(dataToSign); //Use the same algorithm when verifiying the data.

                //4. Digitally sign the hashed data.
                byte[] signature = rsaAlgorithm.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1); //Indicate which algorithm was used to hash the data.
                return Convert.ToBase64String(signature);
            }
        }

        //Verify whether the data was modified or left in its original state whereby true denotes untouched, false denotes data was tampered with.
        public static bool VerifyDigitalSignature(MemoryStream dataToVerify, string signature, string publicKey)
        {
            //1. Create the algorithm.
            RSA rsaAlgorithm = RSA.Create();

            //2. Import the public key.
            rsaAlgorithm.FromXmlString(publicKey);

            /* Reset the position to the beginning of the stream before hashing it. 
             * This is important because if the position is not reset,
             * the hash will be computed from the current position to the end of the stream,
             * which may not include all the data that needs to be signed. */
            dataToVerify.Position = 0;

            //3. Hash the data to be verified.
            using(SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(dataToVerify); //Use the same algorithm when verifiying the data.

                //4. Digitally verify the hashed data.
                bool isUntouched = rsaAlgorithm.VerifyHash(hash, Convert.FromBase64String(signature), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1); //Indicate which algorithm was used to hash the data.
                return isUntouched;
            }
        }
    }
}
