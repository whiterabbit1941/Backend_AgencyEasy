using System;
using EventManagement.Domain.Entities;
using FinanaceManagement.API.Models;

namespace EventManagement.Domain
{
    public interface IClientPostLogoutRedirectUriRepository : IRepository<ClientPostLogoutRedirectUris, int>
    {

    }
}
