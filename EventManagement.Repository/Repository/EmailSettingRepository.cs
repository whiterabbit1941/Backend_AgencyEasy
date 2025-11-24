using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class EmailSettingRepository : Repository<EmailSetting, Guid>, IEmailSettingRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<EmailSetting> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public EmailSettingRepository(EventManagementContext context, ILogger<EmailSettingRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<EmailSetting>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<EmailSetting> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
