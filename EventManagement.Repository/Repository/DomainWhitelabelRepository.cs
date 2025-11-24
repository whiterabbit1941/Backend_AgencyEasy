using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class DomainWhitelabelRepository : Repository<DomainWhitelabel, Guid>, IDomainWhitelabelRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<DomainWhitelabel> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public DomainWhitelabelRepository(EventManagementContext context, ILogger<DomainWhitelabelRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<DomainWhitelabel>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<DomainWhitelabel> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
