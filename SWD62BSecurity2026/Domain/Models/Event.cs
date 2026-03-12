using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class Event
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please input an event name!")]
        [RegularExpression(@"^[A-Za-z0-9\s]+$", ErrorMessage = "Event name must only contain letters, numbers, and whitespaces!")]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please input a price!")]
        [Range(0, 10000, ErrorMessage = "Price must be greater than 0 but less than 10000. If it is higher, contact administration.")]
        public double Price { get; set; }

        public bool Public { get; set; }

        [Range(0, 100000, ErrorMessage = "Maximum tickets must be greater than 0 but less than 100000. If it is higher, contact administration.")]
        public int MaximumTickets { get; set; }

        public string FilePath { get; set; }

        [EmailAddress(ErrorMessage = "Please input a valid email address!")]
        public string Organiser { get; set; }
    }
}