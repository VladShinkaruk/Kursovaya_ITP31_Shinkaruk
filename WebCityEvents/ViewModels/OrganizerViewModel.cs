using System.ComponentModel.DataAnnotations;

namespace WebCityEvents.ViewModels
{
    public class OrganizerViewModel
    {
        public int OrganizerID { get; set; }

        [Required(ErrorMessage = "Поле ФИО обязательно для заполнения")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Поле должность обязательно для заполнения")]
        public string Post { get; set; }
    }
}