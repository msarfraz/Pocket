using Microsoft.AspNet.Identity;
using Pocket.Common;
using Pocket.ViewModels;
using Postal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace Pocket.App_Start
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            /*var email = new EmailMessage("RegEmail");
            email.To = message.Destination;
            email.Subject = message.Subject;
            email.CallbackURL = message.Body;
            return email.SendAsync();*/
            //email.Send();
            
            // Credentials:
            var credentialUserName = "admin@xpertbudget.com";
            var sentFrom = "support@xpertbudget.com";
            var pwd = "Remember#81";

            // Configure the client:
            System.Net.Mail.SmtpClient client =
                new System.Net.Mail.SmtpClient();
            client.Host = "smtp.office365.com";
            client.Port = 587;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;

            // Creatte the credentials:
            System.Net.NetworkCredential credentials =
                new System.Net.NetworkCredential(credentialUserName, pwd);

            client.EnableSsl = true;
            client.Credentials = credentials;

            // Create the message:
            var mail =
                new System.Net.Mail.MailMessage(Global.FromAddress, message.Destination, message.Subject, message.Body);
            
            mail.IsBodyHtml = true;
            // Send:
            return client.SendMailAsync(mail);
        }
    }
}