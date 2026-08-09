using Faces.WebMvc.RestClients;
using Faces.WebMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Faces.WebMvc.Pages;

public class OrderManagementDetailModel : PageModel
{
    [BindProperty]
    public OrderViewModel Order { get; set; }

    private readonly IOrderManagementApi _orderManagementApi;

    public OrderManagementDetailModel(IOrderManagementApi orderManagementApi)
    {
        _orderManagementApi = orderManagementApi;
    }

    public async Task OnGet()
    {
        var orderId = Guid.Parse(Request.Query["OrderId"]);
        Order = await _orderManagementApi.GetOrderById(orderId);
        Order.ImageString = ConvertAndFormatToString(Order.ImageData);

        foreach (var detail in Order.OrderDetails)
        {
            detail.ImageString = ConvertAndFormatToString(detail.FaceData);
        }
    }

    private string ConvertAndFormatToString(byte[] imageData)
    {
        string imageBase64Data = Convert.ToBase64String(imageData);
        return string.Format("data:image/png;base64, {0}", imageBase64Data);
    }
}
