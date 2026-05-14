using System.Security.Cryptography;

namespace Presentation.Helpers.Cryptography.DigitalSigning
{
    public class DigitalSigningHelperV3
    {
        public static string DigitallySignData(MemoryStream dataToSign, string privateKey)
        {
            /* RSA implementations internally use native memory, unmanaged system resources, operating system cryptography handles,
             * and cryptographic provider contexts which are not directly controlled by the .NET garbage collector. 
             * Wrapping the instantiation of RSA in a using clause allows these resources to be disposed once execution leaves the clause. 
             * Therefore, the lifetime of the RSA instance lies within the using clause. 
             * Failure to use the using clause yields the following results: 
             *  - Resources may stay allocated for longer than necessary. 
             *  - Handles may leak temporarily. 
             *  - Performance may degrade under heavy load. 
             * Unmanaged resources exist outside the CLR heap, are native operating system resources, and require explicit cleanup/disposal. 
             * Examples of which include database connections, cryptographic providers, sockets, and file handles. 
             * Whereas, managed memory is tracked by the .NET Garbage Collector and hence implicit disposal takes place. 
             * In short, the using statement/clause ensures unmanaged resources are disposed of properly. */
            using (RSA rsaAlgorithm = RSA.Create())
            {
                //FromXmlString is not ideal to use in modern .NET applications as it is a legacy method from the .NET Framework era.

                /* ImportFromPem can be used to import both the public key and the private key. 
                 * It detects the key type automatically and is simpler to invoke than the lower-level ImportRSAPublicKey and ImportRSAPrivateKey.
                 * PEM (short for Privacy-Enhanced Mail) is a text encoding format for cryptographic data enclosed in BEGIN/END markers.
                 * It is the defacto industry standard used by OpenSSL, Linux, Azue, Kubernetes, modern .NET, Java, Python, Go, and AWS. */
                rsaAlgorithm.ImportFromPem(privateKey);

                //CanSeek denotes whether the read/write position of the stream can be adjusted accordingly.
                if (dataToSign.CanSeek)
                    dataToSign.Position = 0; //Reset the position of the stream to 0.

                /* For RSA signatures, SHA256 is usually preferred as SHA512 may add CPU cost but offers minimal practical advantage.
                 * The latter may sometimes be faster on 64-bit CPUs.
                 * Furthermore, the modern recommendation is to use opt for RSASignaturePadding.Pss instead of RSASignaturePadding.Pkcs1
                 * unless the latter is required for interoperability purposes.
                 * The use of RSASignaturePadding.Pss is suggested due to enhanced mathemtical robustness and resistance to cryptographic attacks. */

                /* SignData() does not require manual hashing (this method takes care of producing a hashed digest).
                 * For this reason alone, it is safer, less error-prone, and prevents mismatch bugs compared to SignHash(). */
                byte[] signature = rsaAlgorithm.SignData(dataToSign, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
                return Convert.ToBase64String(signature);
            }
        }

        public static bool VerifyData(MemoryStream dataToVerify, string signature, string publicKey)
        {
            RSA rsaAlgorithm = RSA.Create();
            rsaAlgorithm.ImportFromPem(publicKey);

            //CanSeek denotes whether the read/write position of the stream can be adjusted accordingly.
            if (dataToVerify.CanSeek)
                dataToVerify.Position = 0; //Reset the position of the stream to 0.

            /* VerifyData() does not require manual hashing (this method takes care of producing a hashed digest). 
             * For this reason alone, it is safer, less error-prone, and prevents mismatch bugs compared to VerifyData(). */
            return rsaAlgorithm.VerifyData(dataToVerify, Convert.FromBase64String(signature), HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
        }
    }
}