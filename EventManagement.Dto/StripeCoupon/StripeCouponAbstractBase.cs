using System;

namespace EventManagement.Dto
{
    public abstract class StripeCouponAbstractBase
    {
        /// <summary>
        /// StripeCoupon Id.
        /// </summary>
        public Guid Id { get; set; }

        public string CouponCode { get; set; }

        public DateTime ExpiredAt { get; set; }

        public string DiscountPercent { get; set; }

        public string GeneratedBy { get; set; }

        public Guid CompanyId { get; set; }

        public string Message { get; set; }

    }
}
