using Newtonsoft.Json.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Linq;
using System.Threading.Tasks;
using SHARP.BusinessLogic.Interfaces;
using System.Collections.Generic;
using SHARP.BusinessLogic.DTO.Report;
using System;
using System.Net.Mime;

namespace SHARP.BusinessLogic.Services
{
    public class EmailClient : IEmailClient
    {
        IPortalReportService _portalReportService;
        public EmailClient(IPortalReportService portalReportService) {
            _portalReportService = portalReportService;
        }
        public async Task<Response> SendNeedHelpEmailAsync(string apiKey, string[] emails,string emailFrom, string emailUser, string name, string messageContent)
        {
            var client = new SendGridClient(apiKey);
            var id = await GetIdByNameAsync(client, "PORTAL_NEED_HELP");
            var message = new SendGridMessage();
            message.SetTemplateId(id);
            message.SetTemplateData(new
            {
                messageContent = messageContent,
                Name = name,
                email = emailUser,
            });

            IList<EmailAddress> emailAddresses = new List<EmailAddress>();

            foreach (var email in emails)
            {
                if (!string.IsNullOrEmpty(email))
                    emailAddresses.Add(new EmailAddress(email));
            }

            message.AddTos(emailAddresses.ToList());
            message.SetFrom(emailFrom);
            var repo = await client.SendEmailAsync(message);
            return repo;
        }

        public async Task<Response> SendOTPAsync(string templateName, string apiKey, string from, string email, string otp, string url)
        {
            var client = new SendGridClient(apiKey);
            var id = await GetIdByNameAsync(client, templateName );

            var message = new SendGridMessage();
            message.SetTemplateId(id);
            message.SetTemplateData(new
            {
                code = otp,
                url
            });
            message.SetFrom(from);
            message.AddTo(email);
            
            return await client.SendEmailAsync(message);
        }

        public async Task<Response> SendReportsAsync(string apiKey,string subject ,string[] ccEmails, string from, IList<string> emails,  string url, string urlWithParams, SendReportDto sendReportDto,string messageFromACS)
        {
            var client = new SendGridClient(apiKey);
            var id = await GetIdByNameAsync(client, "PORTAL_REPORTS");

            var message = new SendGridMessage();
            message.SetTemplateId(id);
            message.SetTemplateData(new
            {
                url,
                facility = sendReportDto.Facility?.Name,
                reports = sendReportDto.Reports.Select(x => x.Name),
                messageContent = messageFromACS,
                urlWithParams = urlWithParams,
                subject = subject,
            });
            IList<EmailAddress> emailAddresses = new List<EmailAddress>();

            foreach (var email in emails)
            {
                if( !string.IsNullOrEmpty(email) )
                    emailAddresses.Add(new EmailAddress(email));
            }

            message.AddTos(emailAddresses.ToList());

            if(string.IsNullOrEmpty(sendReportDto.Organization?.OperatorName))
                message.SetFrom(from);
            else
                message.SetFrom(from, sendReportDto.Organization?.OperatorName);



            IList<EmailAddress> emailCCAddresses = new List<EmailAddress>();
            foreach (var email in ccEmails)
            {
                if(!emailAddresses.Any( x => x.Email == email) && !string.IsNullOrEmpty(email))
                   emailCCAddresses.Add(new EmailAddress(email));
            }
            if (emailCCAddresses.Any())
                message.AddCcs(emailCCAddresses.ToList());

            if(sendReportDto.Organization.AttachPortalReport)
                await AttachToTheMessage(message,sendReportDto.Reports);
   
            var repo = await client.SendEmailAsync(message);
            return repo;
        }

        private async Task AttachToTheMessage(SendGridMessage message, IReadOnlyCollection<PortalReportDto> reports)
        {
            foreach (var report in reports)
            {
                try
                {
                    var result = await  _portalReportService.DownloadPortalReportForAttachment(report.Id);
                    if (result != null)
                    {
                        message.AddAttachment(new Attachment
                        {
                            Content = Convert.ToBase64String(result.Item1),
                            Filename = report.Name,
                            Type = result.Item2,
                            Disposition = DispositionTypeNames.Attachment
                        });
                    }
                }catch(Exception ex)
                {

                }
            }
        }

        public async Task<Response> SendUserInvitationAsync(string apiKey, string emailFrom, string email,
            string password, string portalUrl,string urlWithParam,string organization)
        {

            var client = new SendGridClient(apiKey);
            var id = await GetIdByNameAsync(client, "PORTAL_USER_INVITATION");

            var message = new SendGridMessage();
            message.SetTemplateId(id);
            message.SetTemplateData(new
            {
                email = email,
                organization = organization,
                portal_url = portalUrl,
                url = urlWithParam,
                password = password,


            });
            message.SetFrom(emailFrom);
            message.AddTo(email);

            return await client.SendEmailAsync(message);
        }

        public async Task<Response> SendUserInvitationWithReportsAsync(string apiKey, string from, string email, string password, string url, string urlWithParam, SendReportDto sendReportDto)
        {
            var client = new SendGridClient(apiKey);
            var id = await GetIdByNameAsync(client, "PORTAL_INVITATION_WITH_REPORTS");

            var message = new SendGridMessage();
            message.SetTemplateId(id);
            message.SetTemplateData(new
            {
                facility = sendReportDto.Facility?.Name,
                reports = sendReportDto.Reports.Select( x => x.Name),
                email = email,
                organization = sendReportDto.Organization?.Name,
                portal_url = url,
                url = urlWithParam,
                password = password,
            });
            message.SetFrom(from);
            message.AddTo(email);
            if (sendReportDto.Organization.AttachPortalReport)
                await AttachToTheMessage(message, sendReportDto.Reports);

            var repo = await client.SendEmailAsync(message);
            return repo;
        }

        public async Task<Response> SendUserResetPasswordAsync(string apiKey, string emailFrom, string email, string urlWithParam)
        {
            var client = new SendGridClient(apiKey);
            var id = await GetIdByNameAsync(client, "PORTAL_USER_RESET_PASSWORD");

            var message = new SendGridMessage();
            message.SetTemplateId(id);
            message.SetTemplateData(new
            {
                email = email,
                url = urlWithParam,
            });
            message.SetFrom(emailFrom);
            message.AddTo(email);

            return await client.SendEmailAsync(message);
        }

        private async Task<string> GetIdByNameAsync(SendGridClient client, string name)
        {
            var templatesRaw = await client.RequestAsync(
                BaseClient.Method.GET,
                null,
                "{'page_size': 200, 'generations': 'dynamic'}",
                "templates");
            var templatesJson = await templatesRaw.DeserializeResponseBodyAsync(templatesRaw.Body);
            var templates = (JArray)templatesJson["result"];

            var template = templates.Single(templ => templ["name"].ToString() == name);
            return template["id"].ToString();
        }
    }
}
