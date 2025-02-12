using System.Net;
using System.Net.Mail;

namespace platform.Services.Email
{
    public class SMTPEmailService : IEmailService
    {
        private readonly SmtpClient _client;
        private string _smtpAddress = "smtp.mail.yahoo.com";
        private int _portNumber = 587;
        private string _emailFrom = "lazaralexandru14@yahoo.com";
        private string _password = "pcukkvyauemvcnpc";

        public SMTPEmailService()
        {
            _client = new SmtpClient(_smtpAddress, _portNumber)
            {
                Credentials = new NetworkCredential(_emailFrom, _password),
                EnableSsl = true,
            };
        }

        public void SendEmailYahooRegister(string emailTo, Guid activationCode)
        {
            try
            {
                string subject = "Account Activation";
                string activationUrl =
                    $"http://localhost:5285/Account/VerifyAccount?activationCode={activationCode}";
                string body =
                    $"Hello,\n\nPlease click the following link to activate your account: \n{activationUrl}\n\nBest regards,\nYour Platform Team";

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(_emailFrom, "Platform Team");
                    mail.To.Add(emailTo);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = false;

                    _client.Send(mail);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending password reset email: {ex.Message}");
            }
        }

        public void SendEmailYahooForgot(string emailTo, Guid activationCode)
        {
            try
            {
                string subject = "Forgot Password";
                string activationUrl =
                    $"http://localhost:5285/Account/ResetPassword?resetPasswordCode={activationCode}";
                string body =
                    $"Hello,\n\nPlease click the following link to change your password: \n{activationUrl}\n\nBest regards,\nYour Platform Team";

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(_emailFrom, "Platform Team");
                    mail.To.Add(emailTo);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = false;

                    _client.Send(mail);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending password reset email: {ex.Message}");
            }
        }

        public void SendEmailYahooReset(string emailTo, string resetLink)
        {
            try
            {
                string subject = "Password Reset Request";

                string body =
                    $@"
Hello,

We received a request to reset your password. Please click the link below to reset your password:

{resetLink}

If you did not request a password reset, please ignore this email or contact support if you have concerns.

Best regards,
Your Platform Team
";

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(_emailFrom, "Platform team");
                    mail.To.Add(emailTo);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = false;

                    _client.Send(mail);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending password reset email: {ex.Message}");
            }
        }
    }
}
