using Faces.WebMvc.ViewModels;
using Refit;

namespace Faces.WebMvc.RestClients
{
    public interface IOrderManagementApi
    {
        [Get("/order")]
        Task<List<OrderViewModel>> GetOrders();

        [Get("/order/{orderId}")]
        Task<OrderViewModel> GetOrderById(Guid orderId);
    }
}