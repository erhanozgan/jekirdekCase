using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Redirect("/Account/Login");
        }

        return Redirect("/customers/customers");
    }
}