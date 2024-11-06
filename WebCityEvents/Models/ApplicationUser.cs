using Microsoft.AspNetCore.Identity;

namespace WebCityEvents.Models
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime RegistrationDate { get; set; }
    }
}