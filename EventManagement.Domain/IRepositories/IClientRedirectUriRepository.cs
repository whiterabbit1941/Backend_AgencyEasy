using System;
using EventManagement.Domain.Entities;
using FinanaceManagement.API.Models;

namespace EventManagement.Domain
{
    public interface IClientRedirectUriRepository : IRepository<ClientRedirectUris, int>
    {

    }
}
