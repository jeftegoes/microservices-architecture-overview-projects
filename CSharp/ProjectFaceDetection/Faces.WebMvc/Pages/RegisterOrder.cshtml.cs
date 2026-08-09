using Faces.WebMvc.ViewModels;
using MassTransit;
using Messaging.Interfaces.SharedLib.Commands;
using Messaging.Interfaces.SharedLib.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Faces.WebMvc.Pages;

public class RegisterOrderModel : PageModel
{
    private readonly ILogger<RegisterOrderModel> _logger;
    private readonly IBusControl _busControl;

    public RegisterOrderModel(ILogger<RegisterOrderModel> logger, IBusControl busControl)
    {
        _busControl = busControl;
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        return Page();
    }


    [BindProperty]
    public OrderViewModel OrderViewModel { get; set; } = default!;

    public async Task<IActionResult> OnPostAsync()
    {
        var memoryStream = new MemoryStream();

        using (var uploadedFile = OrderViewModel.File.OpenReadStream())
        {
            await uploadedFile.CopyToAsync(memoryStream);
        }

        OrderViewModel.ImageData = memoryStream.ToArray();
        OrderViewModel.PictureUrl = OrderViewModel.File.FileName;
        OrderViewModel.OrderId = Guid.NewGuid();

        var sendToUri = new Uri($"{RabbitMqMassTransitConstants.RabbitMqUri}" + $"{RabbitMqMassTransitConstants.RegisterOrderCommandQueue}");

        var endPoint = await _busControl.GetSendEndpoint(sendToUri);

        await endPoint.Send<IRegisterOrderCommand>(new
        {
            OrderViewModel.OrderId,
            OrderViewModel.PictureUrl,
            OrderViewModel.UserEmail,
            OrderViewModel.ImageData
        });

        return Redirect($"Thanks?OrderId={OrderViewModel.OrderId}");
    }
}
