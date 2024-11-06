using WebCityEvents.Models;

namespace WebCityEvents.Tests
{
    internal static class TestDataHelper
    {
        public static List<Customer> GetFakeCustomersList()
        {
            return new List<Customer>
            {
                new Customer { CustomerID = 1, FullName = "John Doe", PassportData = "AB123456" },
                new Customer { CustomerID = 2, FullName = "Jane Doe", PassportData = "CD789012" },
                new Customer { CustomerID = 3, FullName = "Sam Smith", PassportData = "EF345678" }
            };
        }

        public static List<Event> GetFakeEventsList()
        {
            return new List<Event>
            {
                new Event
                {
                    EventID = 1,
                    EventName = "Concert",
                    PlaceID = 1,
                    EventDate = new DateTime(2024, 11, 15),
                    TicketPrice = 100,
                    TicketAmount = 200,
                    OrganizerID = 1,
                    Place = new Place
                    {
                        PlaceID = 1,
                        PlaceName = "Main Hall"
                    },
                    Organizer = new Organizer
                    {
                        OrganizerID = 1,
                        FullName = "John Smith"
                    }
                },
                new Event
                {
                    EventID = 2,
                    EventName = "Art Exhibition",
                    PlaceID = 2,
                    EventDate = new DateTime(2024, 12, 1),
                    TicketPrice = 50,
                    TicketAmount = 150,
                    OrganizerID = 2,
                    Place = new Place
                    {
                        PlaceID = 2,
                        PlaceName = "Gallery"
                    },
                    Organizer = new Organizer
                    {
                        OrganizerID = 2,
                        FullName = "Art Inc."
                    }
                }
            };
        }

        public static List<TicketOrder> GetFakeTicketOrdersList()
        {
            return new List<TicketOrder>
            {
                new TicketOrder
                {
                    OrderID = 1,
                    EventID = 1,
                    CustomerID = 1,
                    TicketCount = 2,
                    OrderDate = DateTime.Now.AddDays(-10),
                    Event = GetFakeEventsList().First(e => e.EventID == 1),
                    Customer = GetFakeCustomersList().First(c => c.CustomerID == 1)
                },
                new TicketOrder
                {
                    OrderID = 2,
                    EventID = 2,
                    CustomerID = 2,
                    TicketCount = 3,
                    OrderDate = DateTime.Now.AddDays(-5),
                    Event = GetFakeEventsList().First(e => e.EventID == 2),
                    Customer = GetFakeCustomersList().First(c => c.CustomerID == 2)
                }
            };
        }

        public static List<Place> GetFakePlacesList()
        {
            return new List<Place>
            {
                new Place { PlaceID = 1, PlaceName = "Main Hall", Geolocation = "50.123, 30.567" },
                new Place { PlaceID = 2, PlaceName = "Gallery", Geolocation = "50.456, 30.789" },
                new Place { PlaceID = 3, PlaceName = "Open Air Stage", Geolocation = "50.789, 30.101" }
            };
        }

        public static List<Organizer> GetFakeOrganizersList()
        {
            return new List<Organizer>
            {
                new Organizer
                {
                    OrganizerID = 1,
                    FullName = "John Smith",
                    Post = "Manager"
                },
                new Organizer
                {
                    OrganizerID = 2,
                    FullName = "Art Inc.",
                    Post = "Director"
                },
                new Organizer
                {
                    OrganizerID = 3,
                    FullName = "Jane Doe",
                    Post = "Coordinator"
                }
            };
        }
    }
}
