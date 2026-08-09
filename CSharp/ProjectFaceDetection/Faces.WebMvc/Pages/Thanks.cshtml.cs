using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Faces.WebMvc.Pages;

public class ThanksModel : PageModel
{
    [BindProperty]
    public Guid OrderId { get; set; }

    public ThanksModel()
    {

    }

    public void OnGet(Guid orderId)
    {
        OrderId = Guid.Parse(Request.Query["OrderId"]);
    }
}
