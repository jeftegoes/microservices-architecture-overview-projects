using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MassTransit;
using Messaging.Interfaces.SharedLib.Commands;
using Messaging.Interfaces.SharedLib.Events;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orders.Api.Entities;
using Orders.Api.Hubs;
using Orders.Api.Persistence;

namespace Orders.Api.Messages.Consumers
{
    public class RegisterOrderCommandConsumer : IConsumer<IRegisterOrderCommand>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly IOptions<OrderSettings> _settings;

        public RegisterOrderCommandConsumer(IOrderRepository orderRepository,
                                            IHttpClientFactory clientFactory,
                                            IHubContext<OrderHub> hubContext,
                                            IOptions<OrderSettings> settings)
        {
            _orderRepository = orderRepository;
            _clientFactory = clientFactory;
            _hubContext = hubContext;
            _settings = settings;
        }

        public async Task Consume(ConsumeContext<IRegisterOrderCommand> context)
        {
            var result = context.Message;

            if (result == null)
                return;

            if (result.PictureUrl != null &&
                result.UserEmail != null &&
                result.ImageData != null)
            {
                await SaveOrder(result);

                await _hubContext.Clients.All.SendAsync("UpdateOrders", "New Order Created", result.OrderId);

                Tuple<List<byte[]>, Guid> orderDetailData = await GetFacesFromFaceApiAsync(result.ImageData, result.OrderId);
                var faces = orderDetailData.Item1;
                var orderId = orderDetailData.Item2;

                SaveOrderDetail(orderId, faces);

                await _hubContext.Clients.All.SendAsync("UpdateOrders", "Order processed", result.OrderId);

                await context.Publish<IOrderProcessedEvent>(new
                {
                    OrderId = orderId,
                    result.UserEmail,
                    Faces = faces,
                    result.PictureUrl
                });
            }
        }

        private async Task<Tuple<List<byte[]>, Guid>> GetFacesFromFaceApiAsync(byte[] imageData, Guid orderId)
        {
            Tuple<List<byte[]>, Guid> orderDetailData = null;

            var byteContent = new ByteArrayContent(imageData);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.PostAsync($"{_settings.Value.FacesApiUrl}/api/faces?orderId={orderId}", byteContent))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    orderDetailData = JsonConvert.DeserializeObject<Tuple<List<byte[]>, Guid>>(apiResponse);
                }
            }

            return orderDetailData;
        }

        private void SaveOrderDetail(Guid orderId, List<byte[]> faces)
        {
            var order = _orderRepository.GetOrderAsync(orderId).Result;

            if (order != null)
            {
                order.Status = Status.Processed;
                foreach (var face in faces)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = orderId,
                        FaceData = face
                    };
                    order.OrderDetails.Add(orderDetail);
                }

                _orderRepository.UpdateOrder(order);
            }
        }

        private async Task SaveOrder(IRegisterOrderCommand result)
        {
            var order = new Order
            {
                OrderId = result.OrderId,
                UserEmail = result.UserEmail,
                Status = Status.Registered,
                PictureUrl = result.PictureUrl,
                ImageData = result.ImageData
            };

            await _orderRepository.RegisterOrder(order);
        }
    }
}