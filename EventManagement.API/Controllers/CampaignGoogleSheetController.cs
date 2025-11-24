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
    /// CampaignGoogleSheet endpoint
    /// </summary>
    [Route("api/campaigngooglesheets")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
    public class CampaignGoogleSheetController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignGoogleSheetService _campaigngooglesheetService;
        private ILogger<CampaignGoogleSheetController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public CampaignGoogleSheetController(ICampaignGoogleSheetService campaigngooglesheetService, ILogger<CampaignGoogleSheetController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _campaigngooglesheetService = campaigngooglesheetService;
            _urlHelper = urlHelper;
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredCampaignGoogleSheets")]
        [Produces("application/vnd.tourmanagement.campaigngooglesheets.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CampaignGoogleSheetDto>>> GetFilteredCampaignGoogleSheets([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_campaigngooglesheetService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<CampaignGoogleSheetDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var campaigngooglesheetsFromRepo = await _campaigngooglesheetService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.campaigngooglesheets.hateoas+json")
            {
                //create HATEOAS links for each show.
                campaigngooglesheetsFromRepo.ForEach(campaigngooglesheet =>
                {
                    var entityLinks = CreateLinksForCampaignGoogleSheet(campaigngooglesheet.Id, filterOptionsModel.Fields);
                    campaigngooglesheet.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = campaigngooglesheetsFromRepo.TotalCount,
                    pageSize = campaigngooglesheetsFromRepo.PageSize,
                    currentPage = campaigngooglesheetsFromRepo.CurrentPage,
                    totalPages = campaigngooglesheetsFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForCampaignGoogleSheets(filterOptionsModel, campaigngooglesheetsFromRepo.HasNext, campaigngooglesheetsFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = campaigngooglesheetsFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = campaigngooglesheetsFromRepo.HasPrevious ?
                    CreateCampaignGoogleSheetsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = campaigngooglesheetsFromRepo.HasNext ?
                    CreateCampaignGoogleSheetsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = campaigngooglesheetsFromRepo.TotalCount,
                    pageSize = campaigngooglesheetsFromRepo.PageSize,
                    currentPage = campaigngooglesheetsFromRepo.CurrentPage,
                    totalPages = campaigngooglesheetsFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(campaigngooglesheetsFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.campaigngooglesheets.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetCampaignGoogleSheet")]
        public async Task<ActionResult<CampaignGoogleSheet>> GetCampaignGoogleSheet(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object campaigngooglesheetEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetCampaignGoogleSheet called");

                //then get the whole entity and map it to the Dto.
                campaigngooglesheetEntity = Mapper.Map<CampaignGoogleSheetDto>(await _campaigngooglesheetService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                campaigngooglesheetEntity = await _campaigngooglesheetService.GetPartialEntityAsync(id, fields);
            }

            //if campaigngooglesheet not found.
            if (campaigngooglesheetEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.campaigngooglesheets.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForCampaignGoogleSheet(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((CampaignGoogleSheetDto)campaigngooglesheetEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = campaigngooglesheetEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = campaigngooglesheetEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }


        [HttpGet("GetGoogleAccount", Name = "GetGoogleAccount")]        
        public async Task<GoogleAccountDto> GetGoogleAccount(Guid campaignId)
        {         
            try
            {
                var data = await _campaigngooglesheetService.GetGoogleAccountDetails(campaignId);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }          
        }

        [HttpGet("GetAllSpreadSheet", Name = "GetAllSpreadSheet")]
       
        public async Task<List<DriveFile>> GetAllSpreadSheet(Guid campaignId)
        {
            try
            {
                var data = await _campaigngooglesheetService.GetListSpreadSheet(campaignId);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet("GetAllSheetsById", Name = "GetAllSheetsById")]
       
        public async Task<List<SheetProperties>> GetAllSpreadSheet(Guid campaignId,string spreadSheetId)
        {
            try
            {
                var data = await _campaigngooglesheetService.GetListSheets(campaignId,spreadSheetId);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost("GetGoogleSheetReport", Name = "GetGoogleSheetReport")]
        [AllowAnonymous]
        public async Task<List<GoogleSheetData>>  GetGoogleSheetReport(List<GoogleSheetSettingsDto> settingDto)
        {                                                   
            var data = await _campaigngooglesheetService.GetGoogleSheetReport(settingDto);

                return data;
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateCampaignGoogleSheet")]
        public async Task<IActionResult> UpdateCampaignGoogleSheet(Guid id, [FromBody]CampaignGoogleSheetForUpdate CampaignGoogleSheetForUpdate)
        {

            //if show not found
            if (!await _campaigngooglesheetService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _campaigngooglesheetService.UpdateEntityAsync(id, CampaignGoogleSheetForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateCampaignGoogleSheet(Guid id, [FromBody] JsonPatchDocument<CampaignGoogleSheetForUpdate> jsonPatchDocument)
        {
            CampaignGoogleSheetForUpdate dto = new CampaignGoogleSheetForUpdate();
            CampaignGoogleSheet campaigngooglesheet = new CampaignGoogleSheet();

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
            Mapper.Map(dto, campaigngooglesheet);

            //set the Id for the show model.
            campaigngooglesheet.Id = id;

            //partially update the chnages to the db. 
            await _campaigngooglesheetService.UpdatePartialEntityAsync(campaigngooglesheet, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateCampaignGoogleSheet")]
        public async Task<ActionResult<CampaignGoogleSheetDto>> CreateCampaignGoogleSheet([FromBody]CampaignGoogleSheetForCreation campaigngooglesheet)
        {
            //create a show in db.
            var campaigngooglesheetToReturn = await _campaigngooglesheetService.CreateEntityAsync<CampaignGoogleSheetDto, CampaignGoogleSheetForCreation>(campaigngooglesheet);

            //return the show created response.
            return CreatedAtRoute("GetCampaignGoogleSheet", new { id = campaigngooglesheetToReturn.Id }, campaigngooglesheetToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteCampaignGoogleSheetById")]
        public async Task<IActionResult> DeleteCampaignGoogleSheetById(Guid id)
        {
            //if the campaigngooglesheet exists
            if (await _campaigngooglesheetService.ExistAsync(x => x.Id == id))
            {
                //delete the campaigngooglesheet from the db.
                await _campaigngooglesheetService.DeleteEntityAsync(id);
            }
            else
            {
                //if campaigngooglesheet doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForCampaignGoogleSheet(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignGoogleSheet", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignGoogleSheet", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteCampaignGoogleSheetById", new { id = id }),
              "delete_campaigngooglesheet",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateCampaignGoogleSheet", new { id = id }),
             "update_campaigngooglesheet",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateCampaignGoogleSheet", new { }),
              "create_campaigngooglesheet",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCampaignGoogleSheets(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateCampaignGoogleSheetsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateCampaignGoogleSheetsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateCampaignGoogleSheetsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateCampaignGoogleSheetsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredCampaignGoogleSheets",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredCampaignGoogleSheets",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredCampaignGoogleSheets",
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
