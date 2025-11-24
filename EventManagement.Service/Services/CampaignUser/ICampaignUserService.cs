using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;

namespace EventManagement.Service
{
    public interface ICampaignUserService : IService<CampaignUser, Guid>
    {
        dynamic GetUserbySubjectID(string sID);
        dynamic GetUserbyEmailID(string email);
    }
}
