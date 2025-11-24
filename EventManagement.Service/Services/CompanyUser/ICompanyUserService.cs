using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using System.Collections.Generic;

namespace EventManagement.Service
{
    public interface ICompanyUserService : IService<CompanyUser, Guid>
    {
        List<SuperAdminDashboard> GetAdminDashboard(bool IsSuperAdmin, string userId);
    }
}
