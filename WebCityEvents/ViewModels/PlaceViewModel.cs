using System.ComponentModel.DataAnnotations;

namespace WebCityEvents.ViewModels
{
    public class PlaceViewModel
    {
        public int PlaceID { get; set; }

        [Required(ErrorMessage = "Поле название места обязательно для заполнения")]
        public string PlaceName { get; set; }

        [Required(ErrorMessage = "Поле геолокация обязательно для заполнения")]
        public string Geolocation { get; set; }
    }
}