using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class Ticket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Event))]
        public int EventFK { get; set; }
        public virtual Event Event { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string IdCard { get; set; }
        public Status Status { get; set; }
    }
}