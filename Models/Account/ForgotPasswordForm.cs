using System.ComponentModel.DataAnnotations;

namespace platform.Models.Account
{
    public class ForgotPasswordForm
    {
        [Display(Name = "Your email")]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }
    }
}
