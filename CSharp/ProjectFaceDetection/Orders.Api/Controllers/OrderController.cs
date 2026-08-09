using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orders.Api.Persistence;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{

    private readonly ILogger<OrderController> _logger;
    private readonly IOrderRepository _orderRepository;

    public OrderController(ILogger<OrderController> logger, IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var data = await _orderRepository.GetOrdersAsync();

        return Ok(data);
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrderById(string orderId)
    {
        var order = await _orderRepository.GetOrderAsync(Guid.Parse(orderId));

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }
}