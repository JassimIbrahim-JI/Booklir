using Booklir.Core.Interfaces;
using Booklir.Core.RazorHelper;
using Booklir.ViewModels.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Booklir.Controllers
{
    public class NewsletterController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly IRazorViewToStringRenderer _razorRenderer;
        private readonly IConfiguration _config;

        public NewsletterController(
           IEmailSender emailSender,
           IRazorViewToStringRenderer razorRenderer,
           IConfiguration config)
        {
            _emailSender = emailSender;
            _razorRenderer = razorRenderer;
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe(string email)
        {
            // build the unsubscribe link however you like
            var unsubscribeUrl = Url.Action(
                "Unsubscribe",
                "Newsletter",
                new { email = WebUtility.UrlEncode(email) },
                Request.Scheme);

            var model = new NewsletterViewModel
            {
                LogoUrl = $"https://ik.imagekit.io/eg8pwa5pj/ChatGPT%20Image%20Jun%203,%202025,%2011_54_00%20PM.png?updatedAt=1749564575047",
                SubscribeUrl = unsubscribeUrl,  // or a different confirm link
                UnsubscribeUrl = unsubscribeUrl,
                CompanyName = _config["EmailSettings:FromName"]
            };

            // render the HTML
            var htmlBody = await _razorRenderer.RenderAsync("Email/Newsletter.cshtml", model);

            // send it!
            await _emailSender.SendEmailAsync(
              toEmail: email,
              subject: "Confirm your subscription to our newsletter",
              htmlContent: htmlBody
            );

            TempData["Message"] = "Please check your inbox to confirm your subscription.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Unsubscribe(string email)
        {
            // TODO: remove the email from your subscriber list
            ViewBag.Email = email;
            return View();  // thank‐you/unsubscribe‐success page
        }
    }
}
