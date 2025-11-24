using System;
using EventManagement.Domain.Entities;

namespace EventManagement.Domain
{
    public interface ICompanyUserRepository : IRepository<CompanyUser, Guid>
    {

    }
}
