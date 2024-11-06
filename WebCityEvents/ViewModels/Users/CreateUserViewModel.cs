using System.ComponentModel.DataAnnotations;

namespace WebCityEvents.ViewModels.Users
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Имя пользователя обязательно")]
        [Display(Name = "Имя")]
        public string UserName { get; set; }

        [EmailAddress(ErrorMessage = "Некорректный адрес")]
        public string Email { get; set; }

        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Роль")]
        public string UserRole { get; set; }

        [Display(Name = "Дата регистрации")]
        public DateTime RegistrationDate { get; set; }

        public CreateUserViewModel()
        {
            UserRole = "user";
            RegistrationDate = DateTime.Now;
        }
    }
}