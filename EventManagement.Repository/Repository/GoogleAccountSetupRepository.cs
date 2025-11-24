using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class GoogleAccountSetupRepository : Repository<GoogleAccountSetup, Guid>, IGoogleAccountSetupRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<GoogleAccountSetup> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public GoogleAccountSetupRepository(EventManagementContext context, ILogger<GoogleAccountSetupRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<GoogleAccountSetup>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<GoogleAccountSetup> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
