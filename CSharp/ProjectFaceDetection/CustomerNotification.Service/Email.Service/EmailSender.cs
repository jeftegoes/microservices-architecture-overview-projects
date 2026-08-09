using MailKit.Net.Smtp;
using MimeKit;

namespace Email.Service
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailConfig _emailConfig;

        public EmailSender(EmailConfig emailConfig)
        {
            _emailConfig = emailConfig;
        }

        public async Task SendEmailAsync(Message message)
        {
            var emailMessage = CreateEmailMessage(message);
            await SendAsync(emailMessage);
        }

        private MimeMessage CreateEmailMessage(Message message)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_emailConfig.From, _emailConfig.From));
            mimeMessage.To.AddRange(message.To);
            mimeMessage.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = string.Format("<h2 style='color: red;'>{0}</h2>", message.Content)
            };

            if (message.Attachments != null && message.Attachments.Any())
            {
                var i = 1;
                foreach (var attachment in message.Attachments)
                {
                    bodyBuilder.Attachments.Add("attachment" + i, attachment);
                    i++;
                }
            }

            mimeMessage.Body = bodyBuilder.ToMessageBody();

            return mimeMessage;
        }

        private async Task SendAsync(MimeMessage emailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, false);
                    //client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);
                    await client.SendAsync(emailMessage);
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync("" + ex.Message);
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }
        }
    }
}