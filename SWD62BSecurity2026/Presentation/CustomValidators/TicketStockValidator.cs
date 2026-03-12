using DataAccess.Repositories;
using Domain.Models;
using Presentation.Models.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace Presentation.CustomValidators
{
    public class TicketStockValidator : ValidationAttribute
    {
        /// <summary>
        /// If access to database or services within the Dependency Injection instance pool (for instance, repository classes), 
        /// it is ideal to override the protected counterpart of the IsValid method.
        /// This comes with a second parameter validationContext of type ValidationContext consisting of the following members:<br></br>
        /// - ObjectInstance: the object being validated<br></br>
        /// - MemberName: The property being validated<br></br>
        /// - Items: A dictionary to pass additional data during the validation process. It can also be used to pass services instead of direct Dependency Injection invocations.<br></br>
        /// - GetService(): A method to retrieve services from the Dependency Injection instance pool.<br></br>
        /// </summary>
        /// <param name="value">The value being validated.</param>
        /// <param name="validationContext">System.ComponentModel.DataAnnotations.ValidationContext is a class used in .NET validation that provides context information when validating an object or property.</param>
        /// <returns>Returns a new instance of ValidationResult with an error message if validation fails, otherwise ValidationResult.Success.</returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            EventsRepository eventsRepository = (EventsRepository)validationContext.GetService<EventsRepository>();

            Event specificEvent = eventsRepository.GetAllEvents().SingleOrDefault(e => e.Id == ((CreateTicketViewModel)validationContext.ObjectInstance).EventFK);

            if (specificEvent.MaximumTickets >= (int)value)
                return ValidationResult.Success;
            else
                return new ValidationResult($"The maximum number of tickets for this event is {specificEvent.MaximumTickets}.");
        }
    }
}
