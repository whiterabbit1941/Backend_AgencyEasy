using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using EventManagement.Domain;
using System.Collections.Generic;

namespace EventManagement.Service
{
    public interface ICompanyService : IService<Company, Guid>
    {
        Task<CompanyDto> CreateCompany(string name, string type,string imageUrl,string subdomain,string dashboard,string fevicon,string extension);

        Task<CompanyUserDto> AddUserToCompany(string userId, string companyId, string role);


        CompanyDto GetCompany(string userId);

        Company GetCompanyDetailsByDomain(string domain);     

        Task<CompanyDto> UpdateCompanyPartially(Guid id, string requestedUrl);

        Task<CompanyDto> UpdateCompanyRowGuid(Guid id,Guid rowGuId);

        string ResizeImage(string imageUrl, int height, int width);
        Task<string> UploadImageToAws(Guid cid, string base64, string fileName);

        CustomDomainCompanyInfo GetCustomDomainCompanyInfo(string domain);

        CompanyDto GetCompanyByUserEmail(string email);

        List<SuperAdminDashboard> GetAdminDashboard(string userId,bool isAdmin);
    }
}

