using System.Net;
using Faces.WebMvc.ViewModels;
using Microsoft.Extensions.Options;
using Refit;

namespace Faces.WebMvc.RestClients
{
    public class OrderManagementApi : IOrderManagementApi
    {
        private IOrderManagementApi _orderManagementApi;
        private readonly IOptions<AppSettings> _settings;

        public OrderManagementApi(IOptions<AppSettings> settings,
                                  HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri($"{settings.Value.OrdersApiUrl}/api");
            _orderManagementApi = RestService.For<IOrderManagementApi>(httpClient);
            _settings = settings;
        }

        public async Task<List<OrderViewModel>> GetOrders()
        {
            return await _orderManagementApi.GetOrders();
        }

        public async Task<OrderViewModel> GetOrderById(Guid orderId)
        {
            try
            {
                return await _orderManagementApi.GetOrderById(orderId);
            }
            catch (ApiException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}