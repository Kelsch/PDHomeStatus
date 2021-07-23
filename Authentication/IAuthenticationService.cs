using DataAccessLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PDSalesSchedule.Authentication
{
    public interface IAuthenticationService
    {
        Task<HttpResponseModel> Login(AuthenticationUserModel user);
        Task Logout();
    }
}
