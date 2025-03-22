using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace jekirdekCase.Controllers;

[Route("Account")]
public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        // Kullanıcı zaten giriş yaptıysa, ana sayfaya yönlendir
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home"); // Burada ana sayfaya yönlendiriyorsunuz
        }
    
        return View();
    }
    
    [HttpGet("Logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Account/Login");  // Çıkış sonrası Login sayfasına yönlendir
    }

}