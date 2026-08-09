namespace Email.Service
{
    public interface IEmailSender
    {
        Task SendEmailAsync(Message message);
    }
}