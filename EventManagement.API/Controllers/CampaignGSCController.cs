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
using EventManagement.Utility.Enums;
using System.Linq;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// CampaignGSC endpoint
    /// </summary>
    [Route("api/campaigngscs")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]

    public class CampaignGSCController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignGSCService _campaigngscService;
        private readonly ICampaignService _campaignService;
        private ILogger<CampaignGSCController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public CampaignGSCController(ICampaignService campaignService, ICampaignGSCService campaigngscService, ILogger<CampaignGSCController> logger, IUrlHelper urlHelper)
        {
            _logger = logger;
            _campaigngscService = campaigngscService;
            _urlHelper = urlHelper;
            _campaignService = campaignService;
        }

        #endregion


        #region HTTPGET

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetCampaignGscDataById", Name = "GetCampaignGscDataById")]
        public async Task<ActionResult<GscData>> GetCampaignGscDataById(Guid campaignId, string startDate, string endDate)
        {
            var gscData = await _campaigngscService.GetCampaignGscDataById(campaignId, startDate, endDate);

            return Ok(gscData);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetCampaignGscChartDataWithDateById", Name = "GetCampaignGscChartDataWithDateById")]
        public async Task<ActionResult<GscChartResponse>> GetCampaignGscChartDataWithDateById(Guid campaignId, string startDate, string endDate)
        {
            var gscChartData = await _campaigngscService.GetCampaignGscChartDataWithDateById(campaignId, startDate, endDate);

            return Ok(gscChartData);
        }

        [HttpGet(Name = "GetFilteredCampaignGSCs")]
        [Produces("application/vnd.tourmanagement.campaigngscs.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CampaignGSCDto>>> GetFilteredCampaignGSCs([FromQuery] FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_campaigngscService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<CampaignGSCDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var campaigngscsFromRepo = await _campaigngscService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.campaigngscs.hateoas+json")
            {
                //create HATEOAS links for each show.
                campaigngscsFromRepo.ForEach(campaigngsc =>
                {
                    var entityLinks = CreateLinksForCampaignGSC(campaigngsc.Id, filterOptionsModel.Fields);
                    campaigngsc.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = campaigngscsFromRepo.TotalCount,
                    pageSize = campaigngscsFromRepo.PageSize,
                    currentPage = campaigngscsFromRepo.CurrentPage,
                    totalPages = campaigngscsFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForCampaignGSCs(filterOptionsModel, campaigngscsFromRepo.HasNext, campaigngscsFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = campaigngscsFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = campaigngscsFromRepo.HasPrevious ?
                    CreateCampaignGSCsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = campaigngscsFromRepo.HasNext ?
                    CreateCampaignGSCsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = campaigngscsFromRepo.TotalCount,
                    pageSize = campaigngscsFromRepo.PageSize,
                    currentPage = campaigngscsFromRepo.CurrentPage,
                    totalPages = campaigngscsFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(campaigngscsFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.campaigngscs.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetCampaignGSC")]
        public async Task<ActionResult<CampaignGSC>> GetCampaignGSC(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object campaigngscEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetCampaignGSC called");

                //then get the whole entity and map it to the Dto.
                campaigngscEntity = Mapper.Map<CampaignGSCDto>(await _campaigngscService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                campaigngscEntity = await _campaigngscService.GetPartialEntityAsync(id, fields);
            }

            //if campaigngsc not found.
            if (campaigngscEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.campaigngscs.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForCampaignGSC(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((CampaignGSCDto)campaigngscEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = campaigngscEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = campaigngscEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        [HttpGet("CheckGSCIntegratedByCampaignId", Name = "CheckGSCIntegratedByCampaignId")]
        public async Task<bool> CheckGSCIntegratedByCampaignId(string campaignId)
        {
            var isExists = _campaigngscService.GetAllEntities().Where(x => x.CampaignID.ToString() == campaignId).Any();
            if (isExists)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [HttpGet("GetGSCList", Name = "GetGSCList")]
        public async Task<RootObjectOfGSCList> GetGSCList(Guid campaignId)
        {
            var list = new RootObjectOfGSCList();
            try
            {
                list = await _campaigngscService.GetGSCList(campaignId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return list;
        }
        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateCampaignGSC")]
        public async Task<IActionResult> UpdateCampaignGSC(Guid id, [FromBody] CampaignGSCForUpdate CampaignGSCForUpdate)
        {

            //if show not found
            if (!await _campaigngscService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _campaigngscService.UpdateEntityAsync(id, CampaignGSCForUpdate);

            //return the response.
            return NoContent();
        }

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("UpdateCampaignGSCByCampaignId", Name = "UpdateCampaignGSCByCampaignId")]
        public async Task<IActionResult> UpdateCampaignGSCByCampaignId([FromBody]CampaignGSCForUpdate CampaignGSCForUpdate)
        {
            try
            {
                var temp = await _campaigngscService.UpdateBulkEntityAsync(x => new CampaignGSC
                {
                    UrlOrName = CampaignGSCForUpdate.UrlOrName,
                    EmailId = CampaignGSCForUpdate.EmailId,
                }, y => y.CampaignID == CampaignGSCForUpdate.CampaignID);

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
        public async Task<IActionResult> PartiallyUpdateCampaignGSC(Guid id, [FromBody] JsonPatchDocument<CampaignGSCForUpdate> jsonPatchDocument)
        {
            CampaignGSCForUpdate dto = new CampaignGSCForUpdate();
            CampaignGSC campaigngsc = new CampaignGSC();

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
            Mapper.Map(dto, campaigngsc);

            //set the Id for the show model.
            campaigngsc.Id = id;

            //partially update the chnages to the db. 
            await _campaigngscService.UpdatePartialEntityAsync(campaigngsc, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateCampaignGSC")]
        public async Task<ActionResult<CampaignGSCDto>> CreateCampaignGSC([FromBody] CampaignGSCForCreation campaigngsc)
        {
            //create a show in db.
            var campaigngscToReturn = await _campaigngscService.CreateEntityAsync<CampaignGSCDto, CampaignGSCForCreation>(campaigngsc);

            //return the show created response.
            return CreatedAtRoute("GetCampaignGSC", new { id = campaigngscToReturn.Id }, campaigngscToReturn);
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("UpdateRefreshTokenAndEmailGSC", Name = "UpdateRefreshTokenAndEmailGSC")]
        public async Task<ActionResult<bool>> UpdateRefreshTokenAndEmail([FromBody] CampaignGSCForCreation campaigngsc)
        {

            var isCompanyIdExist = Request.Headers.ContainsKey("SelectedCompanyId");
            if (isCompanyIdExist)
            {
                Request.Headers.TryGetValue("SelectedCompanyId", out Microsoft.Extensions.Primitives.StringValues companyId);

                return await _campaigngscService.UpdateRefreshTokenAndEmail(campaigngsc, companyId);
            }

            //return the show created response.
            return false;
        }


        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteCampaignGSCById")]
        public async Task<IActionResult> DeleteCampaignGSCById(Guid id)
        {
            //if the campaigngsc exists
            if (await _campaigngscService.ExistAsync(x => x.Id == id))
            {
                //Update dashboard table 
                await _campaignService.UpdateDashboardTable(id, (int)ReportTypes.GoogleSearchConsole);
                //delete the campaigngsc from the db.
                await _campaigngscService.DeleteEntityAsync(id);
            }
            else
            {
                //if campaigngsc doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForCampaignGSC(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignGSC", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignGSC", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteCampaignGSCById", new { id = id }),
              "delete_campaigngsc",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateCampaignGSC", new { id = id }),
             "update_campaigngsc",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateCampaignGSC", new { }),
              "create_campaigngsc",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCampaignGSCs(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateCampaignGSCsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateCampaignGSCsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateCampaignGSCsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateCampaignGSCsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredCampaignGSCs",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredCampaignGSCs",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredCampaignGSCs",
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
