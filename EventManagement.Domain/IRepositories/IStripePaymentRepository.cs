using System;
using EventManagement.Domain.Entities;

namespace EventManagement.Domain
{
    public interface IStripePaymentRepository : IRepository<StripePayment, Guid>
    {

    }
}
