using Email.Service;
using MassTransit;
using Messaging.Interfaces.SharedLib.Events;

namespace Notification.Service.Consumers
{
    public class OrderProcessedEventConsumer : IConsumer<IOrderProcessedEvent>
    {
        private readonly IEmailSender _emailSender;

        public OrderProcessedEventConsumer(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task Consume(ConsumeContext<IOrderProcessedEvent> context)
        {
            var rootFolder = AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));

            var result = context.Message;

            var facesData = result.Faces;

            if (facesData.Count < 1)
            {
                await Console.Out.WriteLineAsync($"No faces detected");
            }
            else
            {
                var j = 0;

                foreach (var face in facesData)
                {
                    var ms = new MemoryStream(face);

                    var image = Image.Load(ms);
                    image.Save(rootFolder + "/Images/Face" + j + ".jpg");

                    j++;
                }

                // string[] mailAddress = { result.UserEmail };

                // await _emailSender.SendEmailAsync(new Message(mailAddress, "Yout order" + result.OrderId, "From FacesAndFaces", facesData));

                await context.Publish<IOrderDispatchedEvent>(new
                {
                    orderId = context.Message.OrderId,
                    DispatchDateTime = DateTime.UtcNow
                });
            }
        }
    }
}