using Marvin.IDP.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Marvin.IDP.Services
{
    public interface IApplicationAccountRepository
    {
        IEnumerable<Edge_Account> GetAccounts();
        IEnumerable<Edge_UserAccount> GetUserAccounts();
        IEnumerable<Edge_Client> GetClients();
        IEnumerable<Edge_ClientUser> GetClientUsers();
    }
}
