using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models
{
    public class Book
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [RegularExpression(@"^[A-Za-z ]+$", ErrorMessage = "Name can only contain letters and spaces.")]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Fill in the author name")]
        public string Author { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Filename { get; set; }

        [Required(ErrorMessage = "Fill in the year")]
        public int Year { get; set; }

        [Required]
        [ForeignKey(nameof(Category))]
        public int CategoryFK { get; set; }

        public virtual Category Category { get; set; }

        public virtual List<Permission> Permissions { get; set; }

        public string? EncryptedSymmetricKey { get; set; }
        public string? EncryptedSymmetricIV { get; set; }
        public string? DigitalSignature { get; set; }
    }
}
