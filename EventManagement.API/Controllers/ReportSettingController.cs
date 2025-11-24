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
using System.Linq;
using IdentityServer4.AccessTokenValidation;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// ReportSetting endpoint
    /// </summary>
    [Route("api/reportsettings")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class ReportSettingController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly IReportSettingService _reportsettingService;
        private ILogger<ReportSettingController> _logger;
        private readonly IUrlHelper _urlHelper;
        private readonly ICampaignService _campaignService;

        #endregion


        #region CONSTRUCTOR

        public ReportSettingController(ICampaignService campaignService, IReportSettingService reportsettingService, ILogger<ReportSettingController> logger, IUrlHelper urlHelper)
        {
            _logger = logger;
            _reportsettingService = reportsettingService;
            _urlHelper = urlHelper;
            _campaignService = campaignService;
        }

        #endregion


        #region HTTPGET

        [HttpGet("GetReportById", Name = "GetReportById")]
        [Produces("application/vnd.tourmanagement.reportsettings.hateoasjson", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ReportSettingDto>>> GetReportById(Guid reportId)
        {
            var reportData = _reportsettingService.GetEntityById(reportId);
            return Ok(reportData);
        }

        [HttpGet("GetReportByCampaign", Name = "GetReportByCampaign")]
        [Produces("application/vnd.tourmanagement.reportsettings.hateoasjson", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<List<ReportSetting>> GetReportByCampaign(string campaignId,Guid companyId)
        {
            var reportData =  _reportsettingService.GetReportByCampaign(campaignId, companyId);

            //var reportDataGroup = reportData.GroupBy(x => x.CampaignId).ToList();

            //foreach (var report in reportDataGroup)
            //{
            //    ReportByCampaignDto entity = new ReportByCampaignDto();
            //    entity.CampaignId = report.Key;
            //    entity.CampaignName = _campaignService.GetEntityById(report.Key).Name;
            //    entity.ReportList = Mapper.Map<List<ReportSettingDto>>(report.ToList());

            //    returnData.Add(entity);
            //}

            return Ok(reportData);
        }

        [HttpGet(Name = "GetFilteredReportSettings")]
        [Produces("application/vnd.tourmanagement.reportsettings.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ReportSettingDto>>> GetFilteredReportSettings([FromQuery] FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_reportsettingService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<ReportSettingDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var reportsettingsFromRepo = await _reportsettingService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            //if (mediaType == "application/vnd.tourmanagement.reportsettings.hateoas+json")
            //{
            //    //create HATEOAS links for each show.
            //    reportsettingsFromRepo.ForEach(reportsetting =>
            //    {
            //        var entityLinks = CreateLinksForReportSetting(reportsetting.Id, filterOptionsModel.Fields);
            //        reportsetting.links = entityLinks;
            //    });

            //    //prepare pagination metadata.
            //    var paginationMetadata = new
            //    {
            //        totalCount = reportsettingsFromRepo.TotalCount,
            //        pageSize = reportsettingsFromRepo.PageSize,
            //        currentPage = reportsettingsFromRepo.CurrentPage,
            //        totalPages = reportsettingsFromRepo.TotalPages,
            //    };

            //    //add pagination meta data to response header.
            //    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

            //    //create links for shows.
            //    var links = CreateLinksForReportSettings(filterOptionsModel, reportsettingsFromRepo.HasNext, reportsettingsFromRepo.HasPrevious);

            //    //prepare model with data and HATEOAS links.
            //    var linkedCollectionResource = new
            //    {
            //        value = reportsettingsFromRepo,
            //        links = links
            //    };

            //    //return the data with Ok response.
            //    return Ok(linkedCollectionResource);
            //}
            //else
            //{
            //    var previousPageLink = reportsettingsFromRepo.HasPrevious ?
            //        CreateReportSettingsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

            //    var nextPageLink = reportsettingsFromRepo.HasNext ?
            //        CreateReportSettingsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

            //    //prepare the pagination metadata.
            //    var paginationMetadata = new
            //    {
            //        previousPageLink = previousPageLink,
            //        nextPageLink = nextPageLink,
            //        totalCount = reportsettingsFromRepo.TotalCount,
            //        pageSize = reportsettingsFromRepo.PageSize,
            //        currentPage = reportsettingsFromRepo.CurrentPage,
            //        totalPages = reportsettingsFromRepo.TotalPages
            //    };

            //    //add pagination meta data to response header.
            //    Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

            //    //return the data with Ok response.
            //    return Ok(reportsettingsFromRepo);
            //}
            return Ok(reportsettingsFromRepo);
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.reportsettings.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetReportSetting")]
        public async Task<ActionResult<ReportSetting>> GetReportSetting(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object reportsettingEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetReportSetting called");

                //then get the whole entity and map it to the Dto.
                reportsettingEntity = Mapper.Map<ReportSettingDto>(await _reportsettingService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                reportsettingEntity = await _reportsettingService.GetPartialEntityAsync(id, fields);
            }

            //if reportsetting not found.
            if (reportsettingEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.reportsettings.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForReportSetting(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((ReportSettingDto)reportsettingEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = reportsettingEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = reportsettingEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateReportSetting")]
        public async Task<IActionResult> UpdateReportSetting(Guid id, [FromBody] ReportSettingForUpdate ReportSettingForUpdate)
        {

            //if show not found
            if (!await _reportsettingService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _reportsettingService.UpdateEntityAsync(id, ReportSettingForUpdate);

            //return the response.
            return Ok("updated");
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateReportSetting(Guid id, [FromBody] JsonPatchDocument<ReportSettingForUpdate> jsonPatchDocument)
        {
            ReportSettingForUpdate dto = new ReportSettingForUpdate();
            ReportSetting reportsetting = new ReportSetting();

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
            Mapper.Map(dto, reportsetting);

            //set the Id for the show model.
            reportsetting.Id = id;

            //partially update the chnages to the db. 
            await _reportsettingService.UpdatePartialEntityAsync(reportsetting, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateReportSetting")]
        public async Task<ActionResult<ReportSettingDto>> CreateReportSetting([FromBody] ReportSettingForCreation reportsetting)
        {
            //create a show in db.
            var reportsettingToReturn = await _reportsettingService.CreateEntityAsync<ReportSettingDto, ReportSettingForCreation>(reportsetting);

            //return the show created response.
            return Ok(reportsettingToReturn); // CreatedAtRoute("GetReportSetting", new { id = reportsettingToReturn.Id }, reportsettingToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteReportSettingById")]
        public async Task<IActionResult> DeleteReportSettingById(Guid id)
        {
            //if the reportsetting exists
            if (await _reportsettingService.ExistAsync(x => x.Id == id))
            {
                //delete the reportsetting from the db.
                await _reportsettingService.DeleteEntityAsync(id);
            }
            else
            {
                //if reportsetting doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return Ok(id);
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForReportSetting(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetReportSetting", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetReportSetting", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteReportSettingById", new { id = id }),
              "delete_reportsetting",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateReportSetting", new { id = id }),
             "update_reportsetting",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateReportSetting", new { }),
              "create_reportsetting",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForReportSettings(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateReportSettingsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateReportSettingsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateReportSettingsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateReportSettingsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredReportSettings",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredReportSettings",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredReportSettings",
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
