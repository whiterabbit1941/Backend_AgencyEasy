using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;
using FinanaceManagement.API.Models;

namespace EventManagement.Repository
{
    public class ClientRedirectUriRepository : Repository<ClientRedirectUris, int>, IClientRedirectUriRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<ClientRedirectUris> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public ClientRedirectUriRepository(EventManagementContext context, ILogger<ClientRedirectUriRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<ClientRedirectUris>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<ClientRedirectUris> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
