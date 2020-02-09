using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Web.WebPages.OAuth;

namespace OpenAuth.App_Start
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            //OAuthWebSecurity.RegisterTwitterClient(
            //    consumerKey: "",
            //    consumerSecret: "");

            OAuthWebSecurity.RegisterFacebookClient(
                appId: "645145318865406",
               appSecret: "918d5ededf8f9bc4f68b8276c9871e32");

            OAuthWebSecurity.RegisterGoogleClient();

            //OAuthWebSecurity.RegisterLinkedInClient(
            //    consumerKey: "",
            //    consumerSecret: "");

            //OAuthWebSecurity.RegisterYahooClient();
        }
    }
}