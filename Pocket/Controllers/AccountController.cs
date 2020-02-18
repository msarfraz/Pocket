using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Pocket.Models;
using Microsoft.Owin;
using Postal;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Helpers;
using Pocket.Common;
using Microsoft.Owin.Infrastructure;
using System.Net;
using Pocket.Extensions;

namespace Pocket.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public AccountController()
            : this(new ApplicationUserManager(new UserStore<ApplicationUser>(new QDbContext())))
        {
           
        }

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public UserManager<ApplicationUser> UserManager { get; private set; }
         
        [HttpPost]
        [AllowAnonymous]
        public JsonResult MAutoLogin()
        {
            return Util.Package<JsonResult>(new object[]{new
            {
                success = User.Identity.IsAuthenticated,
                message = ""
            }});
        }

        [HttpPost]
        [AllowAnonymous]
        [ActionName("MobileLogin")]
        public async Task<JsonResult> MobileLogin(string UserName, string Password, bool? RememberMe)
        {
            var errorMessage = string.Empty;
            var accessToken = string.Empty;
            var success = false;

            var user = UserManager.Find(UserName, Password);
           
            if (user != null)
            {
                if (!UserManager.IsEmailConfirmed(user.Id)) 
                {
                    await SendEmailConfirmationTokenAsync(user.Id);

                    errorMessage = "You must have a confirmed email to log on. "
                                         + "An email with login instructions has been sent to your email account."
                                        + " Please follow the instructions and login again.";
                }
                else
                {
                    accessToken = GenerateAccessToken(user, RememberMe);
                    success = true;
                }
                
            }
            else
            {
                errorMessage = "Invalid Login or Password.";
            }
            return Util.Package<JsonResult>(new object[]{new
            {
                UserName = UserName,
                AccessToken = accessToken,
                success = success,
                message = errorMessage
            }});
        }

        private string GenerateAccessToken(ApplicationUser user, bool? RememberMe)
        {

            var identity = new ClaimsIdentity(Startup.OAuthBearerOptions.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
            identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));

            AuthenticationTicket ticket = new AuthenticationTicket(identity, new AuthenticationProperties());
            var currentUtc = new SystemClock().UtcNow;
            ticket.Properties.IssuedUtc = currentUtc;
            ticket.Properties.ExpiresUtc = currentUtc.Add(RememberMe.HasValue && RememberMe.Value?TimeSpan.FromDays(30): TimeSpan.FromDays(1));

            var accessToken = Startup.OAuthBearerOptions.AccessTokenFormat.Protect(ticket);
            return accessToken;
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindAsync(model.UserName, model.Password);
                if (user != null)
                {
                    if (!await UserManager.IsEmailConfirmedAsync(user.Id))
                    {
                        string callbackUrl = await SendEmailConfirmationTokenAsync(user.Id);

                        var errorMessage = "You must have a confirmed email to log on. "
                                             + "An email with login instructions has been sent to your email account."
                                            + " Please follow the instructions and login again.";
                        ModelState.AddModelError("", errorMessage);
                    }
                    else
                    {
                        await SignInAsync(user, model.RememberMe);
                        return RedirectToAction("Default", "Home"); // RedirectToLocal(returnUrl);
                    }
                    
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }
        //
        // POST: /Account/Register
        //[HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> MRegister(RegisterViewModel model)
        {
            var errorMessage = string.Empty;
            if (ModelState.IsValid)
            {
                if (UserManager.FindByEmail(model.Email) != null)
                {
                    errorMessage = "Email already registered.";
                }
                else
                {
                    var user = new ApplicationUser() { UserName = model.UserName, Email = model.Email, EmailConfirmationSent = DateTime.Today.AddDays(-2) };
                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        await SendEmailConfirmationTokenAsync(user.Id);
                        Global.CreateUserDefaults(user.Id);
                        var accessToken = GenerateAccessToken(user, false);
                        return Util.Package<JsonResult>(new object[]{new
                        {
                            UserName = user.UserName,
                            AccessToken = accessToken,
                            success = true,
                            message = "Congratulations!\r\nYour account is registered. You must confirm your email address to login. Please look for an email in your inbox that provides further instructions."
                        }});
                    }
                    else
                    {
                        foreach (var item in result.Errors)
                        {
                            errorMessage += item;
                        }
                    }
                }
                
            }
            else
            {
                var errorList = ModelState.Values.SelectMany(m => m.Errors)
                                 .Select(e => e.ErrorMessage)
                                 .ToList();
                foreach (var item in errorList)
                {
                    errorMessage += item;
                }
            }
            return Util.Package<JsonResult>(new object[]{new
                        {
                            success = false,
                            message = errorMessage
                        }});
        }
        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if(UserManager.FindByEmail(model.Email) != null)
                {
                    ModelState.AddModelError("Email", "Email already registered.");
                    return View(model);
                }
                var user = new ApplicationUser() { UserName = model.UserName, Email = model.Email, EmailConfirmationSent = DateTime.Today.AddDays(-2) };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SendEmailConfirmationTokenAsync(user.Id);
                    ViewBag.UserID = user.Id;
                    return View("RegisterStepTwo");

                    return RedirectToAction("Default", "Home");
                }
                else
                {
                    AddErrors(result);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult RegisterStepTwo()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterStepTwo(string userID)
        {
            await SendEmailConfirmationTokenAsync(userID);
            ViewBag.errorMessage = "An email with registration instructions is resent.";
            ViewBag.UserID = userID;
            return View();
        }

        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            IdentityResult result;
            var emailAlreadyConfirmed = false;
            
            try
            {
                if (await UserManager.IsEmailConfirmedAsync(userId))
                {
                    emailAlreadyConfirmed = true;
                }
                result = await UserManager.ConfirmEmailAsync(userId, code);
            }
            catch (InvalidOperationException ioe)
            {
                // ConfirmEmailAsync throws when the userId is not found.
                ViewBag.errorMessage = ioe.Message;
                return View("Error");
            }

            if (result.Succeeded)
            {
                if (!emailAlreadyConfirmed)
                 Global.CreateUserDefaults(userId);
                return View("ConfirmationSuccess");

            }
            
            // If we got this far, something failed.
            AddErrors(result);
            ViewBag.errorMessage = "Email Confirmation failed.";
            return View("Error");
        }
        private async Task<string> SendEmailConfirmationTokenAsync(string userID)
        {
            var callbackUrl = string.Empty;
            ApplicationUser user = UserManager.FindById(userID); 
            
            if (!await UserManager.IsEmailConfirmedAsync(userID) && user.EmailConfirmationSent.Date < DateTime.Today)
            {
                string code = await UserManager.GenerateEmailConfirmationTokenAsync(userID);
                callbackUrl = Url.Action("ConfirmEmail", "Account",
                   new { userId = userID, code = code }, protocol: Request.Url.Scheme);
                

                string emailBody = string.Format( "Dear {0}, <br><br>Thanks for registering an account with us. Please confirm your account by clicking <a href=\"{1}\">here.</a>" + 
                                        "<br/><br/>Regards<br/>" +
                                         "{2} Team", user.UserName, callbackUrl, Global.ApplicationName);
                await UserManager.SendEmailAsync(userID, "Welcome to " + Global.ApplicationName,emailBody );
                user.EmailConfirmationSent = DateTime.Now;
                await UserManager.UpdateAsync(user);
            }

            return callbackUrl;
        }

        [AllowAnonymous]
        public ActionResult ConfirmationSuccess()
        {
            return View();
        }

        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            ViewBag.HasLocalPassword = HasPassword();
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then prompt the user to create an account
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName, Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        //
        // GET: /Account/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            if (result.Succeeded)
            {
                return RedirectToAction("Manage");
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser()
                {
                    UserName = model.UserName,
                    Email = model.Email,
                   
                };

                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        //Global.CreateUserDefaults(user.Id);
                        //await SignInAsync(user, isPersistent: false);
                        //return RedirectToLocal(returnUrl);
                        ViewBag.UserID = user.Id;
                        return View("RegisterStepTwo");
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }
        
        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
                UserManager = null;
            }
            base.Dispose(disposing);
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

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Default", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
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
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
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
