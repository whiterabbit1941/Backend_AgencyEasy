using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Service;
using EventManagement.Domain.Entities;
using EventManagement.Utility;
using Microsoft.Extensions.Logging;
using IdentityServer4.AccessTokenValidation;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// GoogleAnalyticsAccount endpoint
    /// </summary>
    [Route("api/googleanalyticsaccounts")]
    [Produces("application/json")]
    [ApiController]    
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]

    public class GoogleAnalyticsAccountController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly IGoogleAnalyticsAccountService _googleanalyticsaccountService;      
        private ILogger<GoogleAnalyticsAccountController> _logger;
        private readonly IUrlHelper _urlHelper;

        #endregion


        #region CONSTRUCTOR

        public GoogleAnalyticsAccountController(IGoogleAnalyticsAccountService googleanalyticsaccountService, ILogger<GoogleAnalyticsAccountController> logger, IUrlHelper urlHelper) 
        {
            _logger = logger;
            _googleanalyticsaccountService = googleanalyticsaccountService;
            _urlHelper = urlHelper;           
        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredGoogleAnalyticsAccounts")]
        [Produces("application/vnd.tourmanagement.googleanalyticsaccounts.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<GoogleAnalyticsAccountDto>>> GetFilteredGoogleAnalyticsAccounts([FromQuery]FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_googleanalyticsaccountService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<GoogleAnalyticsAccountDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var googleanalyticsaccountsFromRepo = await _googleanalyticsaccountService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.googleanalyticsaccounts.hateoas+json")
            {
                //create HATEOAS links for each show.
                googleanalyticsaccountsFromRepo.ForEach(googleanalyticsaccount =>
                {
                    var entityLinks = CreateLinksForGoogleAnalyticsAccount(googleanalyticsaccount.Id, filterOptionsModel.Fields);
                    googleanalyticsaccount.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = googleanalyticsaccountsFromRepo.TotalCount,
                    pageSize = googleanalyticsaccountsFromRepo.PageSize,
                    currentPage = googleanalyticsaccountsFromRepo.CurrentPage,
                    totalPages = googleanalyticsaccountsFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForGoogleAnalyticsAccounts(filterOptionsModel, googleanalyticsaccountsFromRepo.HasNext, googleanalyticsaccountsFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = googleanalyticsaccountsFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = googleanalyticsaccountsFromRepo.HasPrevious ?
                    CreateGoogleAnalyticsAccountsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = googleanalyticsaccountsFromRepo.HasNext ?
                    CreateGoogleAnalyticsAccountsResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = googleanalyticsaccountsFromRepo.TotalCount,
                    pageSize = googleanalyticsaccountsFromRepo.PageSize,
                    currentPage = googleanalyticsaccountsFromRepo.CurrentPage,
                    totalPages = googleanalyticsaccountsFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(googleanalyticsaccountsFromRepo);
            }
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]        
        [HttpGet("{id}", Name = "GetGoogleAnalyticsAccount")]
        public async Task<ActionResult<GoogleAnalyticsAccount>> GetGoogleAnalyticsAccount(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object googleanalyticsaccountEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetGoogleAnalyticsAccount called");

                //then get the whole entity and map it to the Dto.
                googleanalyticsaccountEntity = Mapper.Map<GoogleAnalyticsAccountDto>(await _googleanalyticsaccountService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                googleanalyticsaccountEntity = await _googleanalyticsaccountService.GetPartialEntityAsync(id, fields);
            }

            //if googleanalyticsaccount not found.
            if (googleanalyticsaccountEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.googleanalyticsaccounts.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForGoogleAnalyticsAccount(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((GoogleAnalyticsAccountDto)googleanalyticsaccountEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = googleanalyticsaccountEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = googleanalyticsaccountEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetGaAccountByCampaignID", Name = "GetGaAccountByCampaignID")]
        public async Task<ActionResult<List<GoogleAnalyticsAccountDto>>> GetGaAccountByCampaignID([FromQuery]string id)
        {
           var gaSetup = _googleanalyticsaccountService.GetGaAccountByCampaignID(id);
           return Ok(gaSetup);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetGaAnalyticsReports", Name = "GetGaAnalyticsReports")]
        public ActionResult<GaReportsDto> GetGaAnalyticsReports([FromQuery]string id,string startDate,string endDate)
        {
            var gaSetup =  _googleanalyticsaccountService.GetGaAnalyticsReports(id,startDate,endDate);
            return Ok(gaSetup);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetTrafficSourcesReports", Name = "GetTrafficSourcesReports")]
        public async Task<ActionResult<ListTrafficSource>>  GetTrafficSourcesReports([FromQuery]string id, string startDate, string endDate)
        {
            var gaSetup = await _googleanalyticsaccountService.GetTrafficSourcesReports(id, startDate, endDate);
            return Ok(gaSetup);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetTrafficSourcesMediumsReports", Name = "GetTrafficSourcesMediumsReports")]
        public ActionResult<SourcesMediums> GetTrafficSourcesMediumsReports([FromQuery]string id, string startDate, string endDate)
        {
            var gaSetup = _googleanalyticsaccountService.GetTrafficSourcesMediumsReports(id, startDate, endDate);
            return Ok(gaSetup);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetCampaignReports", Name = "GetCampaignReports")]
        public ActionResult<Campaigns> GetCampaignReports([FromQuery]string id, string startDate, string endDate)
        {
            var gaSetup = _googleanalyticsaccountService.GetCampaignReports(id, startDate, endDate);
            return Ok(gaSetup);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetAudienceReports", Name = "GetAudienceReports")]
        public ActionResult<Campaigns> GetAudienceReports([FromQuery]string id, string startDate, string endDate)
        {
            var gaSetup = _googleanalyticsaccountService.GetAudienceReports(id, startDate, endDate);
            return Ok(gaSetup);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetDeviceCategoryReports", Name = "GetDeviceCategoryReports")]
        public ActionResult<DeviceCategory> GetDeviceCategoryReports([FromQuery]string id, string startDate, string endDate)
        {
            var gaSetup = _googleanalyticsaccountService.GetDeviceCategoryReports(id, startDate, endDate);
            return Ok(gaSetup);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetGeoLocationReports", Name = "GetGeoLocationReports")]
        public ActionResult<GeoLocationDto> GetGeoLocationReports([FromQuery]string id, string startDate, string endDate)
        {
            var gaSetup = _googleanalyticsaccountService.GetGeoLocationReports(id, startDate, endDate);
            return Ok(gaSetup);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetLanguageReports", Name = "GetLanguageReports")]
        public ActionResult<GeoLocationDto> GetLanguageReports([FromQuery]string id, string startDate, string endDate)
        {
            var gaSetup = _googleanalyticsaccountService.GetLanguageReports(id, startDate, endDate);
            return Ok(gaSetup);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetGaBehaviorAnalyticsReports", Name = "GetGaBehaviorAnalyticsReports")]
        public ActionResult<BehaviorDto> GetGaBehaviorAnalyticsReports([FromQuery]string id, string startDate, string endDate)
        {
            var gaSetup = _googleanalyticsaccountService.GetGaBehaviorAnalyticsReports(id, startDate, endDate);
            return Ok(gaSetup);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetGaConversionsAnalyticsReports", Name = "GetGaConversionsAnalyticsReports")]
        public ActionResult<ConversionDto> GetGaConversionsAnalyticsReports([FromQuery]string id, string startDate, string endDate)
        {
            var gaSetup = _googleanalyticsaccountService.GetGaConversionsAnalyticsReports(id, startDate, endDate);
            return Ok(gaSetup);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateGoogleAnalyticsAccount")]
        public async Task<IActionResult> UpdateGoogleAnalyticsAccount(Guid id, [FromBody]GoogleAnalyticsAccountForUpdate GoogleAnalyticsAccountForUpdate)
        {

            //if show not found
            if (!await _googleanalyticsaccountService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _googleanalyticsaccountService.UpdateEntityAsync(id, GoogleAnalyticsAccountForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        //[Consumes("application/json-patch+json")]
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[HttpPatch("{id}")]
        //public async Task<IActionResult> PartiallyUpdateGoogleAnalyticsAccount(Guid id, [FromBody] JsonPatchDocument<GoogleAnalyticsAccountForUpdate> jsonPatchDocument)
        //{
        //    GoogleAnalyticsAccountForUpdate dto = new GoogleAnalyticsAccountForUpdate();
        //    GoogleAnalyticsAccount googleanalyticsaccount = new GoogleAnalyticsAccount();

        //    //apply the patch changes to the dto. 
        //    jsonPatchDocument.ApplyTo(dto, ModelState);

        //    //if the jsonPatchDocument is not valid.
        //    if (!ModelState.IsValid)
        //    {
        //        //then return unprocessableEntity response.
        //        return new UnprocessableEntityObjectResult(ModelState);
        //    }

        //    //if the dto model is not valid after applying changes.
        //    if (!TryValidateModel(dto))
        //    {
        //        //then return unprocessableEntity response.
        //        return new UnprocessableEntityObjectResult(ModelState);
        //    }

        //    //map the chnages from dto to entity.
        //    Mapper.Map(dto, googleanalyticsaccount);

        //    //set the Id for the show model.
        //    googleanalyticsaccount.Id = id;

        //    //partially update the chnages to the db. 
        //    await _googleanalyticsaccountService.UpdatePartialEntityAsync(googleanalyticsaccount, jsonPatchDocument);

        //    //return the response.
        //    return NoContent();
        //}


        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]        
        public async Task<IActionResult> UpdateGaAccountProfile(Guid id,[FromBody] JsonPatchDocument<GoogleAnalyticsAccountForUpdate> jsonPatchDocument)
        {

            var getCampaigns = await _googleanalyticsaccountService.InActiveAllGaAnalytics(id);

            GoogleAnalyticsAccountForUpdate dto = new GoogleAnalyticsAccountForUpdate();
            GoogleAnalyticsAccount googleanalyticsaccount = new GoogleAnalyticsAccount();

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
            Mapper.Map(dto, googleanalyticsaccount);

            //set the Id for the show model.
            googleanalyticsaccount.Id = id;

            //partially update the chnages to the db. 
            await _googleanalyticsaccountService.UpdatePartialEntityAsync(googleanalyticsaccount, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateGoogleAnalyticsAccount")]
        public async Task<ActionResult<GoogleAnalyticsAccountDto>> CreateGoogleAnalyticsAccount([FromBody]GoogleAnalyticsAccountForCreation googleanalyticsaccount)
        {
            //create a show in db.
            var googleanalyticsaccountToReturn = await _googleanalyticsaccountService.CreateEntityAsync<GoogleAnalyticsAccountDto, GoogleAnalyticsAccountForCreation>(googleanalyticsaccount);

            //return the show created response.
            return CreatedAtRoute("GetGoogleAnalyticsAccount", new { id = googleanalyticsaccountToReturn.Id }, googleanalyticsaccountToReturn);
        }

       
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("AuthGoogleAnalyticsAccount", Name = "AuthGoogleAnalyticsAccount")]

        public async Task<Dictionary<string, string>> AuthGoogleAnalyticsAccount([FromQuery]string id,Guid CompanyId)
        {
            //create a show in db.
            //return await _googleanalyticsaccountService.SetupGoogleAnalyticsAccountWithJson(id, CompanyId);
            //return await _googleanalyticsaccountService.SetupGoogleAnalyticsAccount(id, CompanyId);
            return await _googleanalyticsaccountService.SetupGoogleAnalyticsAccountNew(id,CompanyId);       
        }
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("AuthRefreshGoogleAccount", Name = "AuthRefreshGoogleAccount")]

        public GoogleTokenResponse AuthRefreshGoogleAccount([FromQuery] string code) {
            //create a show in db.
            // return await _googleanalyticsaccountService.SetupGoogleAnalyticsAccount(id,CompanyId);       
            //return await _googleanalyticsaccountService.RefreshGoogleAccount(refreshToken, accessToken);
            //return await _googleanalyticsaccountService.SetupGoogleAnalyticsAccountWithJson(refreshToken, accessToken);
            return _googleanalyticsaccountService.Callback(code);
        }
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("AuthGoogleRestSharp", Name = "AuthGoogleRestSharp")]

        public GoogleTokenResponse AuthGoogleRestSharp([FromQuery] string code) {
            //create a show in db.
            // return await _googleanalyticsaccountService.SetupGoogleAnalyticsAccount(id,CompanyId);       
            //return await _googleanalyticsaccountService.RefreshGoogleAccount(refreshToken, accessToken);
            //return await _googleanalyticsaccountService.SetupGoogleAnalyticsAccountWithJson(refreshToken, accessToken);
            return _googleanalyticsaccountService.GenerateToken();
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("AuthGoogleAdsAccount", Name = "AuthGoogleAdsAccount")]
        public string AuthGoogleAdsAccount()
        {
            //create a show in db.
            var redirectUrl =  _googleanalyticsaccountService.SetupGoogleAdsAccount();

            if (!string.IsNullOrEmpty(redirectUrl))
            {
                return redirectUrl;
            }
            return "";
                     
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("GoogleAdsCallback", Name = "GoogleAdsCallback")]
        public IActionResult GoogleAdsCallback([FromQuery]string code)
        {

            //create a show in db.
            var redirectUrl = _googleanalyticsaccountService.PrepareGoogleAdsToken(code);

            
            return Redirect(redirectUrl);

        }


        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteGoogleAnalyticsAccountById")]
        public async Task<IActionResult> DeleteGoogleAnalyticsAccountById(Guid id)
        {
            //if the googleanalyticsaccount exists
            if (await _googleanalyticsaccountService.ExistAsync(x => x.Id == id))
            {
                //delete the googleanalyticsaccount from the db.
                await _googleanalyticsaccountService.DeleteEntityAsync(id);
            }
            else
            {
                //if googleanalyticsaccount doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForGoogleAnalyticsAccount(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetGoogleAnalyticsAccount", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetGoogleAnalyticsAccount", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteGoogleAnalyticsAccountById", new { id = id }),
              "delete_googleanalyticsaccount",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateGoogleAnalyticsAccount", new { id = id }),
             "update_googleanalyticsaccount",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateGoogleAnalyticsAccount", new { }),
              "create_googleanalyticsaccount",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForGoogleAnalyticsAccounts(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateGoogleAnalyticsAccountsResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateGoogleAnalyticsAccountsResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateGoogleAnalyticsAccountsResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateGoogleAnalyticsAccountsResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredGoogleAnalyticsAccounts",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredGoogleAnalyticsAccounts",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredGoogleAnalyticsAccounts",
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
