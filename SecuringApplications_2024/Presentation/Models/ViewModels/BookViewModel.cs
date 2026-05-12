using Common.Models;
using DataAccess.Repositories;
using Presentation.Validators;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Models.ViewModels
{
    public class BookViewModel
    {
        public BookViewModel() {}

        public BookViewModel(CategoriesRepository categoriesRepository)
        {
            this.Categories = categoriesRepository.GetCategories();
        }

        public string EncryptedId { get; set; }

        public int Id { get; set; }

        [Required]
        [RegularExpression(@"^[A-Za-z ]+$", ErrorMessage = "Name can only contain letters and spaces.")]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Fill in the author name")]
        public string Author { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Filename { get; set; }

        public IFormFile File { get; set; }

        [Required(ErrorMessage = "Fill in the year")]
        [YearValidation()]
        public int Year { get; set; }

        [CategoryValidation()]
        public int CategoryFK { get; set; }

        public string CategoryName { get; set; }
        public IQueryable<Category> Categories { get; set; }
        public string EncryptedSymmetricKey { get; set; }
        public string EncryptedSymmetricIV { get; set; }
        public string? DigitalSignature { get; set; }
    }
}