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
using System.Linq;
using Stripe;
using Microsoft.Extensions.Configuration;
using Plan = EventManagement.Domain.Entities.Plan;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// Plan endpoint
    /// </summary>
    [Route("api/plans")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]

    public class PlanController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly IPlanService _planService;
        private ILogger<PlanController> _logger;
        private readonly IUrlHelper _urlHelper;
        private readonly IProductService _productService;
        private IConfiguration _configuration;

        #endregion


        #region CONSTRUCTOR

        public PlanController(IProductService productService, IPlanService planService,
            ILogger<PlanController> logger, IUrlHelper urlHelper, IConfiguration configuration)
        {
            _logger = logger;
            _planService = planService;
            _urlHelper = urlHelper;
            _productService = productService;
            _configuration = configuration;
        }

        #endregion


        #region HTTPGET

        [HttpGet("GetAllPlanByCompanyId", Name = "GetAllPlanByCompanyId")]
        [Produces("application/vnd.tourmanagement.plans.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PlanDto>>> GetAllPlanByCompanyId([FromQuery] Guid companyId)
        {
            List<PlanDto> returnData = new List<PlanDto>();
            var productIdList = _productService.GetAllEntities().Select(x => x.Id).ToList();
            for (int i = 0; i < productIdList.Count; i++)
            {
                var plans = _planService.getPlansByProductId(productIdList[i]);
                if (plans.Count > 0)
                {
                    returnData.AddRange(plans);
                }
            }
            return Ok(returnData);
        }

        [HttpGet(Name = "GetFilteredPlans")]
        [Produces("application/vnd.tourmanagement.plans.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<PlanDto>>> GetFilteredPlans([FromQuery] FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_planService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<PlanDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var plansFromRepo = await _planService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.plans.hateoas+json")
            {
                //create HATEOAS links for each show.
                plansFromRepo.ForEach(plan =>
                {
                    var entityLinks = CreateLinksForPlan(plan.Id, filterOptionsModel.Fields);
                    plan.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = plansFromRepo.TotalCount,
                    pageSize = plansFromRepo.PageSize,
                    currentPage = plansFromRepo.CurrentPage,
                    totalPages = plansFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForPlans(filterOptionsModel, plansFromRepo.HasNext, plansFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = plansFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = plansFromRepo.HasPrevious ?
                    CreatePlansResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = plansFromRepo.HasNext ?
                    CreatePlansResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = plansFromRepo.TotalCount,
                    pageSize = plansFromRepo.PageSize,
                    currentPage = plansFromRepo.CurrentPage,
                    totalPages = plansFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(plansFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.plans.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetPlan")]
        public async Task<ActionResult<Domain.Entities.Plan>> GetPlan(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object planEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetPlan called");

                //then get the whole entity and map it to the Dto.
                planEntity = Mapper.Map<PlanDto>(await _planService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                planEntity = await _planService.GetPartialEntityAsync(id, fields);
            }

            //if plan not found.
            if (planEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.plans.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForPlan(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((PlanDto)planEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = planEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = planEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetPlansByProductId", Name = "GetPlansByProductId")]
        public async Task<ActionResult<List<CampaignDto>>> GetPlansByProductId([FromQuery] Guid productId)
        {
            var planInfo = _planService.getPlansByProductId(productId);

            return Ok(planInfo);
        }
        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdatePlan")]
        public async Task<IActionResult> UpdatePlan(Guid id, [FromBody] PlanForUpdate PlanForUpdate)
        {

            var plandetails = _planService.GetPlansById(id);

            if (PlanForUpdate.Price != plandetails.Price || PlanForUpdate.PaymentType != plandetails.PaymentType
                || PlanForUpdate.PaymentCycle != plandetails.PaymentCycle || PlanForUpdate.Currency != plandetails.Currency)
            {
                var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

                var paymentMode = PlanForUpdate.PaymentType == "recurring" ? "subscription" : "payment";

                //Archive old stripe price id and create new stripe price stripe

                var updateOptions = new PriceUpdateOptions
                {
                    Active = false
                };
                var service = new PriceService(stripeSecretKey);
                service.Update(
                  PlanForUpdate.priceId,
                  updateOptions);

                //Create the new price
                var createOptions = new PriceCreateOptions();
                if (PlanForUpdate.PaymentType == "payment")
                {
                    createOptions = new PriceCreateOptions
                    {
                        UnitAmount = Convert.ToInt32(PlanForUpdate.Price.ToString()) * 100,
                        Currency = PlanForUpdate.Currency,
                        Product = _configuration["MarketPlaceProductId"]
                    };
                }
                else if (PlanForUpdate.PaymentType == "recurring")
                {
                    createOptions = new PriceCreateOptions
                    {
                        UnitAmount = Convert.ToInt32(PlanForUpdate.Price.ToString()) * 100,
                        Currency = PlanForUpdate.Currency,
                        Product = _configuration["MarketPlaceProductId"],
                        Recurring = new PriceRecurringOptions
                        {
                            Interval = PlanForUpdate.PaymentCycle,
                        },
                    };
                }

                var createService = new PriceService();
                var price = service.Create(createOptions);
                PlanForUpdate.priceId = price.Id;
            }

            //if show not found
            if (!await _planService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _planService.UpdateEntityAsync(id, PlanForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdatePlan(Guid id, [FromBody] JsonPatchDocument<PlanForUpdate> jsonPatchDocument)
        {
            PlanForUpdate dto = new PlanForUpdate();
            Plan plan = new Plan();

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
            Mapper.Map(dto, plan);

            //set the Id for the show model.
            plan.Id = id;

            //partially update the chnages to the db. 
            await _planService.UpdatePartialEntityAsync(plan, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreatePlan")]
        public async Task<ActionResult<PlanDto>> CreatePlan([FromBody] PlanForCreation plan)
        {
            //create a show in db.
            var planToReturn = await _planService.CreateEntityAsync<PlanDto, PlanForCreation>(plan);

            //return the show created response.
            return CreatedAtRoute("GetPlan", new { id = planToReturn.Id }, planToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeletePlanById")]
        public async Task<IActionResult> DeletePlanById(Guid id)
        {
            //if the plan exists
            if (await _planService.ExistAsync(x => x.Id == id))
            {
                //delete the plan from the db.
                await _planService.DeleteEntityAsync(id);
            }
            else
            {
                //if plan doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForPlan(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetPlan", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetPlan", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeletePlanById", new { id = id }),
              "delete_plan",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdatePlan", new { id = id }),
             "update_plan",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreatePlan", new { }),
              "create_plan",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForPlans(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreatePlansResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreatePlansResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreatePlansResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreatePlansResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredPlans",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredPlans",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredPlans",
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
