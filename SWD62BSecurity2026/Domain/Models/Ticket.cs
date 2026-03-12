using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public enum Status { Paid, Cancelled, Used }

    public class Ticket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Event))]
        public int EventFK { get; set; }

        [ValidateNever]
        public virtual Event Event { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please input your name!")]
        [StringLength(100, ErrorMessage = "Name must be less than 100 characters!")]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please input your surname!")]
        [StringLength(100, ErrorMessage = "Surname must be less than 100 characters!")]
        public string Surname { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please input your ID number!")]
        [RegularExpression(@"^[A-Za-z0-9 ]+$", ErrorMessage = "ID number must only contain letters and numbers - no symbols!")]
        [StringLength(10, ErrorMessage = "ID number must be less than 10 characters!")]
        public string IdCard { get; set; }

        public Status Status { get; set; }

        [Required]
        public virtual int Quantity { get; set; }
    }
}