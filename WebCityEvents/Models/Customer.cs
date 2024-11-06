namespace WebCityEvents.Models
{
    public class Customer
    {
        public int CustomerID { get; set; }
        public string FullName { get; set; }
        public string PassportData { get; set; }
        public ICollection<TicketOrder> TicketOrders { get; set; }

        public Customer()
        {
            TicketOrders = new List<TicketOrder>();
        }
    }
}