using System.ComponentModel.DataAnnotations;

namespace WebCityEvents.ViewModels
{
    public class CustomerViewModel
    {
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Поле ФИО обязательно для заполнения")]
        [RegularExpression(@"^[А-Яа-яЁёA-Za-z\s]+$", ErrorMessage = "ФИО должно содержать только буквы")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Поле паспортные данные обязательно для заполнения")]
        public string PassportData { get; set; }
    }
}