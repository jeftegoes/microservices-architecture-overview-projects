using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Orders.Api.Entities;

namespace Orders.Api.Persistence
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderContext _context;

        public OrderRepository(OrderContext context)
        {
            _context = context;

        }

        public Order GetOrder(Guid id)
        {
            return _context.Order
                .Include(c => c.OrderDetails)
                .FirstOrDefault(c => c.OrderId == id);
        }

        public async Task<Order> GetOrderAsync(Guid id)
        {
            return await _context.Order
                .Include(c => c.OrderDetails)
                .FirstOrDefaultAsync(c => c.OrderId == id);
        }

        public async Task<IEnumerable<Order>> GetOrdersAsync()
        {
            return await _context.Order
                .Include(c => c.OrderDetails)
                .ToListAsync();
        }

        public Task RegisterOrder(Order order)
        {
            _context.Order.AddAsync(order);
            _context.SaveChanges();
            return Task.FromResult(true);
        }

        public void UpdateOrder(Order order)
        {
            _context.Entry(order).State = EntityState.Modified;
            _context.SaveChanges();
        }
    }
}