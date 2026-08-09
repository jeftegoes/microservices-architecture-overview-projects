using System;

namespace Orders.Api.Entities
{
    public class OrderDetail
    {
        public Guid OrderId { get; set; }
        public int OrderDetailId { get; set; }
        public byte[] FaceData { get; set; }
    }
}