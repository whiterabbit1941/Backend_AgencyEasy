using System;
using EventManagement.Domain.Entities;

namespace EventManagement.Domain
{
    public interface ICompanyRepository : IRepository<Company, Guid>
    {

    }
}
