using DataAccess.Repositories;
using Domain.Models;
using Presentation.CustomValidators;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Models.ViewModels
{
    public class CreateTicketViewModel : Ticket
    {
        public CreateTicketViewModel()
        {
        }

        //public CreateTicketViewModel(EventsRepository eventsRepository)
        //{
        //    this.Events = eventsRepository.GetAllEvents();
        //}

        //[Required]
        //public int EventFK { get; set; }

        //public IQueryable<Event> Events { get; set; }

        //[Required(AllowEmptyStrings = false, ErrorMessage = "Please input your name!")]
        //[StringLength(100, ErrorMessage = "Name must be less than 100 characters!")]
        //public string Name { get; set; }

        //[Required(AllowEmptyStrings = false, ErrorMessage = "Please input your surname!")]
        //[StringLength(100, ErrorMessage = "Surname must be less than 100 characters!")]
        //public string Surname { get; set; }

        //[Required(AllowEmptyStrings = false, ErrorMessage = "Please input your ID number!")]
        //[RegularExpression(@"^[a-z][A-Z][0-9]*$", ErrorMessage = "ID number must only contain letters and numbers - no symbols!")]
        //[StringLength(10, ErrorMessage = "ID number must be less than 10 characters!")]
        //public string IdCard { get; set; }

        [TicketStockValidator]
        public override int Quantity { get; set; }
    }
}
