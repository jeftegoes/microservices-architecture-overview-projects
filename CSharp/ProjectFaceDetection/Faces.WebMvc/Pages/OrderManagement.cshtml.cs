using Faces.WebMvc.RestClients;
using Faces.WebMvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Faces.WebMvc.Pages;

public class OrderManagementModel : PageModel
{
    [BindProperty]
    public List<OrderViewModel> Orders { get; set; }

    private readonly IOrderManagementApi _orderManagementApi;

    public OrderManagementModel(IOrderManagementApi orderManagementApi)
    {
        _orderManagementApi = orderManagementApi;
    }

    public async Task OnGet()
    {
        Orders = await _orderManagementApi.GetOrders();

        foreach (var order in Orders)
        {
            order.ImageString = ConvertAndFormatToString(order.ImageData);
        }
    }

    private string ConvertAndFormatToString(byte[] imageData)
    {
        string imageBase64Data = Convert.ToBase64String(imageData);
        return string.Format("data:image/png;base64, {0}", imageBase64Data);
    }
}
