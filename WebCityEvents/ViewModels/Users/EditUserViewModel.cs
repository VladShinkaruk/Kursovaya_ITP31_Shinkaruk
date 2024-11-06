using System.ComponentModel.DataAnnotations;

namespace WebCityEvents.ViewModels.Users
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Имя пользователя обязательно")]
        [Display(Name = "Имя")]
        public string UserName { get; set; }

        [EmailAddress(ErrorMessage = "Некорректный адрес")]
        public string Email { get; set; }

        [Display(Name = "Роль")]
        public string UserRole { get; set; }

        public EditUserViewModel()
        {
            UserRole = "user";
        }
    }
}