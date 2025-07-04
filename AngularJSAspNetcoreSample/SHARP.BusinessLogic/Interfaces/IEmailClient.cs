using SendGrid;
using SHARP.BusinessLogic.DTO.Report;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic.Interfaces
{
    public interface IEmailClient
    {
        public Task<Response> SendNeedHelpEmailAsync(string apiKey, string[] emailsArray,string emailFrom, string email, string name, string message);
        public Task<Response> SendOTPAsync(string templateName,string apiKey, string from, string email, string otp, string url);
        public Task<Response> SendReportsAsync(string apiKey,string subject, string[] ccEmails, string from, IList<string> emails, string url, string urlWithParams, SendReportDto sendReportDto,string message);
        public Task<Response> SendUserInvitationAsync(string apiKey,string emailFrom, string email, string password, string url, string urlWithParam, string organization);
        public Task<Response> SendUserInvitationWithReportsAsync(string apiKey, string from, string email, string password, string url, string urlWithParam, SendReportDto sendReportDto);
        public Task<Response> SendUserResetPasswordAsync(string apiKey, string from, string v, string urlWithParam);
    }
}
