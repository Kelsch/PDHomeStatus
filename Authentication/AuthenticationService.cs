using DataAccessLibrary.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using PDSalesSchedule.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace PDSalesSchedule.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly HttpClient _client;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILocalStorageService _localStorage;
        private readonly IConfiguration _config;
        private string authTokenStorageKey;

        public AuthenticationService(HttpClient client, AuthenticationStateProvider authStateProvider, ILocalStorageService localStorage, IConfiguration config)
        {
            _client = client;
            _authStateProvider = authStateProvider;
            _localStorage = localStorage;
            _config = config;
            authTokenStorageKey = _config["authTokenStorageKey"];
        }

        public async Task<HttpResponseModel> Login(AuthenticationUserModel userForAuthentication)
        {
            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", userForAuthentication.UserName),
                new KeyValuePair<string, string>("password", userForAuthentication.Password)
            });

            string apiLogin = _config["APIConnection"] + _config["loginEndPoint"];

            HttpResponseMessage authResult = null;
            HttpResponseModel resultMessage = new()
            {
                Success = false,
                Message = "Failure"
            };

            try
            {
                authResult = await _client.PostAsync(apiLogin, data);
                resultMessage.Message = "Sucess";
                resultMessage.Success = true;
            }
            catch (HttpRequestException e)
            {
                resultMessage.Message = e.InnerException == null ? e.Message.ToString() : e.InnerException.Message.ToString();
                resultMessage.Success = false;
                return resultMessage;
            }
            var authContent = await authResult.Content.ReadAsStringAsync();

            if (!authResult.IsSuccessStatusCode)
            {
                resultMessage.Message = authResult.ReasonPhrase;
                resultMessage.Success = false;
                return resultMessage;
            }

            var result = JsonSerializer.Deserialize<DataUserNoPasswordModel>(authContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            await _localStorage.SetItemAsync(authTokenStorageKey, result.Token);

            ((AuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.Token);

            resultMessage.Success = true;
            return resultMessage;
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync(authTokenStorageKey);
            ((AuthStateProvider)_authStateProvider).NotifyUserLogout();
            _client.DefaultRequestHeaders.Authorization = null;
        }
    }
}
