using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PDSalesSchedule.Services
{
    public interface IHttpService
    {
        Task<T> GetAsync<T>(string uri);
        Task<T> PostAsync<T>(string uri, T value, bool backgrounSync);
        Task<T> PutAsync<T>(string uri, T value, bool backgrounSync);
    }

    public class HttpService : IHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;
        private readonly ILocalStorageService _localStorageService;
        private readonly IConfiguration _config;

        public HttpService(
            HttpClient httpClient,
            NavigationManager navigationManager,
            ILocalStorageService localStorageService,
            IConfiguration config
        )
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;
            _localStorageService = localStorageService;
            _config = config;
        }

        public async Task<T> GetAsync<T>(string uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _config["APIConnection"] + uri);
            return await sendRequest<T>(request);
        }

        public async Task<T> PostAsync<T>(string uri, T value, bool backgrounSync)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _config["APIConnection"] + uri);
            request.Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
            request.Headers.Add("BackgroundSync", backgrounSync.ToString());
            return await sendRequest<T>(request);
        }

        public async Task<T> PutAsync<T>(string uri, T value, bool backgrounSync)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, _config["APIConnection"] + uri);
            request.Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
            request.Headers.Add("Background-Sync", backgrounSync.ToString());
            return await sendRequest<T>(request);
        }

        // helper methods
        private async Task<T> sendRequest<T>(HttpRequestMessage request)
        {
            // add jwt auth header if user is logged in and request is to the api url
            string authTokenStorageKey = _config["authTokenStorageKey"];
            var token = await _localStorageService.GetItemAsync<string>(authTokenStorageKey);
            var isApiUrl = !request.RequestUri.IsAbsoluteUri;
            if (!string.IsNullOrWhiteSpace(token) && isApiUrl)
                request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);

            using var response = await _httpClient.SendAsync(request);

            // auto logout on 401 response
            if (response.StatusCode == HttpStatusCode.Unauthorized || string.IsNullOrWhiteSpace(token))
            {
                _navigationManager.NavigateTo("logout");
                return default;
            }

            // throw exception on error response
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                throw new Exception(error["message"]);
            }

            return await response.Content.ReadFromJsonAsync<T>();
        }
    }
}
