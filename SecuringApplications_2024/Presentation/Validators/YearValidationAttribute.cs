using System.ComponentModel.DataAnnotations;

namespace Presentation.Validators
{
    public class YearValidationAttribute : ValidationAttribute
    {
        public YearValidationAttribute()
        {
            this.ErrorMessage = "Year has to be in the past!";
        }

        public override bool IsValid(object? value)
        {
            int inputYear = (int)value;
            return inputYear <= DateTime.Today.Year;
        }
    }
}
