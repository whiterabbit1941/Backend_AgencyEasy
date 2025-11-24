using System;
using EventManagement.Domain.Entities;

namespace EventManagement.Domain
{
    public interface IEmailSettingRepository : IRepository<EmailSetting, Guid>
    {

    }
}
