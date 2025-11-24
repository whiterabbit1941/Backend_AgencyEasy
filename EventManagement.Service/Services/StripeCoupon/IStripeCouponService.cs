using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;

namespace EventManagement.Service
{
    public interface IStripeCouponService : IService<StripeCoupon, Guid>
    {
        Task<StripeCouponDto> GenerateFiftyPercentCoupon(Guid companyId);

    }
}
