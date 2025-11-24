using System;
using System.Collections.Generic;

namespace EventManagement.Dto
{
    /// <summary>
    /// StripeCoupon Model
    /// </summary>
    public class StripeCouponDto : StripeCouponAbstractBase
    {

    }


    public class CouponDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal? PercentOff { get; set; }
        public long? AmountOff { get; set; }
        public long TimesRedeemed { get; set; }
        public long? MaxRedemptions { get; set; }
        public DateTime? RedeemBy { get; set; }
        public bool Valid { get; set; }
        public string Duration { get; set; }
        public long? DurationInMonths { get; set; }
        public string Currency { get; set; }
    }


    public class PromotionCodeDetails
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public bool Active { get; set; }
        public string Code { get; set; }
        public string CouponCodeId { get; set; }
        public CouponDetailsDto Coupon { get; set; }
        public DateTime Created { get; set; }
        public string Customer { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool Livemode { get; set; }
        public long? MaxRedemptions { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
        public RestrictionsDto Restrictions { get; set; }
        public long TimesRedeemed { get; set; }
    }

    public class CreatePromotionCodeDetails
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string CouponCodeId { get; set; }     
        public string Customer { get; set; }
        public DateTime? ExpiresAt { get; set; }       
        public long? MaxRedemptions { get; set; }        
        public RestrictionsDto Restrictions { get; set; }
    }


    public class CouponDetailsDto
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public int? AmountOff { get; set; }
        public long Created { get; set; }
        public string Currency { get; set; }
        public string Duration { get; set; }
        public int? DurationInMonths { get; set; }
        public bool Livemode { get; set; }
        public int? MaxRedemptions { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
        public string Name { get; set; }
        public double? PercentOff { get; set; }
        public long? RedeemBy { get; set; }
        public int TimesRedeemed { get; set; }
        public bool Valid { get; set; }
    }

    public class RestrictionsDto
    {
        public bool FirstTimeTransaction { get; set; }
        public long? MinimumAmount { get; set; }
        public string MinimumAmountCurrency { get; set; }
    }

    public class CustomerDto
    {
        public string Id { get; set; }
        public string Name{ get; set; }
        public string Email { get; set; }
    }

    public class CreateStripeDto
    {
              
    }
}
