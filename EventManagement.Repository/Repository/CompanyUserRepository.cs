using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class CompanyUserRepository : Repository<CompanyUser, Guid>, ICompanyUserRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<CompanyUser> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public CompanyUserRepository(EventManagementContext context, ILogger<CompanyUserRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<CompanyUser>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<CompanyUser> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
