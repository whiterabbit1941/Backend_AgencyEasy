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

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// CompanyPlan endpoint
    /// </summary>
    [Route("api/companyplans")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class CompanyPlanController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ICompanyPlanService _companyplanService;
        private ILogger<CompanyPlanController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public CompanyPlanController(ICompanyPlanService companyplanService, ILogger<CompanyPlanController> logger, IUrlHelper urlHelper)
        {
            _logger = logger;
            _companyplanService = companyplanService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetAllTransactionHistoryByCompanyId", Name = "GetAllTransactionHistoryByCompanyId")]
        public async Task<IActionResult> GetAllTransactionHistoryByCompanyId(Guid id)
        {
            var planData = await _companyplanService.GetAllTransactionHistoryByCompanyId(id);
            return Ok(planData);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetAppsumoPlanByCompanyPlanId", Name = "GetAppsumoPlanByCompanyPlanId")]
        public async Task<IActionResult> GetAppsumoPlanByCompanyPlanId(Guid id)
        {
            var planData = await _companyplanService.GetAppsumoPlanByCompanyPlanId(id);
            return Ok(planData);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("IsInvoiceExists", Name = "IsInvoiceExists")]
        public async Task<IActionResult> IsInvoiceExists(string id)
        {
            var planData = await _companyplanService.IsInvoiceExists(id);
            return Ok(planData);
        }


        [HttpGet(Name = "GetFilteredCompanyPlans")]
        [Produces("application/vnd.tourmanagement.companyplans.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CompanyPlanDto>>> GetFilteredCompanyPlans([FromQuery] FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_companyplanService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<CompanyPlanDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var companyplansFromRepo = await _companyplanService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.companyplans.hateoas+json")
            {
                //create HATEOAS links for each show.
                companyplansFromRepo.ForEach(companyplan =>
                {
                    var entityLinks = CreateLinksForCompanyPlan(companyplan.Id, filterOptionsModel.Fields);
                    companyplan.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = companyplansFromRepo.TotalCount,
                    pageSize = companyplansFromRepo.PageSize,
                    currentPage = companyplansFromRepo.CurrentPage,
                    totalPages = companyplansFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForCompanyPlans(filterOptionsModel, companyplansFromRepo.HasNext, companyplansFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = companyplansFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = companyplansFromRepo.HasPrevious ?
                    CreateCompanyPlansResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = companyplansFromRepo.HasNext ?
                    CreateCompanyPlansResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = companyplansFromRepo.TotalCount,
                    pageSize = companyplansFromRepo.PageSize,
                    currentPage = companyplansFromRepo.CurrentPage,
                    totalPages = companyplansFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(companyplansFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.companyplans.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetCompanyPlan")]
        public async Task<ActionResult<CompanyPlan>> GetCompanyPlan(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object companyplanEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetCompanyPlan called");

                //then get the whole entity and map it to the Dto.
                companyplanEntity = Mapper.Map<CompanyPlanDto>(await _companyplanService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                companyplanEntity = await _companyplanService.GetPartialEntityAsync(id, fields);
            }

            //if companyplan not found.
            if (companyplanEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.companyplans.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForCompanyPlan(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((CompanyPlanDto)companyplanEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = companyplanEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = companyplanEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateCompanyPlan")]
        public async Task<IActionResult> UpdateCompanyPlan(Guid id, [FromBody] CompanyPlanForUpdate CompanyPlanForUpdate)
        {

            //if show not found
            if (!await _companyplanService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _companyplanService.UpdateEntityAsync(id, CompanyPlanForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateCompanyPlan(Guid id, [FromBody] JsonPatchDocument<CompanyPlanForUpdate> jsonPatchDocument)
        {
            CompanyPlanForUpdate dto = new CompanyPlanForUpdate();
            CompanyPlan companyplan = new CompanyPlan();

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
            Mapper.Map(dto, companyplan);

            //set the Id for the show model.
            companyplan.Id = id;

            //partially update the chnages to the db. 
            await _companyplanService.UpdatePartialEntityAsync(companyplan, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateCompanyPlan")]
        public async Task<ActionResult<CompanyPlanDto>> CreateCompanyPlan([FromBody] CompanyPlanForCreation companyplan)
        {
            //create a show in db.
            var companyplanToReturn = await _companyplanService.CreateEntityAsync<CompanyPlanDto, CompanyPlanForCreation>(companyplan);

            //return the show created response.
            return CreatedAtRoute("GetCompanyPlan", new { id = companyplanToReturn.Id }, companyplanToReturn);
        }

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("CreateCompanyPlanForStripe", Name = "CreateCompanyPlanForStripe")]
        public async Task<ActionResult<bool>> CreateCompanyPlanForStripe([FromBody] CompanyPlanDetailDto companyplan)
        {
            //create a show in db.
            var companyplanToReturn = await _companyplanService.CreateStripePlan(companyplan);

            //return the show created response.
            return companyplanToReturn;
        }


        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        [HttpPost("IsNewPlanPaymentIdExists", Name = "IsNewPlanPaymentIdExists")]
        public async Task<ActionResult<bool>> IsNewPlanPaymentIdExists(string newPlanPaymentId)
        {
            var res = await _companyplanService.ExistAsync(x => x.PaymentProfileId == newPlanPaymentId);
            return res;
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteCompanyPlanById")]
        public async Task<IActionResult> DeleteCompanyPlanById(Guid id)
        {
            //if the companyplan exists
            if (await _companyplanService.ExistAsync(x => x.Id == id))
            {
                //delete the companyplan from the db.
                await _companyplanService.DeleteEntityAsync(id);
            }
            else
            {
                //if companyplan doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForCompanyPlan(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCompanyPlan", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCompanyPlan", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteCompanyPlanById", new { id = id }),
              "delete_companyplan",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateCompanyPlan", new { id = id }),
             "update_companyplan",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateCompanyPlan", new { }),
              "create_companyplan",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCompanyPlans(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateCompanyPlansResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateCompanyPlansResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateCompanyPlansResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateCompanyPlansResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredCompanyPlans",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredCompanyPlans",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredCompanyPlans",
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
