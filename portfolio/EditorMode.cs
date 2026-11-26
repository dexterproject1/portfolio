using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace portfolio;

// Reads the current mode from the session: true when the visitor switched to editor mode.
public static class EditorMode
{
    public static bool IsEditor(this HttpContext context)
    {
        return context.Session.GetString("mode") == "editor";
    }
}

// Put [EditorOnly] on an action to refuse visitors who are in viewer mode.
public class EditorOnlyAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.IsEditor())
        {
            context.Result = new RedirectToActionResult("Index", "Home", null);
        }
        base.OnActionExecuting(context);
    }
}