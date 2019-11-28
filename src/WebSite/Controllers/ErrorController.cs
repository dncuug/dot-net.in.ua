using System.Diagnostics;
using System.Net;
using Core.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace WebSite.Controllers
{
    [Route("error")]
    public class ErrorController : Controller
    {
        public ErrorController()
        {
        }

        [Route("info")]
        public ActionResult Info(int? code)
        {
            ViewData["Title"] = "Error";

            var statusCode = code ?? HttpContext.Response.StatusCode;
            
            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode,
                Description = GetDescription(statusCode)
            };
            
            return View(model);
        }

        private static string GetDescription(int code)
        {
            switch (code)
            {
                case (int) HttpStatusCode.NotFound:
                    return "Page not found";
                default:
                    return "System error";
            }
        }
    }
}