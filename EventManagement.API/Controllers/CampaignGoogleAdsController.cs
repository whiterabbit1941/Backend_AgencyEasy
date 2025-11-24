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

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// CampaignGoogleAds endpoint
    /// </summary>
    [Route("api/campaigngoogleadss")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]

    public class CampaignGoogleAdsController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignGoogleAdsService _campaigngoogleadsService;
        private ILogger<CampaignGoogleAdsController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public CampaignGoogleAdsController(ICampaignGoogleAdsService campaigngoogleadsService, ILogger<CampaignGoogleAdsController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _campaigngoogleadsService = campaigngoogleadsService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredCampaignGoogleAdss")]
        [Produces("application/vnd.tourmanagement.campaigngoogleadss.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CampaignGoogleAdsDto>>> GetFilteredCampaignGoogleAdss([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_campaigngoogleadsService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<CampaignGoogleAdsDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var campaigngoogleadssFromRepo = await _campaigngoogleadsService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.campaigngoogleadss.hateoas+json")
            {
                //create HATEOAS links for each show.
                campaigngoogleadssFromRepo.ForEach(campaigngoogleads =>
                {
                    var entityLinks = CreateLinksForCampaignGoogleAds(campaigngoogleads.Id, filterOptionsModel.Fields);
                    campaigngoogleads.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = campaigngoogleadssFromRepo.TotalCount,
                    pageSize = campaigngoogleadssFromRepo.PageSize,
                    currentPage = campaigngoogleadssFromRepo.CurrentPage,
                    totalPages = campaigngoogleadssFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForCampaignGoogleAdss(filterOptionsModel, campaigngoogleadssFromRepo.HasNext, campaigngoogleadssFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = campaigngoogleadssFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = campaigngoogleadssFromRepo.HasPrevious ?
                    CreateCampaignGoogleAdssResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = campaigngoogleadssFromRepo.HasNext ?
                    CreateCampaignGoogleAdssResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = campaigngoogleadssFromRepo.TotalCount,
                    pageSize = campaigngoogleadssFromRepo.PageSize,
                    currentPage = campaigngoogleadssFromRepo.CurrentPage,
                    totalPages = campaigngoogleadssFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(campaigngoogleadssFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.campaigngoogleadss.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetCampaignGoogleAds")]
        public async Task<ActionResult<CampaignGoogleAds>> GetCampaignGoogleAds(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object campaigngoogleadsEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetCampaignGoogleAds called");

                //then get the whole entity and map it to the Dto.
                campaigngoogleadsEntity = Mapper.Map<CampaignGoogleAdsDto>(await _campaigngoogleadsService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                campaigngoogleadsEntity = await _campaigngoogleadsService.GetPartialEntityAsync(id, fields);
            }

            //if campaigngoogleads not found.
            if (campaigngoogleadsEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.campaigngoogleadss.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForCampaignGoogleAds(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((CampaignGoogleAdsDto)campaigngoogleadsEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = campaigngoogleadsEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = campaigngoogleadsEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }


        [HttpGet("CheckGAdsIntegratedByCampaignId", Name = "CheckGAdsIntegratedByCampaignId")]
        public async Task<bool> CheckGAdsIntegratedByCampaignId(string campaignId)
        {
            var isExists = _campaigngoogleadsService.GetAllEntities().Where(x => x.CampaignID.ToString() == campaignId).Any();
            if (isExists)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateCampaignGoogleAds")]
        public async Task<IActionResult> UpdateCampaignGoogleAds(Guid id, [FromBody]CampaignGoogleAdsForUpdate CampaignGoogleAdsForUpdate)
        {

            //if show not found
            if (!await _campaigngoogleadsService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _campaigngoogleadsService.UpdateEntityAsync(id, CampaignGoogleAdsForUpdate);

            //return the response.
            return NoContent();
        }

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("UpdateCampaignGoogleAdsByCampaignId", Name = "UpdateCampaignGoogleAdsByCampaignId")]
        public async Task<IActionResult> UpdateCampaignGoogleAdsByCampaignId([FromBody] CampaignGoogleAdsForUpdate CampaignGoogleAdsForUpdate)
        {
            try
            {
                var temp = await _campaigngoogleadsService.UpdateBulkEntityAsync(x => new CampaignGoogleAds
                {
                    LoginCustomerID = CampaignGoogleAdsForUpdate.LoginCustomerID,
                    EmailId = CampaignGoogleAdsForUpdate
                .EmailId
                }, y => y.CampaignID == CampaignGoogleAdsForUpdate.CampaignID);

                //return the response.
                return Ok(true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateCampaignGoogleAds(Guid id, [FromBody] JsonPatchDocument<CampaignGoogleAdsForUpdate> jsonPatchDocument)
        {
            CampaignGoogleAdsForUpdate dto = new CampaignGoogleAdsForUpdate();
            CampaignGoogleAds campaigngoogleads = new CampaignGoogleAds();

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
            Mapper.Map(dto, campaigngoogleads);

            //set the Id for the show model.
            campaigngoogleads.Id = id;

            //partially update the chnages to the db. 
            await _campaigngoogleadsService.UpdatePartialEntityAsync(campaigngoogleads, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateCampaignGoogleAds")]
        public async Task<ActionResult<CampaignGoogleAdsDto>> CreateCampaignGoogleAds([FromBody]CampaignGoogleAdsForCreation campaigngoogleads)
        {
                //create a show in db.
                var campaigngoogleadsToReturn = await _campaigngoogleadsService.CreateEntityAsync<CampaignGoogleAdsDto, CampaignGoogleAdsForCreation>(campaigngoogleads);

                //return the show created response.
                return CreatedAtRoute("GetCampaignGoogleAds", new { id = campaigngoogleadsToReturn.Id }, campaigngoogleadsToReturn);           
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]     
        [HttpPost("GetListOfGaAdsCustomer", Name = "GetListOfGaAdsCustomer")]
        public List<GoogleAdsCustomerDto> GetListOfGaAdsCustomer(Guid campaignId)
        {
            var details = _campaigngoogleadsService.GetAllEntities().Where(x => x.CampaignID == campaignId).FirstOrDefault();
            //create a show in db.
            var gaCustomer = _campaigngoogleadsService.GetListOfGaAdsCustomer(details.RefreshToken);
          
                return gaCustomer;

        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("GetGoogleAdsReports", Name = "GetGoogleAdsReports")]
        public List<GoogleAdsCampaignReport> GetGoogleAdsReports(string campaignid, string startDate, string endDate, int reportType)
        {           
            var res = _campaigngoogleadsService.GetGoogleAdsReports(campaignid, startDate, endDate, reportType);
            
            return res;

        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("UpdateRefreshTokenAndEmailGAds", Name = "UpdateRefreshTokenAndEmailGAds")]
        public async Task<ActionResult<bool>> UpdateRefreshTokenAndEmail([FromBody] CampaignGoogleAdsForCreation campaigngoogleads)
        {

            var isCompanyIdExist = Request.Headers.ContainsKey("SelectedCompanyId");
            if (isCompanyIdExist)
            {
                Request.Headers.TryGetValue("SelectedCompanyId", out Microsoft.Extensions.Primitives.StringValues companyId);

                return await _campaigngoogleadsService.UpdateRefreshTokenAndEmail(campaigngoogleads, companyId);
            }

            //return the show created response.
            return false;
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteCampaignGoogleAdsById")]
        public async Task<IActionResult> DeleteCampaignGoogleAdsById(Guid id)
        {
            //if the campaigngoogleads exists
            if (await _campaigngoogleadsService.ExistAsync(x => x.Id == id))
            {
                //delete the campaigngoogleads from the db.
                await _campaigngoogleadsService.DeleteEntityAsync(id);
            }
            else
            {
                //if campaigngoogleads doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForCampaignGoogleAds(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignGoogleAds", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignGoogleAds", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteCampaignGoogleAdsById", new { id = id }),
              "delete_campaigngoogleads",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateCampaignGoogleAds", new { id = id }),
             "update_campaigngoogleads",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateCampaignGoogleAds", new { }),
              "create_campaigngoogleads",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCampaignGoogleAdss(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateCampaignGoogleAdssResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateCampaignGoogleAdssResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateCampaignGoogleAdssResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateCampaignGoogleAdssResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredCampaignGoogleAdss",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredCampaignGoogleAdss",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredCampaignGoogleAdss",
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
