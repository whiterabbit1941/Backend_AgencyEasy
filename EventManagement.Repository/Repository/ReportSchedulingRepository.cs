using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class ReportSchedulingRepository : Repository<ReportScheduling, Guid>, IReportSchedulingRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<ReportScheduling> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public ReportSchedulingRepository(EventManagementContext context, ILogger<ReportSchedulingRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<ReportScheduling>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<ReportScheduling> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
