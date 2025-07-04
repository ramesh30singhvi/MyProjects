using Microsoft.Extensions.Configuration;
using RestSharp;
using SHARP.BusinessLogic.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SHARP.BusinessLogic.Services
{
    public class TableauServerService : ITableauServerService
    {
        private readonly IConfiguration _configuration;

        protected RestClient _client;

        public TableauServerService(IConfiguration configuration)
        {
            _configuration = configuration;

            string tableauServerUri = _configuration["TableauServer:BaseUri"] ?? string.Empty;
            tableauServerUri = tableauServerUri.TrimEnd('/');

            _client = new RestClient(tableauServerUri);
        }

        public async Task<string> GetTableauAuthenticationTicketAsync()
        {
            RestRequest request = CreateObtainTicketRequest();

            var result = await _client.ExecuteAsync(request);

            if (result.IsSuccessful)
            {
                return result.Content;
            }

            throw new Exception(result.ErrorException.Message);
        }

        private RestRequest CreateObtainTicketRequest()
        {
            string userName = _configuration["TableauServer:UserName"];
            string targetSite = _configuration["TableauServer:TargetSiteName"];
            
            var request = new RestRequest("trusted", Method.POST);
            request.AddQueryParameter("username", userName);

            if (!string.IsNullOrEmpty(targetSite))
            {
                request.AddQueryParameter("target_site", targetSite);
            }

            return request;
        }
    }
}
