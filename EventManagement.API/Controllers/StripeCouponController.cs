using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//using EventManagement.API.Helpers;
using EventManagement.Dto;
using EventManagement.Service;
using EventManagement.Domain.Entities;
using EventManagement.Utility;
using Microsoft.Extensions.Logging;
using IdentityServer4.AccessTokenValidation;
using Stripe;
using Microsoft.Extensions.Configuration;
using MailChimp.Net.Models;
using System.Net.Http;
using EventManagement.Domain.Migrations;
using StripeCoupon = EventManagement.Domain.Entities.StripeCoupon;
using System.Linq;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// StripeCoupon endpoint
    /// </summary>
    [Route("api/stripecoupons")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class StripeCouponController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly IStripeCouponService _stripecouponService;
        private ILogger<StripeCouponController> _logger;
        private readonly IUrlHelper _urlHelper;
        private readonly IConfiguration _configuration;

        #endregion


        #region CONSTRUCTOR

        public StripeCouponController(IStripeCouponService stripecouponService, ILogger<StripeCouponController> logger, IUrlHelper urlHelper,
            IConfiguration configuration)
        {
            _logger = logger;
            _stripecouponService = stripecouponService;
            _urlHelper = urlHelper;
            _configuration = configuration;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredStripeCoupons")]
        [Produces("application/vnd.tourmanagement.stripecoupons.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<StripeCouponDto>>> GetFilteredStripeCoupons([FromQuery] FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_stripecouponService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<StripeCouponDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var stripecouponsFromRepo = await _stripecouponService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.stripecoupons.hateoas+json")
            {
                //create HATEOAS links for each show.
                stripecouponsFromRepo.ForEach(stripecoupon =>
                {
                    var entityLinks = CreateLinksForStripeCoupon(stripecoupon.Id, filterOptionsModel.Fields);
                    stripecoupon.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = stripecouponsFromRepo.TotalCount,
                    pageSize = stripecouponsFromRepo.PageSize,
                    currentPage = stripecouponsFromRepo.CurrentPage,
                    totalPages = stripecouponsFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForStripeCoupons(filterOptionsModel, stripecouponsFromRepo.HasNext, stripecouponsFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = stripecouponsFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = stripecouponsFromRepo.HasPrevious ?
                    CreateStripeCouponsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = stripecouponsFromRepo.HasNext ?
                    CreateStripeCouponsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = stripecouponsFromRepo.TotalCount,
                    pageSize = stripecouponsFromRepo.PageSize,
                    currentPage = stripecouponsFromRepo.CurrentPage,
                    totalPages = stripecouponsFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(stripecouponsFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.stripecoupons.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetStripeCoupon")]
        public async Task<ActionResult<StripeCoupon>> GetStripeCoupon(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object stripecouponEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetStripeCoupon called");

                //then get the whole entity and map it to the Dto.
                stripecouponEntity = Mapper.Map<StripeCouponDto>(await _stripecouponService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                stripecouponEntity = await _stripecouponService.GetPartialEntityAsync(id, fields);
            }

            //if stripecoupon not found.
            if (stripecouponEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.stripecoupons.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForStripeCoupon(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((StripeCouponDto)stripecouponEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = stripecouponEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = stripecouponEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("generate-coupon", Name = "generate-coupon")]
        public async Task<ActionResult<StripeCouponDto>> GenerateCoupon(Guid companyId, string subscriptionId)
        {
            var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

            var couponCode = _configuration.GetSection("StripeCouponCode:20Percent").Value;

            var stripecoupon = new StripeCouponForCreation();

            try
            {
                var CouponCount = _stripecouponService.Count(x => x.CompanyId == companyId && x.DiscountPercent == "20%");

                if (CouponCount == 0)
                {
                    //coupon applied on existing subcription
                    if (!string.IsNullOrEmpty(subscriptionId) && subscriptionId.Contains("sub_"))
                    {
                        var subscriptionService = new SubscriptionService(stripeSecretKey);
                        var subscription = await subscriptionService.GetAsync(subscriptionId);

                        //Update plan if subscription is active
                        if (subscription.Status == "active")
                        {
                            var updateoptions = new SubscriptionUpdateOptions
                            {
                                Coupon = couponCode
                            };
                            var sub_service = new SubscriptionService(stripeSecretKey);
                            var update_sub = sub_service.Update(subscriptionId, updateoptions);
                        }
                    }

                    stripecoupon.CouponCode = couponCode;
                    stripecoupon.ExpiredAt = DateTime.Now.AddYears(100);
                    stripecoupon.GeneratedBy = "User";
                    stripecoupon.DiscountPercent = "20%";
                    stripecoupon.CompanyId = companyId;
                    stripecoupon.Message = "200";

                    //create a show in db.
                    var stripecouponToReturn = await _stripecouponService.CreateEntityAsync<StripeCouponDto, StripeCouponForCreation>(stripecoupon);

                    return Ok(stripecouponToReturn);
                }
                else
                {
                    var stripeCouponDto = new StripeCouponDto();
                    stripeCouponDto.Message = "Allowed only one coupon to generate";
                    return stripeCouponDto;
                }

            }
            catch (StripeException e)
            {
                return BadRequest(new { error = new { message = e.StripeError.Message } });
            }
            catch (System.Exception)
            {
                return BadRequest(new { error = new { message = "unknown failure: 500" } });
            }

        }



        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateStripeCoupon")]
        public async Task<IActionResult> UpdateStripeCoupon(Guid id, [FromBody] StripeCouponForUpdate StripeCouponForUpdate)
        {

            //if show not found
            if (!await _stripecouponService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _stripecouponService.UpdateEntityAsync(id, StripeCouponForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateStripeCoupon(Guid id, [FromBody] JsonPatchDocument<StripeCouponForUpdate> jsonPatchDocument)
        {
            StripeCouponForUpdate dto = new StripeCouponForUpdate();
            StripeCoupon stripecoupon = new StripeCoupon();

            //apply the patch changes to the dto. 
            jsonPatchDocument.ApplyTo(dto, ModelState);

            //if the jsonPatchDocument is not valid.
            if (!ModelState.IsValid)
            {
                //then return unprocessableEntity response.
                return new UnprocessableEntityObjectResult(ModelState);
            }

            //if the dto model is not valid after applying changes.
            if (!TryValidateModel(dto))
            {
                //then return unprocessableEntity response.
                return new UnprocessableEntityObjectResult(ModelState);
            }

            //map the chnages from dto to entity.
            Mapper.Map(dto, stripecoupon);

            //set the Id for the show model.
            stripecoupon.Id = id;

            //partially update the chnages to the db. 
            await _stripecouponService.UpdatePartialEntityAsync(stripecoupon, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST


        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateStripeCoupon")]
        public async Task<ActionResult<StripeCouponDto>> CreateStripeCoupon([FromBody] StripeCouponForCreation stripecoupon)
        {
            //create a show in db.
            var stripecouponToReturn = await _stripecouponService.CreateEntityAsync<StripeCouponDto, StripeCouponForCreation>(stripecoupon);

            //return the show created response.
            return CreatedAtRoute("GetStripeCoupon", new { id = stripecouponToReturn.Id }, stripecouponToReturn);
        }

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("create-coupon", Name = "create-coupon")]       
        public async Task<ActionResult> CreateCoupon([FromBody] CouponDto couponDto)
        {
            try
            {
                var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

                var options = new CouponCreateOptions
                {
                    Duration = couponDto.Duration,
                    Currency = "USD",
                    Name = couponDto.Name
                };

                if (couponDto.PercentOff.HasValue && couponDto.PercentOff > 0)
                {
                    options.PercentOff = couponDto.PercentOff;
                }

                if (couponDto.AmountOff.HasValue && couponDto.AmountOff > 0)
                {
                    options.AmountOff = couponDto.AmountOff;
                }

                if (!string.IsNullOrEmpty(couponDto.Duration) && couponDto.Duration.Trim() == "repeating")
                {
                    options.DurationInMonths = couponDto.DurationInMonths;
                }

                if (couponDto.MaxRedemptions.HasValue && couponDto.MaxRedemptions > 0)
                {
                    options.MaxRedemptions = couponDto.MaxRedemptions;
                }

                if (couponDto.RedeemBy.HasValue)
                {
                    options.RedeemBy = couponDto.RedeemBy;
                }

                var service = new CouponService(stripeSecretKey);
                var response = await service.CreateAsync(options);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }


        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("create-promotion-code", Name = "create-promotion-code")]        
        public async Task<ActionResult> CreatePromotion([FromBody] CreatePromotionCodeDetails promotionCodeDetail)
        {
            try
            {
                var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

                // For restrictions
                var restrictionsOptions = new PromotionCodeRestrictionsOptions
                {
                    FirstTimeTransaction = promotionCodeDetail.Restrictions.FirstTimeTransaction,
               
                };

                if (promotionCodeDetail.Restrictions.MinimumAmount.HasValue && promotionCodeDetail.Restrictions.MinimumAmount > 0)
                {
                    restrictionsOptions.MinimumAmount = promotionCodeDetail.Restrictions.MinimumAmount;
                    restrictionsOptions.MinimumAmountCurrency = promotionCodeDetail.Restrictions.MinimumAmountCurrency;
                }

                var options = new PromotionCodeCreateOptions
                {
                    Coupon = promotionCodeDetail.CouponCodeId,
                    Restrictions = restrictionsOptions
                };

                // For customer
                if (!string.IsNullOrEmpty(promotionCodeDetail.Customer))
                {
                    options.Customer = promotionCodeDetail.Customer;
                }

                // For Max Redemptions
                if (promotionCodeDetail.MaxRedemptions.HasValue && promotionCodeDetail.MaxRedemptions > 0)
                {
                    options.MaxRedemptions = promotionCodeDetail.MaxRedemptions;
                }

                // For Expiration
                if (promotionCodeDetail.ExpiresAt.HasValue)
                {
                    options.ExpiresAt = promotionCodeDetail.ExpiresAt;
                }

                // For customer-facing coupon code
                if (!string.IsNullOrEmpty(promotionCodeDetail.Code))
                {
                    options.Code = promotionCodeDetail.Code;
                }

                var service = new PromotionCodeService(stripeSecretKey);
                var response = await service.CreateAsync(options);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("isexist-promotion-code", Name = "isexist-promotion-code")]
        public async Task<ActionResult> IsExistPromotion([FromBody] CreatePromotionCodeDetails promotionCodeDetail)
        {
            try
            {
                var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

                var options = new PromotionCodeListOptions { Limit = 1, Code = promotionCodeDetail.Code };
                var service = new PromotionCodeService(stripeSecretKey);
                StripeList<PromotionCode> promotionCodes = await service.ListAsync(options);               

                return Ok(promotionCodes);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }


        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteStripeCouponById")]
        public async Task<IActionResult> DeleteStripeCouponById(Guid id)
        {
            //if the stripecoupon exists
            if (await _stripecouponService.ExistAsync(x => x.Id == id))
            {
                //delete the stripecoupon from the db.
                await _stripecouponService.DeleteEntityAsync(id);
            }
            else
            {
                //if stripecoupon doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }



        #endregion

        #region For Coupon module 

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("get-coupon-list", Name = "get-coupon-list")]

        public async Task<ActionResult<List<CouponDto>>> GetCouponList()
        {
            var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

            try
            {
                //A limit on the number of objects to be returned. Limit can range between 1 and 100, and the default is 10.
                var options = new CouponListOptions { Limit = 100 };
                var service = new CouponService(stripeSecretKey);
                StripeList<Coupon> coupons = await service.ListAsync(options);

                var couponList = coupons.Select(coupon => new CouponDto
                {
                    Id = coupon.Id,
                    Name = coupon.Name,
                    PercentOff = coupon.PercentOff,
                    TimesRedeemed = coupon.TimesRedeemed,
                    MaxRedemptions = coupon.MaxRedemptions,
                    RedeemBy = coupon.RedeemBy,
                    Valid = coupon.Valid,
                    Duration = coupon.Duration,
                    DurationInMonths = coupon.DurationInMonths,
                    AmountOff = coupon.AmountOff,

                }).ToList();

                return Ok(couponList);
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = new { message = e.StripeError.Message } });
            }
            catch (System.Exception)
            {
                return BadRequest(new { error = new { message = "unknown failure: 500" } });
            }

        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("get-promotion-code-list", Name = "get-promotion-code-list")]

        public async Task<ActionResult<List<PromotionCodeDetails>>> GetPromotionList(string couponId)
        {
            var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

            try
            {
                var options = new PromotionCodeListOptions { Limit = 100, Coupon = couponId };
                var promotionService = new PromotionCodeService(stripeSecretKey);
                var promotionCodes = await promotionService.ListAsync(options);

                var listOfProCode = promotionCodes.Data.Select(pcode => new PromotionCodeDetails
                {
                    Id = pcode.Id,
                    CouponCodeId = couponId,
                    Code = pcode.Code,
                    Active = pcode.Active,
                    MaxRedemptions = pcode.MaxRedemptions,
                    TimesRedeemed = pcode.TimesRedeemed,
                    ExpiresAt = pcode.ExpiresAt,
                    Created = pcode.Created,
                    Restrictions = new RestrictionsDto
                    {
                        FirstTimeTransaction = pcode.Restrictions.FirstTimeTransaction,
                        MinimumAmount = pcode.Restrictions.MinimumAmount,
                        MinimumAmountCurrency = pcode.Restrictions.MinimumAmountCurrency
                    }

                }).ToList();

                return Ok(listOfProCode);
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = new { message = e.StripeError.Message } });
            }
            catch (System.Exception)
            {
                return BadRequest(new { error = new { message = "unknown failure: 500" } });
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpDelete("delete-coupon", Name = "delete-coupon")]
        public async Task<ActionResult<Coupon>> DeleteCoupon(string couponId)
        {
            var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

            try
            {
                //Get coupon detail
                var service = new CouponService(stripeSecretKey);
                var couponDetail = await service.GetAsync(couponId);
                if (couponDetail != null)
                {
                    var couponService = new CouponService(stripeSecretKey);
                    var response = await couponService.DeleteAsync(couponId);
                    return Ok(response);
                }
                else
                {
                    return NotFound(new { error = new { message = "Coupon not found." } });
                }
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = new { message = e.StripeError.Message } });
            }
            catch (System.Exception)
            {
                return BadRequest(new { error = new { message = "unknown failure: 500" } });
            }

        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("get-customer-list", Name = "get-customer-list")]       
        public async Task<ActionResult<List<CustomerDto>>> GetCustomerList(string keyword)
        {
            var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

            try
            {
                var options = new CustomerSearchOptions
                {
                    Query = "name~'" + keyword.Trim() + "' OR email~'" + keyword.Trim() + "'",
                };
                var service = new CustomerService(stripeSecretKey);
                var response = await service.SearchAsync(options);

                var retVal = response.Select(x => new CustomerDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Email = x.Email

                }).ToList();

                return Ok(retVal);
            }
            catch (StripeException e)
            {
                throw;
            }
            catch (System.Exception)
            {
                throw;
            }
        }


        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForStripeCoupon(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetStripeCoupon", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetStripeCoupon", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteStripeCouponById", new { id = id }),
              "delete_stripecoupon",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateStripeCoupon", new { id = id }),
             "update_stripecoupon",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateStripeCoupon", new { }),
              "create_stripecoupon",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForStripeCoupons(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateStripeCouponsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateStripeCouponsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateStripeCouponsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateStripeCouponsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredStripeCoupons",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredStripeCoupons",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredStripeCoupons",
                    new
                    {
                        fields = filterOptionsModel.Fields,
                        orderBy = filterOptionsModel.OrderBy,
                        searchQuery = filterOptionsModel.SearchQuery,
                        pageNumber = filterOptionsModel.PageNumber,
                        pageSize = filterOptionsModel.PageSize
                    });
            }
        }

        #endregion

    }
}
