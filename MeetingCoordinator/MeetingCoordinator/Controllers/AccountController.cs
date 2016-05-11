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
        /// <summary>
        /// The database connection context for this controller. This is used to interact
        /// with the database via Entity Framework
        /// </summary>
        private ApplicationDbContext db = new ApplicationDbContext();
        /// <summary>
        /// Initialize the OWIN library to handle authentication
        /// </summary>
        private IAuthenticationManager Authentication => HttpContext.GetOwinContext().Authentication;

        /// <summary>
        /// Displays the login page. Also prevents already logged in users
        /// from seeing the page.
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            // Prevent already signed-in users from attempting to sign in 
            // again by redirecting them to the /Home/Index route
            if (Authentication.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            // Display the Login page
            return View();
        }

        /// <summary>
        /// This will only be called if the username and password fields are not empty when submit button is pressed
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginViewModel model)
        {
            Attendee attendee = null;
            try
            {
                attendee = db.Attendees.First(u => u.Username == model.Username);
            }
            // Entity Framework will throw an exception on failing to find a record with the 
            // First method. If that happens, there was not a user located and the view needs
            // to be updated accordingly to show that
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "No User by that name");
                return View(model);
            }

            //if the attendee user name is found
            if (attendee.Username == model.Username)
            {
                //get the attendees hashed password and compare the hashed results
                // since we don't store plain text (non-hashed) passwords 
                // in the database
                if (attendee.Password == hashSha256(model.Password))
                {
                    // Initialize the OWIN managed session handler for this user
                    var identity = new ClaimsIdentity(
                    new[]
                        {
                            // User.Identity.Name will be the string representation of the attendee's ID
                            new Claim(ClaimTypes.NameIdentifier, $"{attendee.ID}"),
                            new Claim(ClaimTypes.Role, "attendee"),
                        },
                        // Persist this authentication as a cookie on the client
                        DefaultAuthenticationTypes.ApplicationCookie,
                        ClaimTypes.NameIdentifier, ClaimTypes.Role
                    );
                    // "Sign in" the user into the OWIN context to have their 
                    // Identity persist and be accessible throughout the application
                    Authentication.SignIn(new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe
                    }, identity);
                    // Redirect the browser to the home page
                    return RedirectToAction("Index", "Home");
                }
            }
            // This message is shown if the username is found but 
            // the password was incorrect
            ModelState.AddModelError(string.Empty, "Invalid login credentials");
            return View(model);
        }

        /// <summary>
        /// Log out the currently logged in user and redirect them to the login page
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult Logout()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Utilty function to apply the SHA 256 hashing algorithm to incoming
        /// passwords.
        /// </summary>
        /// <param name="password">The password string to hash</param>
        /// <returns>The unique SHA 256 hash of the password</returns>
        public string hashSha256(string password)
        {
            var crypt = new SHA256Managed();
            var hash = new System.Text.StringBuilder();

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