using MimeKit;

namespace EmailSenderLibrary
{
  public interface IEmailSender
  {
    MimeMessage GenerateContent(string toName, string toEmail, string subject, string body);
    void ResendEmailConfirmationToken(string fullName, string email, string tokenLink);
    void SendEmail(MimeMessage message);
    void SendEmailConfirmationToken(string fullName, string email, string tokenLink);
    void SendPasswordResetToken(string fullName, string email, string tokenLink);
  }
}
