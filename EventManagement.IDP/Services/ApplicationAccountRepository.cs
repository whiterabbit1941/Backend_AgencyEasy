using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Marvin.IDP.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Marvin.IDP.Services
{
    public class ApplicationAccountRepository : IApplicationAccountRepository
    {
        ApplicationAccountDbContext _context;

        public ApplicationAccountRepository(ApplicationAccountDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Edge_Account> GetAccounts()
        {
            var accountList =  _context.Edge_Accounts;

            return accountList;
        }

        public IEnumerable<Edge_UserAccount> GetUserAccounts()
        {
            var userAccountList = _context.Edge_UserAccounts;

            return userAccountList;
        }

        public IEnumerable<Edge_Client> GetClients()
        {
            var clientList = _context.Edge_Clients; ;

            return clientList;
        }

        public IEnumerable<Edge_ClientUser> GetClientUsers()
        {
            var clientUserList = _context.Edge_ClientUsers; ;

            return clientUserList;
        }

    }
}
