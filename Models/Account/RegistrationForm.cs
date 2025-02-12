using System.ComponentModel.DataAnnotations;

namespace platform.Models.Account
{
    public class RegitrationForm
    {
        [Display(Name = "First Name")]
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }

        [Display(Name = "User Name")]
        [Required(ErrorMessage = "User name is required")]
        public string UserName { get; set; }

        [Display(Name = "Your email")]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Display(Name = "Your password")]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "Password must be at least 6 characters long."
        )]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Confirm your password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
