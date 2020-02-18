using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Pocket.Common;
using Pocket.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Pocket.Controllers
{

    public class AccountController : ApplicationController
    {
        
        public AccountController(ApplicationDbContext context):base(context)
            
        {
            //UserManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));
        }

        /*public AccountController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }*/

       // public UserManager<ApplicationUser> UserManager { get; private set; }
         
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

      
        private string GenerateAccessToken(ApplicationUser user, bool? RememberMe)
        {

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };
            var claimsIdentity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);
            var identity = new ClaimsPrincipal(claimsIdentity);



            AuthenticationTicket ticket = new AuthenticationTicket(identity, CookieAuthenticationDefaults.AuthenticationScheme);
            var currentUtc = new SystemClock().UtcNow;
            ticket.Properties.IssuedUtc = currentUtc;
            ticket.Properties.ExpiresUtc = currentUtc.Add(RememberMe.HasValue && RememberMe.Value?TimeSpan.FromDays(30): TimeSpan.FromDays(1));

            return AuthenticateResult.Success(ticket).ToString();
            //var accessToken = OAuthBearerOptions.AccessTokenFormat.Protect(ticket);
            //return accessToken;
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
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
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

    }
}
