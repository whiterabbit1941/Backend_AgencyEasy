using System;
using EventManagement.Domain.Entities;

namespace EventManagement.Domain
{
    public interface ITemplateSettingRepository : IRepository<TemplateSetting, Guid>
    {

    }
}
