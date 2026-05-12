using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class CustomUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SecurityQuestion { get; set; }
        public string SecurityAnswer { get; set; }
        public string? PublicKeyXml { get; set; }
        public string? EncryptedPrivateKey { get; set; }
    }
}