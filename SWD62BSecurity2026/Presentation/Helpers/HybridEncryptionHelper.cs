using System.Security.Cryptography;

namespace Presentation.Helpers
{
    public class HybridParameters
    {
        public string EncryptedSymmetricKey { get; set; }
        public string EncryptedIV { get; set; }
        public string CipherText { get; set; }
    }

    public class HybridEncryptionHelper
    {
        public static MemoryStream Encrypt(MemoryStream memoryStreamInput, string publicKey)
        {
            //Use the algorithms from both asymmetric and symmetric.

            //1. Import the public key.
            RSA rsa = RSA.Create();
            rsa.FromXmlString(publicKey);

            //2. Generate the symmetric keys.
            SymmetricParameters symmetricParameters = SymmetricEncryptionHelper.GenerateSymmetricParameters(Aes.Create());

            //Exercise: Create an overload of the SymmetricEncryptionHelper.Encrypt() to accept a memory stream.

            //3. Convert the memory stream into a string.
            string textToEncrypt = System.Text.Encoding.UTF8.GetString(memoryStreamInput.ToArray());
            string cipherAsText = SymmetricEncryptionHelper.Encrypt(textToEncrypt, symmetricParameters, Aes.Create());

            //4. Encrypt the symmetric keys using the public key.
            byte[] encryptedSecretKey = AsymmetricEncryptionHelper.Encrypt(symmetricParameters.Key, publicKey);
            byte[] encryptedIV = AsymmetricEncryptionHelper.Encrypt(symmetricParameters.IV, publicKey);

            //5. Store the encrypted key and encrypted IV and encrypted data (file) into one MemoryStream and return it.
            MemoryStream memoryStreamOutput = new MemoryStream();
            memoryStreamOutput.Write(encryptedSecretKey, 0, encryptedSecretKey.Length);
            memoryStreamOutput.Write(encryptedIV, 0, encryptedIV.Length);

            /* When deciding on the conversion of the cipher text to byte array, consult what was the last conversion before obtaining cipherAsText. 
             * If it was Convert.ToBase64String, use Convert.FromBase64String. */
            MemoryStream memoryStreamCipher = new MemoryStream(Convert.FromBase64String(cipherAsText));
            memoryStreamCipher.CopyTo(memoryStreamOutput);
            memoryStreamOutput.Position = 0; //Reset the position to the beginning of the stream before returning it.
            return memoryStreamOutput;
        }

        public static MemoryStream Decrypt(MemoryStream memoryStreamCipherIn, string privateKey) 
        {
            //Exercise: Implement the decryption logic for the above encryption method.

            //1. Create an instance of the RSA algorithm and import the private key.

            /* 2. Open memoryStreamCipherIn and and first read the:
             *  - encrypted key (128 bytes, when you are reading, read memoryStreamCipherIn.Read(encrpytedKey, 0, 128))
             *  - encrypted IV (128 bytes, when you are reading, read memoryStreamCipherIn.Read(encrpytedIV, 0, 128))
             *  - encrypted data (read what is left, that would be the cipher text) */

            //3. Decrypt the encrypted key and IV using the private key (asymmetric).

            //4. Decrypt the cipher text using the decrypted key and IV (symmetric).

            //5. Return the decrypted data as a MemoryStream.
        }
    }
}
