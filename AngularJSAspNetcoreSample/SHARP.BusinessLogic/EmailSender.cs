using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic
{
    public class EmailSender
    {
        private string _apiKey;
        public EmailSender(string apiKey)
        {
            this._apiKey = apiKey;
        }
        public async Task Send(string from, string to, string subject, string fullName, string htmlContent)
        {
            var client = new SendGridClient(this._apiKey);
            var fromEmailAddress = new EmailAddress(from, "SHARP noreply");
            var toEmailAddress = new EmailAddress(to, fullName);
            var msg = MailHelper.CreateSingleEmail(fromEmailAddress, toEmailAddress, subject, htmlContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
          
        }
    }
}
