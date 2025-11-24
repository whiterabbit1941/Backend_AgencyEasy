using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class AuditsRepository : Repository<Audits, Guid>, IAuditsRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<Audits> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public AuditsRepository(EventManagementContext context, ILogger<AuditsRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<Audits>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<Audits> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
