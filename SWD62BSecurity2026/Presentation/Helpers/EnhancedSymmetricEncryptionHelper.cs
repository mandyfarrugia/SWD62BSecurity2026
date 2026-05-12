using System.Security.Cryptography;

namespace Presentation.Helpers
{
    public class EnhancedSymmetricEncryptionHelper
    {
        public static SymmetricParameters GetSymmetricParameters(SymmetricAlgorithm symmetricAlgorithm)
        {
            return SymmetricEncryptionHelper.GenerateSymmetricParameters(symmetricAlgorithm);
        }

        public static MemoryStream Encrypt(MemoryStream memoryStreamInput, SymmetricParameters symmetricParameters, SymmetricAlgorithm symmetricAlgorithm)
        {
            symmetricAlgorithm.Key = symmetricParameters.Key;
            symmetricAlgorithm.IV = symmetricParameters.IV;

            MemoryStream memoryStreamOutput = new MemoryStream();

            using (CryptoStream cryptoStream = new CryptoStream(memoryStreamOutput, symmetricAlgorithm.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true))
            {
                memoryStreamInput.CopyTo(cryptoStream);
            }

            memoryStreamOutput.Position = 0;
            return memoryStreamOutput;
        }

        public static MemoryStream Decrypt(MemoryStream memoryStreamInput, SymmetricParameters symmetricParameters, SymmetricAlgorithm symmetricAlgorithm)
        {
            symmetricAlgorithm.Key = symmetricParameters.Key;
            symmetricAlgorithm.IV = symmetricParameters.IV;

            MemoryStream memoryStreamOutput = new MemoryStream();

            using (CryptoStream cryptoStream = new CryptoStream(memoryStreamInput, symmetricAlgorithm.CreateDecryptor(), CryptoStreamMode.Read, leaveOpen: true))
            {
                cryptoStream.CopyTo(memoryStreamOutput);
            }

            memoryStreamOutput.Position = 0;
            return memoryStreamOutput;
        }
    }
}
