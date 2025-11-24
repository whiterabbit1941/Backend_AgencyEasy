using System;
using EventManagement.Domain.Entities;

namespace EventManagement.Domain
{
    public interface IFeatureRepository : IRepository<Feature, Guid>
    {

    }
}
