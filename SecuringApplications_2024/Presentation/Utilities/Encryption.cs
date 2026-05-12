using System.Security.Cryptography;
using System.Text;

namespace Presentation.Utilities
{
    public class Encryption
    {
        public byte[] Hash(byte[] input)
        {
            SHA512 sha512Algorithm = SHA512.Create();
            byte[] digest = sha512Algorithm.ComputeHash(input);
            return digest;
        }

        public string Hash(string input)
        {
            byte[] inputAsBytes = UTF32Encoding.UTF32.GetBytes(input);
            byte[] digest = this.Hash(inputAsBytes);
            string output = Convert.ToBase64String(digest);
            return output;
        }

        byte[] salt = new byte[] { 45, 2, 1, 49, 60, 100, 100, 2, 200, 178, 190, 35, 150 };

        public byte[] SymmetricEncrypt(byte[] input, string password)
        {
            Aes aesAlgorithm = Aes.Create();

            Rfc2898DeriveBytes keyGenerator = new Rfc2898DeriveBytes(password, salt);
            byte[] secretKey = keyGenerator.GetBytes(aesAlgorithm.KeySize / 8);
            byte[] iv = keyGenerator.GetBytes(aesAlgorithm.BlockSize / 8);

            MemoryStream memoryStreamInput = new MemoryStream(input);
            memoryStreamInput.Position = 0;

            MemoryStream memoryStreamOutput = new MemoryStream();

            using (CryptoStream cryptoStream = new CryptoStream(memoryStreamInput, aesAlgorithm.CreateEncryptor(secretKey, iv), CryptoStreamMode.Read))
            {
                cryptoStream.CopyTo(memoryStreamOutput);
            }

            memoryStreamOutput.Position = 0;
            return memoryStreamOutput.ToArray();
        }

        public byte[] SymmetricDecrypt(byte[] cipher, string password)
        {
            Aes aesAlgorithm = Aes.Create();

            Rfc2898DeriveBytes keyGenerator = new Rfc2898DeriveBytes(password, salt);
            byte[] secretKey = keyGenerator.GetBytes(aesAlgorithm.KeySize / 8);
            byte[] iv = keyGenerator.GetBytes(aesAlgorithm.BlockSize / 8);

            MemoryStream memoryStreamInput = new MemoryStream(cipher);
            memoryStreamInput.Position = 0;

            MemoryStream memoryStreamOutput = new MemoryStream();

            using (CryptoStream cryptoStream = new CryptoStream(memoryStreamInput, aesAlgorithm.CreateDecryptor(secretKey, iv), CryptoStreamMode.Read))
            {
                cryptoStream.CopyTo(memoryStreamOutput);
            }

            memoryStreamOutput.Position = 0;
            return memoryStreamOutput.ToArray();
        }
        
        public AsymmetricParameters GenerateAsymmetricKeys()
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                AsymmetricParameters parameters = new AsymmetricParameters
                {
                    PublicKey = rsa.ToXmlString(false),
                    PrivateKey = rsa.ToXmlString(true)
                };

                return parameters;
            }
        }

        public byte[] AsymmetricEncrypt(byte[] clearBytes, string publicKey)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(publicKey);
                byte[] cipher = rsa.Encrypt(clearBytes, true);
                return cipher;
            }
        }

        public byte[] AsymmetricDecrypt(byte[] cipher, string privateKey) 
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                byte[] originalData = rsa.Decrypt(cipher, true);
                return originalData;
            }
        }

        public byte[] DigitalSign(byte[] data, string privateKey)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                byte[] digest = this.Hash(data);
                SHA512 sha512Algorithm = SHA512.Create();
                byte[] signature = rsa.SignHash(digest, new HashAlgorithmName(nameof(SHA512)), RSASignaturePadding.Pkcs1);
                return signature;
            }
        }

        public bool DigitalVerify(byte[] data, byte[] signature, string publicKey)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.FromXmlString(publicKey);
                return rsa.VerifyData(data, signature, new HashAlgorithmName(nameof(SHA512)), RSASignaturePadding.Pkcs1);
            }
        }

        public (MemoryStream EncryptedFileStream, string EncryptedSymmetricKeyBase64, string EncryptedIVBase64) HybridEncrypt(byte[] fileBytes, string publicKeyXml)
        {
            using (Aes aes = Aes.Create())
            {
                aes.GenerateKey();
                aes.GenerateIV();

                MemoryStream encryptedFileStream = new MemoryStream();
                ICryptoTransform encryptor = aes.CreateEncryptor();

                CryptoStream cryptoStream = new CryptoStream(encryptedFileStream, encryptor, CryptoStreamMode.Write, leaveOpen: true);

                cryptoStream.Write(fileBytes, 0, fileBytes.Length);
                cryptoStream.FlushFinalBlock();
                cryptoStream.Close();

                byte[] encryptedKey = this.AsymmetricEncrypt(aes.Key, publicKeyXml);
                byte[] encryptedIV = this.AsymmetricEncrypt(aes.IV, publicKeyXml);

                string encryptedKeyBase64 = Convert.ToBase64String(encryptedKey);
                string encryptedIVBase64 = Convert.ToBase64String(encryptedIV);

                encryptedFileStream.Position = 0;

                return (encryptedFileStream, encryptedKeyBase64, encryptedIVBase64);
            }
        }
    }
}
