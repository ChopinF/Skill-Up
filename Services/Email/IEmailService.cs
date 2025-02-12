namespace platform.Services.Email
{
    public interface IEmailService
    {
        public void SendEmailYahooRegister(string emailTo, Guid activationCode);
        public void SendEmailYahooReset(string email, string resetLink);
        public void SendEmailYahooForgot(string email, Guid activationCode);
    }
}
