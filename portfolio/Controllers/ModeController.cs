using Microsoft.AspNetCore.Mvc;

namespace portfolio.Controllers;

public class ModeController : Controller
{
    // POST /Mode/Toggle : switch between viewer and editor.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Toggle(string? returnUrl)
    {
        if (HttpContext.IsEditor())
        {
            HttpContext.Session.Remove("mode");
        }
        else
        {
            HttpContext.Session.SetString("mode", "editor");
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }
        return RedirectToAction("Index", "Home");
    }
}