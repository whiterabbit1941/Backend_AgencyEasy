using System;
using System.Threading.Tasks;
using EventManagement.Service;
using EventManagement.Domain.Entities;
using FinanaceManagement.API.Models;
using EventManagement.Dto;

namespace EventManagement.Service
{
    public interface IAspUserService : IService<AspNetUsers, string>
    {

        AspUserDto GetUserDetails(string userId);

        string GetHash(string password);

        string GeneratePassword(bool useLowercase, bool useUppercase, bool useNumbers, bool useSpecial,
        int passwordSize);

        Task<int> UpdateUserPartially(string id);

        Task RegisterUserInGoHighLevel(string emailid, string fname, string lname, string cname, string tag);

        Task<bool> SendEmailToSuperAdminsForAppsumoSignup(string FName, string LName, string Email, string Password, string CompanyName, string DashboardLogo);

        int GetProjectCreatedCount(Guid companyID);

        int GetSerpCreatedCount(Guid companyID);

        Task<bool> CreateUserInMailchimp(string email, string fname, string lname, string companyName, string tagData);

    }
}
