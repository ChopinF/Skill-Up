namespace platform.Models.Account
{
    public class ResetPasswordForm
    {
        public Guid ResetPasswordCode { get; set; }
        public string Email { get; set; }

        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
