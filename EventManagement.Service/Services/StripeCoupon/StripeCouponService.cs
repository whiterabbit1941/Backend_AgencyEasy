using System;
using System.Collections.Generic;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using EventManagement.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Stripe;

namespace EventManagement.Service
{
    public class StripeCouponService : ServiceBase<StripeCoupon, Guid>, IStripeCouponService
    {

        #region PRIVATE MEMBERS

        private readonly IStripeCouponRepository _stripecouponRepository;
        private readonly IConfiguration _configuration;
        #endregion


        #region CONSTRUCTOR

        public StripeCouponService(IStripeCouponRepository stripecouponRepository, ILogger<StripeCouponService> logger, IConfiguration configuration) : base(stripecouponRepository, logger)
        {
            _stripecouponRepository = stripecouponRepository;
            _configuration = configuration;
        }

        #endregion


        #region PUBLIC MEMBERS   

        public async Task<StripeCouponDto> GenerateFiftyPercentCoupon(Guid companyId)
        {
            var couponDto = new StripeCouponDto();
            var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);
            var couponCode = _configuration.GetSection("StripeCouponCode:50Percent").Value;
            var stripecoupon = new StripeCouponForCreation();

            try
            {                                 
                    var options = new PromotionCodeCreateOptions
                    {
                        Coupon = couponCode,
                        MaxRedemptions = 1,                                             
                        Restrictions = new PromotionCodeRestrictionsOptions
                        {
                            FirstTimeTransaction = true                                                      
                        }
                    };  

                    var service = new PromotionCodeService(stripeSecretKey);

                    var response = await service.CreateAsync(options);
                   

                    stripecoupon.CouponCode = response.Code;
                    stripecoupon.ExpiredAt = new DateTime(2023, 10, 31, 23, 59, 0); ;
                    stripecoupon.GeneratedBy = "Free Signup";
                    stripecoupon.DiscountPercent = "50%";
                    stripecoupon.CompanyId = companyId;

                    //create a show in db.
                    couponDto = await CreateEntityAsync<StripeCouponDto, StripeCouponForCreation>(stripecoupon);

                    return couponDto;

            }
            catch (StripeException e)
            {
                couponDto.Message = e.Message;
                return couponDto;
            }
            catch (System.Exception e)
            {
                couponDto.Message = e.Message;
                return couponDto;
            }
        }

        #endregion


        #region OVERRIDDEN IMPLEMENTATION

        public override Dictionary<string, PropertyMappingValue> GetPropertyMapping()
        {
            return new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Id", new PropertyMappingValue(new List<string>() { "Id" } ) },
                        { "CouponCode", new PropertyMappingValue(new List<string>() { "CouponCode" } )},
                        { "DiscountPercent", new PropertyMappingValue(new List<string>() { "DiscountPercent" } )},
                        { "ExpiredAt", new PropertyMappingValue(new List<string>() { "ExpiredAt" } )},
                    };
        }

        public override string GetDefaultOrderByColumn()
        {
            return "CouponCode,ExpiredAt,DiscountPercent";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "CouponCode,ExpiredAt,DiscountPercent";
        }

        #endregion
    }
}
