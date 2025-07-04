using Blazored.LocalStorage;
using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Client.ViewModels;
using CellarPassAppAdmin.Shared.Enums;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class ApiManager : IApiManager
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly IMessageViewModel _messageViewModel;

        public ApiManager(HttpClient httpClient, ILocalStorageService localStorage, IMessageViewModel messageViewModel)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _messageViewModel = messageViewModel;
        }

        public async Task<T> GetAsync<T>(string uri)
        {
            return await CallAPI<T>(HttpMethod.Get, uri, null);
        }

        public async Task<T> PostAsync<T>(string uri, T data)
        {
            return await CallAPI<T>(HttpMethod.Post, uri, data);
        }

        public async Task<TR> PostAsync<T, TR>(string uri, T data)
        {
            return await CallAPI<TR>(HttpMethod.Post, uri, data);
        }

        public async Task<T> PostAsync<T>(string uri, object data)
        {
            return await CallAPI<T>(HttpMethod.Post, uri, data);
        }

        public async Task<T> PutAsync<T>(string uri, T data)
        {
            return await CallAPI<T>(HttpMethod.Put, uri, data);
        }

        public async Task<R> PutAsync<T, R>(string uri, T data)
        {
            return await CallAPI<R>(HttpMethod.Put, uri, data);
        }

        private async Task<T> CallAPI<T>(HttpMethod method, string uri, object data)
        {
            try
            {
                string jsonResult = string.Empty;
                var req = new HttpRequestMessage(method, uri);
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (data != null)
                {
                    req.Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
                }
                string token = await _localStorage.GetItemAsStringAsync("token");
                if (!string.IsNullOrEmpty(token))
                {
                    token = token.Replace("\"", string.Empty);
                    req.Headers.Add("Authorization", $"Bearer {token}");
                }
                var response = await _httpClient.SendAsync(req);
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    };
                    jsonResult = await response.Content.ReadAsStringAsync();
                    var json = JsonSerializer.Deserialize<T>(jsonResult, options);
                    return json;
                }
                else if(response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _messageViewModel.SendMessage(LoginStatus.NoPermission.ToString());
                }
                throw new HttpRequestExceptionEx(response.StatusCode, jsonResult);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.GetType().Name} : {e.Message}");
                throw;
            }
        }

        public async Task<T> DeleteAsync<T>(string uri)
        {
            return await CallAPI<T>(HttpMethod.Delete, uri, null);
            //return await _httpClient.DeleteAsync(GetUri(uri));
        }

        public async Task<R> DeleteAsync<T, R>(string uri, T data)
        {
            return await CallAPI<R>(HttpMethod.Delete, uri, data);
        }


        private string GetUri(string uri) => $"{uri}";
    }
}
