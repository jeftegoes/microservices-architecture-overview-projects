using System;
using System.Threading.Tasks;
using MassTransit;
using Messaging.Interfaces.SharedLib.Events;
using Microsoft.AspNetCore.SignalR;
using Orders.Api.Entities;
using Orders.Api.Hubs;
using Orders.Api.Persistence;

namespace Orders.Api.Messages.Consumers
{
    public class OrderDispatchedEventConsumer : IConsumer<IOrderDispatchedEvent>
    {

        private readonly IOrderRepository _orderRepository;
        private readonly IHubContext<OrderHub> _hubContext;

        public OrderDispatchedEventConsumer(IOrderRepository orderRepository,
                                            IHubContext<OrderHub> hubContext)
        {
            _hubContext = hubContext;
            _orderRepository = orderRepository;
        }

        public async Task Consume(ConsumeContext<IOrderDispatchedEvent> context)
        {
            var message = context.Message;
            var orderId = message.OrderId;

            UpdateDatabase(orderId);

            await _hubContext.Clients.All.SendAsync("UpdateOrders", "Order dispatched", orderId);
        }

        private void UpdateDatabase(Guid orderId)
        {
            var order = _orderRepository.GetOrder(orderId);

            if (order != null)
            {
                order.Status = Status.Sent;
                _orderRepository.UpdateOrder(order);
            }
        }
    }
}