using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using FinanaceManagement.API.Models;

namespace EventManagement.Service
{
    public interface IClientPostLogoutRedirectUriService : IService<ClientPostLogoutRedirectUris, int>
    {

    }
}
