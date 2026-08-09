using System.ComponentModel.DataAnnotations;

namespace Faces.WebMvc.ViewModels
{
    public class OrderViewModel
    {
        public Guid OrderId { get; set; }

        [Display(Name = "Email")]
        public string UserEmail { get; set; } = string.Empty;

        [Display(Name = "Image file")]
        public IFormFile File { get; set; }

        public string PictureUrl { get; set; } = string.Empty;
        public int Status { get; set; }
        public byte[] ImageData { get; set; }
        public string ImageString { get; set; } = string.Empty;
        public List<OrderDetailViewModel> OrderDetails { get; set; }
    }
}