using System.ComponentModel.DataAnnotations;

namespace WebCityEvents.ViewModels
{
    public class EventViewModel
    {
        public int EventID { get; set; }

        [Required(ErrorMessage = "Название мероприятия обязательно")]
        public string EventName { get; set; }

        public int PlaceID { get; set; }

        public int OrganizerID { get; set; }

        public string PlaceName { get; set; }

        public string OrganizerName { get; set; }

        public DateTime EventDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Цена билета не может быть отрицательной")]
        public float TicketPrice { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Количество билетов должно быть больше нуля")]
        public int TicketAmount { get; set; }
        public int AvailableTickets { get; set; }
    }
}