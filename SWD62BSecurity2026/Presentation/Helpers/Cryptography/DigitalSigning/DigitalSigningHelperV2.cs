using Presentation.Helpers.Cryptography.Hashing;
using System.Security.Cryptography;

namespace Presentation.Helpers.Cryptography.DigitalSigning
{
    public class DigitalSigningHelperV2
    {
        public static string DigitallySignData(MemoryStream dataToDigitallySign, string privateKey)
        {
            /* 1. Opt for defensive programming to protect against null instances of MemoryStream, 
             *    as well as private keys that are null, empty, or contain only whitespace. */
            if (dataToDigitallySign == null || dataToDigitallySign.Length == 0)
                throw new ArgumentException(nameof(dataToDigitallySign), "The data to be signed cannot be null or empty!");

            if (string.IsNullOrWhiteSpace(privateKey))
                throw new ArgumentNullException("The private key cannot be null, empty, or contain only whitespace!");

            /* 2. Set up the RSA algorithm which is essentially an asymmetric crytographic algorithm 
             *    using a public key for encryption, and a private key for decryption. 
             *    This secures the transmission of data over public networks, which more often than not are insecure. */
            using (RSA rsaAlgorithm = RSA.Create())
            {
                //3. Import the private key into the RSA algorithm.
                rsaAlgorithm.FromXmlString(privateKey);

                /* 4. Reset the position to 0 before hashing to prevent the hash from being computed 
                 *    starting from the current position to end of the stream, failure to do so may lead to omission of data.
                 *    This should be taken care of within the helper method rather than the point of invocation of the helper method
                 *    to ensure the reliability of the helper method. */
                dataToDigitallySign.Position = 0;

                /* 5. Use the hashing helper method which uses SHA512.
                 *    It is vital to use the same hashing algorithm used for the digital signature when verifying the data. */
                byte[] manuallyHashedDigest = new HashingHelper().Hash(dataToDigitallySign.ToArray()) ??
                    throw new InvalidOperationException("Something went wrong while hashing the data to be digitally signed!");

                /* 6. Pass the manually hashed digest to SignHash() associated with the RSA algorithm,
                 *    alongside the hashing algorithm (make sure to specify the one used to manually hash the data provided,
                 *    and the RSA signature padding (make sure to use the same one when verifying the data). */
                byte[] signature = rsaAlgorithm.SignHash(manuallyHashedDigest, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1)
                    ?? throw new InvalidOperationException("Something went wrong while computing the signature!");

                //7. Convert the signature from an array of bytes to a Base64 string.
                return Convert.ToBase64String(signature);
            }
        }

        public static bool VerifyData(MemoryStream dataToVerify, string signature, string publicKey)
        {
            /* 1. Opt for defensive programming to protect against null instances of MemoryStream, 
             *    as well as private keys that are null, empty, or contain only whitespace. */
            if (dataToVerify == null || dataToVerify.Length == 0)
                throw new ArgumentException(nameof(dataToVerify), "The data to be verified cannot be null or empty!");

            if (string.IsNullOrWhiteSpace(publicKey))
                throw new ArgumentNullException("The public key cannot be null, empty, or contain only whitespace!");

            /* 2. Set up the RSA algorithm which is essentially an asymmetric crytographic algorithm 
             *    using a public key for encryption, and a private key for decryption. 
             *    This secures the transmission of data over public networks, which more often than not are insecure. */
            using (RSA rsaAlgorithm = RSA.Create())
            {
                //3. Import the public key into the RSA algorithm.
                rsaAlgorithm.FromXmlString(publicKey);

                /* 4. Reset the position to 0 before hashing to prevent the hash from being computed 
                 *    starting from the current position to end of the stream, failure to do so may lead to omission of data.
                 *    This should be taken care of within the helper method rather than the point of invocation of the helper method
                 *    to ensure the reliability of the helper method. */
                dataToVerify.Position = 0;

                /* 5. Generate a hashed value using the same hashing algorithm used to digitally sign the data and the signature padding.
                 *    Compare the hashed value with the given signature and return a boolean value indicating whether the data has been tampered with.
                 *    True denotes that the data is untouched, whereas false denotes that the data has been tampered with. */
                return rsaAlgorithm.VerifyData(dataToVerify, Convert.FromBase64String(signature), HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
            }
        }
    }
}