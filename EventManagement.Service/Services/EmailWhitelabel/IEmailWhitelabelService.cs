using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;

namespace EventManagement.Service
{
    public interface IEmailWhitelabelService : IService<EmailWhitelabel, Guid>
    {
        Task<int> DeleteEmailByCompanyID(Guid companyId);
    }
}
