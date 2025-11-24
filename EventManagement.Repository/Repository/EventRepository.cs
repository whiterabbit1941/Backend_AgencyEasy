using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EventManagement.Repository
{
    public class EventRepository : Repository<Event, Guid>, IEventRepository
    {
        #region PRIVATE VARIABLE

        private readonly DbSet<Event> _dbset;
        private readonly EventManagementContext _context;

        #endregion

        #region CONSTRUCTOR

        public EventRepository(EventManagementContext context, ILogger<EventRepository> logger) : base(context, logger)
        {
            _context = context;
            _dbset = context.Set<Event>();
        }

        #endregion

        #region OVERRIDDEN IMPLEMENTATION

        public override string GetPrimaryKeyColumnName()
        {
            return "Id";
        }

        public override IQueryable<Event> GetFilteredEntities(bool bIsAsTrackable = false)
        {
            return _dbset;
        }

        #endregion

    }
}
