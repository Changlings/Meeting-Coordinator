using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MeetingCoordinator.Models;
using Newtonsoft.Json.Linq;

using System.Security.Cryptography;
using System.Text;

namespace MeetingCoordinator.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private IAuthenticationManager Authentication => HttpContext.GetOwinContext().Authentication;
        public AccountController()
        {
        }

        //
        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (Authentication.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        //this will only be called if the username and password fields are not empty when submit button is pressed
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginViewModel model)
        {
            Attendee attendee = null;
            try
            {
               attendee = db.Attendees.First(u => u.Username == model.Username);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "No User by that name");
                return View(model);
            }

            //if the attendee user name is found
            if (attendee.Username == model.Username)
            {
                //get the attendees hashed password
                String hashedPassword = hashSha256(model.Password);
                if (attendee.Password == hashedPassword)
                {
                    
                    var identity = new ClaimsIdentity(
                    new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, $"{attendee.ID}"),
                            new Claim(ClaimTypes.Role, "attendee"), 
                        }, 
                        DefaultAuthenticationTypes.ApplicationCookie,
                        ClaimTypes.NameIdentifier, ClaimTypes.Role
                    );

                    Authentication.SignIn(new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe
                    }, identity);

                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login credentials");
            return View(model);
        }

        [Authorize]
        public ActionResult Logout()
        {
          AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
          return RedirectToAction("Login");
        }

        public String hashSha256(String password)
        {
            SHA256Managed crypt = new SHA256Managed();
            System.Text.StringBuilder hash = new System.Text.StringBuilder();

            //TODO: people were reporting problems with using ASCII as the encoding scheme, but I don't know enough to say that UTF8 is the correct choice for us
            byte[] hashBytes = crypt.ComputeHash(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetByteCount(password));

            foreach (byte b in hashBytes)
            {
                //each byte as two uppercase hex characters
                hash.Append(b.ToString("x2"));
            }

            return hash.ToString();
        }


       
        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}