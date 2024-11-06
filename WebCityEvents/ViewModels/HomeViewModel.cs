using WebCityEvents.Models;

namespace WebCityEvents.ViewModels
{
    public class HomeViewModel
    {
        public List<EventViewModel> Events { get; set; }
        public List<TicketOrderViewModel> TicketOrders { get; set; }
        public List<CustomerViewModel> Customers { get; set; }
        public List<ApplicationUser> Users { get; set; }
        public List<PlaceViewModel> Places { get; set; }
        public List<OrganizerViewModel> Organizers { get; set; }
    }
}