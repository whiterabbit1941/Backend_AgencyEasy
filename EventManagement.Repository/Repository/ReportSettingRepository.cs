using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class ReportSettingRepository : Repository<ReportSetting, Guid>, IReportSettingRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<ReportSetting> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public ReportSettingRepository(EventManagementContext context, ILogger<ReportSettingRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<ReportSetting>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<ReportSetting> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
