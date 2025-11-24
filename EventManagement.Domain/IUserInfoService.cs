using System;

namespace EventManagement.Domain
{
    public interface IUserInfoService
    {
        string UserId { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Role { get; }
        string SelectedCompanyId { get; set; }
        Boolean SuperAdmin { get; set; }
        string CustomDomain { get; set; }

    }
}
