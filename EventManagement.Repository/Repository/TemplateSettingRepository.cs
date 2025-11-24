using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class TemplateSettingRepository : Repository<TemplateSetting, Guid>, ITemplateSettingRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<TemplateSetting> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public TemplateSettingRepository(EventManagementContext context, ILogger<TemplateSettingRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<TemplateSetting>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<TemplateSetting> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
