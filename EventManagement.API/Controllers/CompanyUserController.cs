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
using Microsoft.Extensions.Configuration;
using EventManagement.Domain;
using System.Linq;
using RestSharp;
using IdentityServer4.AccessTokenValidation;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// CompanyUser endpoint
    /// </summary>
    [Route("api/companyusers")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]

    public class CompanyUserController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ICompanyUserService _companyuserService;
        private ILogger<CompanyUserController> _logger;
        private readonly IUrlHelper _urlHelper;
        private readonly IConfiguration _configuration;
        private readonly IUserInfoService _userInfoService;
        private readonly ICompanyService _companyService;
        private readonly ICampaignUserService _campaignuserService;
        private readonly IAspUserService _aspUserService;

        #endregion


        #region CONSTRUCTOR

        public CompanyUserController(ICompanyService companyService,
            IUserInfoService userInfoService, IConfiguration configuration, 
            ICompanyUserService companyuserService, ICampaignUserService campaignuserService,
            ILogger<CompanyUserController> logger, IUrlHelper urlHelper,
            IAspUserService aspUserService)
        {
            _logger = logger;
            _companyuserService = companyuserService;
            _urlHelper = urlHelper;
            _configuration = configuration;
            _userInfoService = userInfoService;
            _companyService = companyService;
            _campaignuserService = campaignuserService;
            _aspUserService = aspUserService;
        }

        #endregion


        #region HTTPGET

        /// <summary>
        /// This will query and fetch all the Users that are created in server.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAllUsersFromSelectedCompany", Name = "GetAllUsersFromSelectedCompany")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public List<AspUserDto> GetAllUsersFromSelectedCompany()
        {
            var loggedInUser = _userInfoService;
            var uID = _companyuserService.GetAllEntities(true)
                                        .Where(c => c.CompanyId.ToString() == loggedInUser.SelectedCompanyId)
                                        .Select(x => new AspUserDto
                                        {
                                            Id = x.UserId,
                                        })
                                        .ToList();

            var uri = _configuration.GetSection("IdentityServerUrl").Value;
            var client = new RestClient(uri + "/Account/");
            var request = new RestRequest("Users", Method.Get);
            request.AddHeader("Content-Type", "application/json");
            uID.ForEach(u => request.AddObject(u));
            var response = client.ExecuteAsync(request).Result;

            var userData = JsonConvert.DeserializeObject<List<AspUserDto>>(response.Content);

            return userData;
        }

        [HttpGet("GetLoggedInUserCompany", Name = "GetLoggedInUserCompany")]
        [Produces("application/vnd.tourmanagement.edge_accounts.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<List<SuperAdminDashboard>> GetLoggedInUserCompany(bool superadmin, string userId)
        {
           var  listOfCompanyPlan = _companyuserService.GetAdminDashboard(superadmin, userId);

           return listOfCompanyPlan;
        }

        [HttpGet(Name = "GetFilteredCompanyUsers")]
        [Produces("application/vnd.tourmanagement.companyusers.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CompanyUserDto>>> GetFilteredCompanyUsers([FromQuery] FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_companyuserService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<CompanyUserDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var companyusersFromRepo = await _companyuserService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.companyusers.hateoas+json")
            {
                //create HATEOAS links for each show.
                companyusersFromRepo.ForEach(companyuser =>
                {
                    var entityLinks = CreateLinksForCompanyUser(companyuser.Id, filterOptionsModel.Fields);
                    companyuser.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = companyusersFromRepo.TotalCount,
                    pageSize = companyusersFromRepo.PageSize,
                    currentPage = companyusersFromRepo.CurrentPage,
                    totalPages = companyusersFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForCompanyUsers(filterOptionsModel, companyusersFromRepo.HasNext, companyusersFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = companyusersFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = companyusersFromRepo.HasPrevious ?
                    CreateCompanyUsersResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = companyusersFromRepo.HasNext ?
                    CreateCompanyUsersResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = companyusersFromRepo.TotalCount,
                    pageSize = companyusersFromRepo.PageSize,
                    currentPage = companyusersFromRepo.CurrentPage,
                    totalPages = companyusersFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(companyusersFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.companyusers.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetCompanyUser")]
        public async Task<ActionResult<CompanyUser>> GetCompanyUser(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object companyuserEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetCompanyUser called");

                //then get the whole entity and map it to the Dto.
                companyuserEntity = Mapper.Map<CompanyUserDto>(await _companyuserService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                companyuserEntity = await _companyuserService.GetPartialEntityAsync(id, fields);
            }

            //if companyuser not found.
            if (companyuserEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.companyusers.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForCompanyUser(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((CompanyUserDto)companyuserEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = companyuserEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = companyuserEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateCompanyUser")]
        public async Task<IActionResult> UpdateCompanyUser(Guid id, [FromBody] CompanyUserForUpdate CompanyUserForUpdate)
        {

            //if show not found
            if (!await _companyuserService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _companyuserService.UpdateEntityAsync(id, CompanyUserForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateCompanyUser(Guid id, [FromBody] JsonPatchDocument<CompanyUserForUpdate> jsonPatchDocument)
        {
            CompanyUserForUpdate dto = new CompanyUserForUpdate();
            CompanyUser companyuser = new CompanyUser();

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
            Mapper.Map(dto, companyuser);

            //set the Id for the show model.
            companyuser.Id = id;

            //partially update the chnages to the db. 
            await _companyuserService.UpdatePartialEntityAsync(companyuser, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateCompanyUser")]
        public async Task<ActionResult<CompanyUserDto>> CreateCompanyUser([FromBody] CompanyUserForCreation companyuser)
        {
            //create a show in db.
            var companyuserToReturn = await _companyuserService.CreateEntityAsync<CompanyUserDto, CompanyUserForCreation>(companyuser);

            //return the show created response.
            return CreatedAtRoute("GetCompanyUser", new { id = companyuserToReturn.Id }, companyuserToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteCompanyUserById")]
        public async Task<IActionResult> DeleteCompanyUserById(string id, string companyId)
        {

            var companyUser = _companyuserService.GetAllEntities(true).Where(u => u.CompanyId == new Guid(companyId) && u.UserId == id).FirstOrDefault();
            var campaignUser = _campaignuserService.GetAllEntities(true).Where(m => m.CompanyId == new Guid(companyId) && m.UserId == id).ToList();
            var aspnetUser = _aspUserService.GetAllEntities(true).Where(x => x.Id == id && x.CompanyID == new Guid(companyId)).FirstOrDefault();
            //if the companyuser exists
            foreach (var c in campaignUser)
            {
                if (await _campaignuserService.ExistAsync(n => n.Id == c.Id))
                {
                    await _campaignuserService.DeleteEntityAsync(c.Id);
                }
            }
            if (await _companyuserService.ExistAsync(x => x.Id == companyUser.Id))
            {
                //delete the companyuser from the db.
                await _companyuserService.DeleteEntityAsync(companyUser.Id);
            }
            else
            {
                //if companyuser doesn't exists then returns not found.
                return NotFound();
            }
            if (aspnetUser != null)
            {
                await _aspUserService.DeleteEntityAsync(id);
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForCompanyUser(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCompanyUser", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCompanyUser", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteCompanyUserById", new { id = id }),
              "delete_companyuser",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateCompanyUser", new { id = id }),
             "update_companyuser",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateCompanyUser", new { }),
              "create_companyuser",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCompanyUsers(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateCompanyUsersResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateCompanyUsersResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateCompanyUsersResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateCompanyUsersResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredCompanyUsers",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredCompanyUsers",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredCompanyUsers",
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
