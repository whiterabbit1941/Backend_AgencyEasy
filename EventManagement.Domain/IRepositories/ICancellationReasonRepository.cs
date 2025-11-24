using System;
using EventManagement.Domain.Entities;

namespace EventManagement.Domain
{
    public interface ICancellationReasonRepository : IRepository<CancellationReason, Guid>
    {

    }
}
