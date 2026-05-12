using Common.Models;
using DataAccess.Repositories;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Validators
{
    public class CategoryValidationAttribute : ValidationAttribute
    {
        public CategoryValidationAttribute()
        {
            this.ErrorMessage = "Category must be within the accepted threshold.";
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            int categoryId = (int)value;
            CategoriesRepository categoriesRepository = (CategoriesRepository)validationContext.GetService(typeof(CategoriesRepository));
            if (categoriesRepository == null) return null;

            IQueryable<Category> existingCategories = categoriesRepository.GetCategories();

            if (existingCategories.Count(category => category.Id == categoryId) == 0)
                return new ValidationResult(this.ErrorMessage);

            return ValidationResult.Success;
        }
    }
}
