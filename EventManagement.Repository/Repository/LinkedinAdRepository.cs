using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class LinkedinAdRepository : Repository<LinkedinAd, Guid>, ILinkedinAdRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<LinkedinAd> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public LinkedinAdRepository(EventManagementContext context, ILogger<LinkedinAdRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<LinkedinAd>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<LinkedinAd> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
