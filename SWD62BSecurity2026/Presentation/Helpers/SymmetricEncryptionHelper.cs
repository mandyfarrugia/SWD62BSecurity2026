using System.Security.Cryptography;
using System.Text;

namespace Presentation.Helpers
{
    public class SymmetricEncryptionHelper
    {
        public static SymmetricParameters GenerateSymmetricParameters(SymmetricAlgorithm algorithm) 
        {
            //Approach 1: We can generate a random key and IV using the selected algorithm.

            algorithm.GenerateKey();
            algorithm.GenerateIV();

            return new SymmetricParameters {
                Key = algorithm.Key,
                IV = algorithm.IV
            };
        }

        public static SymmetricParameters GenerateSymmetricParameters(SymmetricAlgorithm algorithm, string userInput)
        {
            /* Approach 2: We can use a user's input to generate the key and IV. 
             * When is this used? When you want to encrypt data owned by a specific user in a different way to ensure more security because if there was a break for a particular user because his/her password */

            byte[] userInputBytes = Encoding.UTF8.GetBytes(userInput); //Convert the user input to an array of bytes.
            //A salt is a random value that is added to the user input to render the derived key more secure.
            byte[] salt = new byte[16];

            salt = new byte[] { 0x1A, 0x2B, 0x3C, 0x4D, 0x5E, 0x6F, 0x7A, 0x8B, 0x9C, 0xAD, 0xBE, 0xCF, 0xD1, 0xE2, 0xF3, 0x04 }; //Example of a fixed salt. In practice, you should use a unique salt for each user input.

            //Instead of using a fixed salt, we can generate a random salt for each user input to enhance security. This way, even if two users have the same password, their derived keys will be different due to the unique salt.
            //using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
            //{
            //    randomNumberGenerator.GetBytes(salt);
            //}

            Rfc2898DeriveBytes keyGenerator = new Rfc2898DeriveBytes(userInputBytes, salt, 1000, new HashAlgorithmName("SHA512"));

            SymmetricParameters symmetricParameters = new SymmetricParameters
            {
                Key = keyGenerator.GetBytes(algorithm.KeySize / 8), //Use / 8 to convert bits to bytes as KeySize returns the value in bits.
                IV = keyGenerator.GetBytes(algorithm.BlockSize / 8) //Use / 8 to convert bits to bytes as BlockSize returns the value in bits.
            };

            return symmetricParameters;
        }

        public static string Encrypt(string plainText, SymmetricParameters keys, SymmetricAlgorithm algorithm) 
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            algorithm.Key = keys.Key;
            algorithm.IV = keys.IV;

            MemoryStream memoryStreamInput = new MemoryStream(plainTextBytes);
            MemoryStream memoryStreamOutput = new MemoryStream(); //Where the cipher (encrypted bytes) will be stored.

            using (CryptoStream cryptoStream = new CryptoStream(memoryStreamInput, algorithm.CreateEncryptor(), CryptoStreamMode.Read))
            {
                cryptoStream.CopyTo(memoryStreamOutput);
            }

            byte[] cipher = memoryStreamOutput.ToArray(); //Convert memory stream into an array of bytes.
            return Convert.ToBase64String(cipher); //Convert the cipher (array of bytes) into a Base64 string for easier storage and transmission.
        }

        public static string Encrypt(MemoryStream memoryStreamInput, SymmetricParameters keys, SymmetricAlgorithm symmetricAlgorithm)
        {
            symmetricAlgorithm.Key = keys.Key;
            symmetricAlgorithm.IV = keys.IV;

            MemoryStream memoryStreamOutput = new MemoryStream();
            using(CryptoStream cryptoStream = new CryptoStream(memoryStreamInput, symmetricAlgorithm.CreateEncryptor(), CryptoStreamMode.Read))
            {
                cryptoStream.CopyTo(memoryStreamOutput);
            }

            byte[] cipher = memoryStreamOutput.ToArray();
            return Convert.ToBase64String(cipher);
        }

        public static string Decrypt(string cipherText, SymmetricParameters keys, SymmetricAlgorithm algorithm) 
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            algorithm.Key = keys.Key;
            algorithm.IV = keys.IV;

            MemoryStream memoryStreamInput = new MemoryStream(cipherBytes);
            MemoryStream memoryStreamOutput = new MemoryStream(); //Where the clear/plain text will be stored.

            using (CryptoStream cryptoStream = new CryptoStream(memoryStreamInput, algorithm.CreateDecryptor(), CryptoStreamMode.Read))
            {
                cryptoStream.CopyTo(memoryStreamOutput);
            }

            byte[] clearTextBytes = memoryStreamOutput.ToArray(); //Convert memory stream into an array of bytes.
            return Encoding.UTF8.GetString(clearTextBytes); //Convert the clear text (array of bytes) to human readable text.
        }
    }
}
