using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;

namespace EventManagement.Service
{
    public interface IEmailSettingService : IService<EmailSetting, Guid>
    {

    }
}
