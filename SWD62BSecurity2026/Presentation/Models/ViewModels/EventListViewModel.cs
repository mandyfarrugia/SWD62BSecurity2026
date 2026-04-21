using System.ComponentModel;

namespace Presentation.Models.ViewModels
{
    public class EventListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public bool Public { get; set; }
        [DisplayName("Maximum Tickets")]
        public int MaximumTickets { get; set; }
        public string FilePath { get; set; }
    }
}
