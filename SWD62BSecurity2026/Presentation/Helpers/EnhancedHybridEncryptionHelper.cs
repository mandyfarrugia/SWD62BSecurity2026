using System.Security.Cryptography;

namespace Presentation.Helpers
{
    public class EnhancedHybridEncryptionHelper
    {
        public static MemoryStream Encrypt(MemoryStream memoryStreamInput, string publicKey)
        {
            RSA rsa = RSA.Create();
            rsa.FromXmlString(publicKey);

            SymmetricParameters symmetricParameters = EnhancedSymmetricEncryptionHelper.GetSymmetricParameters(Aes.Create());
            MemoryStream memoryStreamCipher = EnhancedSymmetricEncryptionHelper.Encrypt(memoryStreamInput, symmetricParameters, Aes.Create());
            
            byte[] encryptedSecretKey = AsymmetricEncryptionHelper.Encrypt(symmetricParameters.Key, publicKey);
            
            byte[] encryptedIV = AsymmetricEncryptionHelper.Encrypt(symmetricParameters.IV, publicKey);
            
            MemoryStream memoryStreamOutput = new MemoryStream();
            memoryStreamOutput.Write(encryptedSecretKey, 0, encryptedSecretKey.Length);
            memoryStreamOutput.Write(encryptedIV, 0, encryptedIV.Length);
            
            memoryStreamCipher.CopyTo(memoryStreamOutput);
            memoryStreamOutput.Position = 0;
            return memoryStreamOutput;
        }

        public static MemoryStream Decrypt(MemoryStream memoryStreamCipherInput, string privateKey)
        {
            RSA algorithm = RSA.Create();
            algorithm.FromXmlString(privateKey);

            var rsaBlockSize = algorithm.KeySize / 8;

            byte[] encryptedKey = new byte[rsaBlockSize];
            byte[] encryptedIV = new byte[rsaBlockSize];

            memoryStreamCipherInput.Read(encryptedKey, 0, encryptedKey.Length);
            memoryStreamCipherInput.Read(encryptedIV, 0, encryptedIV.Length);

            MemoryStream memoryStreamCipher = new MemoryStream();
            memoryStreamCipherInput.CopyTo(memoryStreamCipher);
            memoryStreamCipher.Position = 0;

            byte[] decryptedKey = AsymmetricEncryptionHelper.Decrypt(encryptedKey, privateKey);
            byte[] decryptedIV = AsymmetricEncryptionHelper.Decrypt(encryptedIV, privateKey);

            SymmetricParameters symmetricParameters = new SymmetricParameters
            {
                Key = decryptedKey,
                IV = decryptedIV
            };

            MemoryStream memoryStreamOutput = EnhancedSymmetricEncryptionHelper.Decrypt(memoryStreamCipher, symmetricParameters, Aes.Create());
            memoryStreamOutput.Position = 0;
            return memoryStreamOutput;
        }
    }
}
