using System;
using EventManagement.Domain;
using FinanaceManagement.API.Models;

namespace EventManagement.Domain
{
    public interface IAspUserRepository : IRepository<AspNetUsers, string>
    {

    }
}
