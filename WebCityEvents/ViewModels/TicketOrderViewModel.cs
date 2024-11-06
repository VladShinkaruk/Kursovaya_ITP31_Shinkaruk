using System.ComponentModel.DataAnnotations;

namespace WebCityEvents.ViewModels
{
    public class TicketOrderViewModel
    {
        public int OrderID { get; set; }

        [Required(ErrorMessage = "Необходимо выбрать мероприятие")]
        [Range(1, int.MaxValue, ErrorMessage = "Мероприятие должно быть выбрано из списка")]
        public int EventID { get; set; }

        public int CustomerID { get; set; }

        public string CustomerName { get; set; }

        public string CustomerPassportData { get; set; }

        public string EventName { get; set; }

        public DateTime OrderDate { get; set; }

        [Required(ErrorMessage = "Количество билетов обязательно")]
        [Range(1, int.MaxValue, ErrorMessage = "Количество билетов должно быть больше 0")]
        public int TicketCount { get; set; }
    }
}