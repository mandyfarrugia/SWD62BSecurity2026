using System.Security.Cryptography;

namespace Presentation.Helpers.Cryptography.AsymmetricEncryption
{
    public class AsymmetricEncryptionHelper
    {
        public static AsymmetricParameters GeneratePemKeys()
        {
            using (RSA rsaAlgorithm = RSA.Create())
            {
                return new AsymmetricParameters()
                {
                    PublicKey = rsaAlgorithm.ExportSubjectPublicKeyInfoPem(),
                    PrivateKey = rsaAlgorithm.ExportPkcs8PrivateKeyPem()
                };
            }
        }

        public static AsymmetricParameters GenerateKeys()
        {
            using(RSA rsaAlgorithm = RSA.Create(1024))
            {
                return new AsymmetricParameters()
                {
                    PublicKey = rsaAlgorithm.ToXmlString(false), //false denotes export only the public key.
                    PrivateKey = rsaAlgorithm.ToXmlString(true) //true denotes export the private key along with the public key.
                };
            }
        }

        public static byte[] Encrypt(byte[] data, string publicKey)
        {
            using (RSA rsaAlgorithm = RSA.Create()) 
            {
                //Make sure that from the XML string, we import the public key into the algorithm.
                rsaAlgorithm.FromXmlString(publicKey); //Import the public key.
                byte[] cipher = rsaAlgorithm.Encrypt(data, RSAEncryptionPadding.Pkcs1); //Encrypt data using the public key.
                return cipher; //Return encrypted data as a byte array.
            }
        }

        public static byte[] Decrypt(byte[] cipher, string privateKey) 
        {
            using (RSA rsaAlgorithm = RSA.Create())
            {
                //Make sure that from the XML string, we import the private key into the algorithm.
                rsaAlgorithm.FromXmlString(privateKey); //Import the private key.
                byte[] clearBytes = rsaAlgorithm.Decrypt(cipher, RSAEncryptionPadding.Pkcs1); //Decrypt data using the private key.
                return clearBytes; //Return decrypted data as a byte array.
            }
        }

        public static string Encrypt(string data, string publicKey)
        {
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(data); //Convert the string data to a byte array.
            byte[] cipherBytes = Encrypt(dataBytes, publicKey); //Encrypt the byte array using the previously defined Encrypt method.
            return Convert.ToBase64String(cipherBytes); //Return the encrypted data as a Base64 string for easier storage and transmission.
        }

        public static string Decrypt(string cipherText, string privateKey)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText); //Convert the Base64 string back to a byte array.
            byte[] clearBytes = Decrypt(cipherBytes, privateKey); //Decrypt the byte array using the previously defined Decrypt method.
            return System.Text.Encoding.UTF8.GetString(clearBytes); //Convert the decrypted byte array back to a string and return it.
        }
    }
}