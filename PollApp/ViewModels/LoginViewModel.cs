using System.ComponentModel.DataAnnotations;

namespace PollApp.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Пожалуйста, введите Email")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }
}