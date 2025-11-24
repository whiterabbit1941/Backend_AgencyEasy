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
using System.Net;
using IdentityServer4.AccessTokenValidation;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// CampaignWooCommerce endpoint
    /// </summary>
    [Route("api/campaignwoocommerces")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class CampaignWooCommerceController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignWooCommerceService _campaignwoocommerceService;
        private ILogger<CampaignWooCommerceController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public CampaignWooCommerceController(ICampaignWooCommerceService campaignwoocommerceService, ILogger<CampaignWooCommerceController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _campaignwoocommerceService = campaignwoocommerceService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredCampaignWooCommerces")]
        [Produces("application/vnd.tourmanagement.campaignwoocommerces.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CampaignWooCommerceDto>>> GetFilteredCampaignWooCommerces([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_campaignwoocommerceService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<CampaignWooCommerceDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var campaignwoocommercesFromRepo = await _campaignwoocommerceService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.campaignwoocommerces.hateoas+json")
            {
                //create HATEOAS links for each show.
                campaignwoocommercesFromRepo.ForEach(campaignwoocommerce =>
                {
                    var entityLinks = CreateLinksForCampaignWooCommerce(campaignwoocommerce.Id, filterOptionsModel.Fields);
                    campaignwoocommerce.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = campaignwoocommercesFromRepo.TotalCount,
                    pageSize = campaignwoocommercesFromRepo.PageSize,
                    currentPage = campaignwoocommercesFromRepo.CurrentPage,
                    totalPages = campaignwoocommercesFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForCampaignWooCommerces(filterOptionsModel, campaignwoocommercesFromRepo.HasNext, campaignwoocommercesFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = campaignwoocommercesFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = campaignwoocommercesFromRepo.HasPrevious ?
                    CreateCampaignWooCommercesResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = campaignwoocommercesFromRepo.HasNext ?
                    CreateCampaignWooCommercesResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = campaignwoocommercesFromRepo.TotalCount,
                    pageSize = campaignwoocommercesFromRepo.PageSize,
                    currentPage = campaignwoocommercesFromRepo.CurrentPage,
                    totalPages = campaignwoocommercesFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(campaignwoocommercesFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.campaignwoocommerces.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetCampaignWooCommerce")]
        public async Task<ActionResult<CampaignWooCommerce>> GetCampaignWooCommerce(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object campaignwoocommerceEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetCampaignWooCommerce called");

                //then get the whole entity and map it to the Dto.
                campaignwoocommerceEntity = Mapper.Map<CampaignWooCommerceDto>(await _campaignwoocommerceService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                campaignwoocommerceEntity = await _campaignwoocommerceService.GetPartialEntityAsync(id, fields);
            }

            //if campaignwoocommerce not found.
            if (campaignwoocommerceEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.campaignwoocommerces.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForCampaignWooCommerce(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((CampaignWooCommerceDto)campaignwoocommerceEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = campaignwoocommerceEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = campaignwoocommerceEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateCampaignWooCommerce")]
        public async Task<IActionResult> UpdateCampaignWooCommerce(Guid id, [FromBody]CampaignWooCommerceForUpdate CampaignWooCommerceForUpdate)
        {

            //if show not found
            if (!await _campaignwoocommerceService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _campaignwoocommerceService.UpdateEntityAsync(id, CampaignWooCommerceForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateCampaignWooCommerce(Guid id, [FromBody] JsonPatchDocument<CampaignWooCommerceForUpdate> jsonPatchDocument)
        {
            CampaignWooCommerceForUpdate dto = new CampaignWooCommerceForUpdate();
            CampaignWooCommerce campaignwoocommerce = new CampaignWooCommerce();

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
            Mapper.Map(dto, campaignwoocommerce);

            //set the Id for the show model.
            campaignwoocommerce.Id = id;

            //partially update the chnages to the db. 
            await _campaignwoocommerceService.UpdatePartialEntityAsync(campaignwoocommerce, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateCampaignWooCommerce")]
        public async Task<ActionResult<CampaignWooCommerceDto>> CreateCampaignWooCommerce([FromBody]CampaignWooCommerceForCreation campaignwoocommerce)
        {
            //create a show in db.
            var campaignwoocommerceToReturn = await _campaignwoocommerceService.CreateEntityAsync<CampaignWooCommerceDto, CampaignWooCommerceForCreation>(campaignwoocommerce);

            //return the show created response.
            return CreatedAtRoute("GetCampaignWooCommerce", new { id = campaignwoocommerceToReturn.Id }, campaignwoocommerceToReturn);
        }


        [HttpGet("GetWcReports", Name = "GetWcReports")]
        public async Task<RootWcReportData> GetWcReports(Guid campaignId, string startDate, string endDate)
        {
            var reportData = new RootWcReportData();
            try
            {
                reportData = await _campaignwoocommerceService.GetWcReports(campaignId, startDate, endDate);
                return reportData;
            }
            catch (Exception ex)
            {
                reportData.ErrorMessage = ex.Message;
                return reportData;
            }
        }

        [HttpGet("validate-shop", Name = "validate-shop")]
        public async Task<WcValidate> ValidateShop(string shopUrl, string consumerKey, string consumerSecret)
        {
            var response = new WcValidate();
            try
            {
                response = await _campaignwoocommerceService.VaidateWcShop(shopUrl, consumerKey, consumerSecret);
                return response;
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
                response.ErrorMessage = ex.StackTrace;
                return response;
            }
        }


        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteCampaignWooCommerceById")]
        public async Task<IActionResult> DeleteCampaignWooCommerceById(Guid id)
        {
            //if the campaignwoocommerce exists
            if (await _campaignwoocommerceService.ExistAsync(x => x.Id == id))
            {
                //delete the campaignwoocommerce from the db.
                await _campaignwoocommerceService.DeleteEntityAsync(id);
            }
            else
            {
                //if campaignwoocommerce doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForCampaignWooCommerce(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignWooCommerce", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignWooCommerce", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteCampaignWooCommerceById", new { id = id }),
              "delete_campaignwoocommerce",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateCampaignWooCommerce", new { id = id }),
             "update_campaignwoocommerce",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateCampaignWooCommerce", new { }),
              "create_campaignwoocommerce",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCampaignWooCommerces(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateCampaignWooCommercesResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateCampaignWooCommercesResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateCampaignWooCommercesResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateCampaignWooCommercesResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredCampaignWooCommerces",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredCampaignWooCommerces",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredCampaignWooCommerces",
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
