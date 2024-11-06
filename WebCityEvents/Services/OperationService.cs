using WebCityEvents.Data;
using WebCityEvents.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace WebCityEvents.Services
{
    public class OperationService : IOperationService
    {
        private readonly EventContext _context;

        public OperationService(EventContext context)
        {
            _context = context;
        }

        public HomeViewModel GetHomeViewModel(int numberRows = 10)
        {
            var events = _context.Events
                .Include(e => e.Organizer)
                .OrderByDescending(e => e.EventDate)
                .Select(e => new EventViewModel
                {
                    EventID = e.EventID,
                    EventName = e.EventName,
                    OrganizerName = e.Organizer.FullName,
                    EventDate = e.EventDate,
                    TicketPrice = e.TicketPrice,
                    TicketAmount = e.TicketAmount
                })
                .Take(numberRows)
                .ToList();

            var customers = _context.Customers
                .Select(c => new CustomerViewModel
                {
                    CustomerID = c.CustomerID,
                    FullName = c.FullName,
                    PassportData = c.PassportData
                })
                .Take(numberRows)
                .ToList();

            var ticketOrders = _context.TicketOrders
                .Include(o => o.Customer)
                .Include(o => o.Event)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new TicketOrderViewModel
                {
                    OrderID = o.OrderID,
                    CustomerName = o.Customer.FullName,
                    EventName = o.Event.EventName,
                    OrderDate = o.OrderDate,
                    TicketCount = o.TicketCount
                })
                .Take(numberRows)
                .ToList();

            var places = _context.Places
                .Select(p => new PlaceViewModel
                {
                    PlaceID = p.PlaceID,
                    PlaceName = p.PlaceName,
                    Geolocation = p.Geolocation
                })
                .Take(numberRows)
                .ToList();

            var organizers = _context.Organizers
                .Select(or => new OrganizerViewModel
                {
                    OrganizerID = or.OrganizerID,
                    FullName = or.FullName,
                    Post = or.Post,
                })
                .Take(numberRows)
                .ToList();

            HomeViewModel homeViewModel = new HomeViewModel
            {
                Events = events,
                Customers = customers,
                TicketOrders = ticketOrders,
                Places = places,
                Organizers = organizers
            };

            return homeViewModel;
        }
    }
}