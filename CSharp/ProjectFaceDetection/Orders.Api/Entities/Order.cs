using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Orders.Api.Entities
{
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; }
        public string PictureUrl { get; set; } = string.Empty;
        public byte[] ImageData { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public Status Status { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }
}