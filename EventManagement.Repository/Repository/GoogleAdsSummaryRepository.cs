using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class GoogleAdsSummaryRepository : Repository<GoogleAdsSummary, Guid>, IGoogleAdsSummaryRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<GoogleAdsSummary> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public GoogleAdsSummaryRepository(EventManagementContext context, ILogger<GoogleAdsSummaryRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<GoogleAdsSummary>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<GoogleAdsSummary> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
