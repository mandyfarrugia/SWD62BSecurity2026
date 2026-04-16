using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.CustomExceptions
{
    public class DuplicateEventEntryException : Exception
    {
        public DuplicateEventEntryException() : base("Another event with the same name already exists!") {}
        public DuplicateEventEntryException(string message) : base(message) {}
        public DuplicateEventEntryException(string message, Exception innerException) : base(message, innerException) {}
    }
}