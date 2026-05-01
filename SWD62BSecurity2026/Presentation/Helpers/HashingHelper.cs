/* All algorithms discussed in relation to Crytography fall under System.Security.Cryptography. */
using System.Security.Cryptography;
using System.Text;

namespace Presentation.Helpers
{
    public class HashingHelper
    {
        //How to hash a string versus how to hash bytes?

        public byte[] Hash(byte[] input)
        {
            /* The bigger the number, the more iterations will be employed (more characters). */
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] digest = sha512.ComputeHash(input);
                return digest;
            }
        }

        public string Hash(string input)
        {
            /* When about to convert from a string to a byte array, it is important to pick the right method.
             * - If input is a string which is human-readable (such as a password or a text file, or in general, input is provided by the user and no cryptography was used), then use Encoding.UTF8.GetBytes().
             * - However, if input is a cryptographic string which thereby is not human-readable, then use Convert.FromBase64String(). */

            byte[] inputAsBytes = Encoding.UTF8.GetBytes(input);
            byte[] digest = this.Hash(inputAsBytes);

            //Convert from byte array to string.
            string result = Convert.ToBase64String(digest); //Convert the byte array to a Base64 string.
            return result;
        }

        //A hashing algorithm that accepts a salt.
        public byte[] HashWithSalt(string input)
        {
            //Convert everything into a byte array.
            byte[] inputAsBytes = Encoding.UTF8.GetBytes(input);

            //Salt can be hardcoded as well.
            byte[] saltAsBytes = { 50, 12, 110, 120, 35, 80, 167, 200, 201, 220, 255 };

            HMACSHA512 hmac = new HMACSHA512(saltAsBytes);
            byte[] digest = hmac.ComputeHash(inputAsBytes);
            return digest;
        }
    }
}
