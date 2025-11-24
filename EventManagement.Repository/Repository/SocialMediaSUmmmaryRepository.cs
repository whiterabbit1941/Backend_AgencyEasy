using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class SocialMediaSUmmmaryRepository : Repository<SocialMediaSUmmmary, Guid>, ISocialMediaSUmmmaryRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<SocialMediaSUmmmary> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public SocialMediaSUmmmaryRepository(EventManagementContext context, ILogger<SocialMediaSUmmmaryRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<SocialMediaSUmmmary>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<SocialMediaSUmmmary> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
