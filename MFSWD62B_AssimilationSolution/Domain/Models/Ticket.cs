namespace Domain.Models
{
    public enum Status { Paid, Cancelled, Used }

    public class Ticket
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IdCard { get; set; }
        public int EventFK { get; set; }
        public virtual Event Event { get; set; }
        public int Quantity { get; set; }
        public Status Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

    }
}
