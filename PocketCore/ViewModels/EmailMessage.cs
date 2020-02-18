namespace Pocket.ViewModels
{
    public class EmailMessage: Aspose.Email.MailMessage
    {
        public EmailMessage(string viewName)
            : base()
        {
            this.viewName = viewName;
        }
        public string viewName { get; set; }
        
        public string CallbackURL { get; set; }
        
    }
}