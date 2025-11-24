using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class CompanyRepository : Repository<Company, Guid>, ICompanyRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<Company> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public CompanyRepository(EventManagementContext context, ILogger<CompanyRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<Company>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<Company> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
