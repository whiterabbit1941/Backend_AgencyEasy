using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class CancellationReasonRepository : Repository<CancellationReason, Guid>, ICancellationReasonRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<CancellationReason> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public CancellationReasonRepository(EventManagementContext context, ILogger<CancellationReasonRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<CancellationReason>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<CancellationReason> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
