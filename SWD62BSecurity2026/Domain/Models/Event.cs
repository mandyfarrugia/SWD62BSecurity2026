using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class Event
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public bool Public { get; set; }
        public int MaximumTickets { get; set; }
        public string FilePath { get; set; }
        public string Organiser { get; set; }
    }
}
