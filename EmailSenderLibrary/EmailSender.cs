using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Security.Authentication;
using NewAgePOSModels.Securities;

namespace EmailSenderLibrary
{
  public class EmailSender : IEmailSender
  {
    public MimeMessage GenerateContent(string toName, string toEmail, string subject, string body)
    {
      MimeMessage message = new MimeMessage();

      MailboxAddress from = new MailboxAddress($"{ Secrets.WebsiteName } Admin", Secrets.SenderEmail);
      MailboxAddress to = new MailboxAddress(toName, toEmail);

      BodyBuilder bodyBuilder = new BodyBuilder()
      {
        HtmlBody = body
      };

      message.To.Add(to);
      message.From.Add(from);
      message.Subject = subject;
      message.Body = bodyBuilder.ToMessageBody();

      return message;
    }

    public void SendEmail(MimeMessage message)
    {
      using (SmtpClient client = new SmtpClient())
      {
        if (Secrets.EmailServer == EmailSenderServerEnum.Rackspace)
        {
          client.CheckCertificateRevocation = false;
          client.SslProtocols = SslProtocols.Tls;
        }

        try
        {
          client.Connect(Secrets.Host, Secrets.Port, MailKit.Security.SecureSocketOptions.SslOnConnect);
          client.Authenticate(Secrets.SenderEmail, Secrets.SenderPassword);
          client.Send(message);
          client.Disconnect(true);
        }
        catch (Exception e)
        {
          throw new Exception("There was a problem with either connecting or authenticating with the client", e);
        }
      }
    }

    public void SendEmailConfirmationToken(string fullName, string email, string tokenLink)
    {
      string subject = "New Registration Confirmation";

      string body = $"<h1>Hello { fullName } </h1> \n\n" +
          $"<p>You've recently registered on { Secrets.WebsiteName }</p> \n\n" +
          "<p>Please click below to confirm your email address</p> \n\n" +
          $"<a href='{ tokenLink }'><button style='color:#fff; background-color:#007bff; border-color:#007bff;'>Confirm</button></a> \n\n" +
          "<p>If the link doesn't work, you can copy and paste the below URL</p> \n\n" +
          $"<p> { tokenLink } </p> \n\n\n" +
          "<p>Thank you!</p>";

      MimeMessage message = GenerateContent(fullName, email, subject, body);

      SendEmail(message);
    }

    public void SendPasswordResetToken(string fullName, string email, string tokenLink)
    {
      string subject = "Password Reset Request Confirmation";

      string body = $"<h1>Hello { fullName } </h1> \n\n" +
          $"<p>You've recently requested for password reset</p> \n\n" +
          "<p>Please click below to reset your password</p> \n\n" +
          $"<a href='{ tokenLink }'><button style='color:#fff; background-color:#007bff; border-color:#007bff;'>Confirm</button></a> \n\n" +
          "<p>If the link doesn't work, you can copy and paste the below URL</p> \n\n" +
          $"<p> { tokenLink } </p> \n\n\n" +
          "<p>Thank you!</p>";

      MimeMessage message = GenerateContent(fullName, email, subject, body);

      SendEmail(message);
    }

    public void ResendEmailConfirmationToken(string fullName, string email, string tokenLink)
    {
      string subject = "Resend Email Confirmation Request";

      string body = $"<h1>Hello { fullName } </h1> \n\n" +
          $"<p>You've recently requested to resend email confirmation link on { Secrets.WebsiteName }</p> \n\n" +
          "<p>Please click below to confirm your email address</p> \n\n" +
          $"<a href='{ tokenLink }'><button style='color:#fff; background-color:#007bff; border-color:#007bff;'>Confirm</button></a> \n\n" +
          "<p>If the link doesn't work, you can copy and paste the below URL</p> \n\n" +
          $"<p> { tokenLink } </p> \n\n\n" +
          "<p>Thank you!</p>";

      MimeMessage message = GenerateContent(fullName, email, subject, body);

      SendEmail(message);
    }
  }
}
