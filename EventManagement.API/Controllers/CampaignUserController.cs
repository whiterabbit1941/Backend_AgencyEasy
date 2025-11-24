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
using Microsoft.Extensions.Configuration;
using RestSharp;
using EventManagement.Domain;
using IdentityServer4.AccessTokenValidation;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// CampaignUser endpoint
    /// </summary>
    [Route("api/campaignusers")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]

    public class CampaignUserController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ICampaignUserService _campaignuserService;
        private ILogger<CampaignUserController> _logger;
        private readonly IUrlHelper _urlHelper;
        private readonly ICampaignService _campaignService;
        private readonly IConfiguration _configuration;
        private readonly IUserInfoService _userInfoService;
        private readonly ICompanyUserService _companyUserService;
        private readonly IAspUserService _aspUserService;
        private readonly IEmailWhitelabelRepository _emailWhitelabelRepository;
        private readonly ICompanyService _companyService;

        #endregion


        #region CONSTRUCTOR

        public CampaignUserController(IAspUserService aspUserService, ICompanyUserService companyUserService, IUserInfoService userinfoService, IConfiguration configuration, ICampaignService campaignService, ICampaignUserService campaignuserService, ILogger<CampaignUserController> logger, IUrlHelper urlHelper,
            IEmailWhitelabelRepository emailWhitelabelRepository,ICompanyService companyService)
        {
            _logger = logger;
            _campaignuserService = campaignuserService;
            _urlHelper = urlHelper;
            _campaignService = campaignService;
            _configuration = configuration;
            _userInfoService = userinfoService;
            _companyUserService = companyUserService;
            _aspUserService = aspUserService;
            _emailWhitelabelRepository = emailWhitelabelRepository;
            _companyService = companyService;
        }

        #endregion


        #region HTTPGET
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetUserCampaignAccessByUserId", Name = "GetUserCampaignAccessByUserId")]
        public async Task<ActionResult<List<UserCampaignAccessDto>>> GetUserCampaignAccessByUserId(Guid CompanyId, Guid UserId)
        {
            var returnDto = new List<UserCampaignAccessDto>();
            try
            {
                var userCampList = _campaignuserService.GetAllEntities().Where(x => x.CompanyId == CompanyId).ToList();

                var campList = _campaignService.GetAllEntities()
                    .Where(x => x.CompanyID == CompanyId)
                    .Select(x => new CampaignDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        CampaignType = x.CampaignType,

                    })
                    .ToList();

                foreach (var camp in campList)
                {
                    var isExist = userCampList.Exists(x => x.CampaignId == camp.Id && x.UserId == UserId.ToString());

                    var entity = new UserCampaignAccessDto();
                    entity.UserId = UserId;
                    entity.CampaignId = camp.Id;
                    entity.CampaignName = camp.Name;
                    entity.CampaignType = camp.CampaignType;
                    entity.IsAccess = isExist;

                    returnDto.Add(entity);
                }

                return Ok(returnDto);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("UserExistsInIdentity", Name = "UserExistsInIdentity")]
        public async Task<ActionResult<bool>> UserExistsInIdentity(string email)
        {
            var user = _campaignuserService.GetUserbyEmailID(email);
            if (user != null)
            {
                return Ok(true);
            }
            else
            {
                return Ok(false);
            }
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("UserExistsInCompany", Name = "UserExistsInCompany")]
        public async Task<ActionResult<bool>> UserExistsInCompany(string email, string companyId)
        {
            try
            {
                //if fields are passed then get the partial show.
                var user = _campaignuserService.GetUserbyEmailID(email);
                if (user != null)
                {
                    var userExists = _companyUserService.GetAllEntities().Where(c => c.UserId == user.subjectId.ToString() && c.CompanyId == new Guid(companyId)).ToList();
                    if (userExists.Count > 0)
                    {
                        return Ok(true);
                    }
                    else
                    {
                        return Ok(false);
                    }
                }
                else
                {
                    return Ok(false);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("UserExistsInCampaign", Name = "UserExistsInCampaign")]
        public async Task<ActionResult<bool>> UserExistsInCampaign(string email, string campaignId)
        {
            try
            {
                //if fields are passed then get the partial show.
                var user = _campaignuserService.GetUserbyEmailID(email);
                if (user != null)
                {
                    var userExists = _campaignuserService.GetAllEntities().Where(c => c.UserId == user.subjectId.ToString() && c.CampaignId == new Guid(campaignId)).ToList();
                    if (userExists.Count > 0)
                    {
                        return Ok(true);
                    }
                    else
                    {
                        return Ok(false);
                    }
                }
                else
                {
                    return Ok(false);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("UserExistsInCampaignUser", Name = "UserExistsInCampaignUser")]
        public async Task<ActionResult<bool>> UserExistsInCompany(string email, Guid companyId)
        {
            try
            {
                //if fields are passed then get the partial show.
                var user = _campaignuserService.GetUserbyEmailID(email);
                if (user != null)
                {
                    var userExists = _campaignuserService.GetAllEntities().Where(c => c.UserId == user.subjectId.ToString() && c.CompanyId == companyId).ToList();
                    if (userExists.Count > 0)
                    {
                        return Ok(true);
                    }
                    else
                    {
                        return Ok(false);
                    }
                }
                else
                {
                    return Ok(false);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("UserExistsInCampaignOrCompany", Name = "UserExistsInCampaignOrCompany")]
        public async Task<ActionResult<bool>> UserExistsInCampaignOrCompany(string userid, string companyId)
        {
            try
            {
                ////if fields are passed then get the partial show.
                //var userExistsInCompany = _companyUserService.GetAllEntities().
                //                  Where(c => c.UserId == userid && c.CompanyId == new Guid(companyId)).ToList();

                var userExistsInCampaign = _campaignuserService.GetAllEntities()
                               .Where(c => c.UserId == userid && c.CompanyId == new Guid(companyId)).ToList();


                if (userExistsInCampaign.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// This function will return Campaign that are assinged to the user in an company.
        /// In case of Super admins it will return all the Campaign in an company. 
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetCampainForLoggedInUser", Name = "GetCampainForLoggedInUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public List<CampaignDto> GetCampainForLoggedInUser(string id, string companyId, string allCampaign)
        {
            List<CampaignDto> campaignList;

            if (allCampaign == "true")
            {
                campaignList = _campaignService.GetAllEntities(true)
                                .Where(c => c.CompanyID.ToString() == companyId)
                                .Select(x => new CampaignDto
                                {
                                    Id = x.Id,
                                    Name = x.Name,
                                    CampaignType = x.CampaignType,
                                    WebUrl = x.WebUrl,
                                    LeadGeneration = x.LeadGeneration,
                                    Sales = x.Sales,
                                    MoreTraffic = x.MoreTraffic,
                                    CompanyID = x.CompanyID,
                                    Ranking = x.Ranking,
                                    Gsc = x.Gsc,
                                    Conversions = x.Conversions,
                                    Traffic = x.Traffic,
                                    TrafficGa4 = x.TrafficGa4,
                                    CreatedBy= x.CreatedBy,
                                    CreatedOn= x.CreatedOn

                                })
                                .ToList();
            }
            else
            {
                var campaignIdList = _campaignuserService.GetAllEntities(true)
                                .Where(c => c.CompanyId.ToString() == companyId
                                       && c.UserId == id).Select(x => x.CampaignId).ToList();

                campaignList = _campaignService.GetAllEntities(true)
                                .Where(c => campaignIdList.Contains(c.Id))
                                .Select(x => new CampaignDto
                                {
                                    Id = x.Id,
                                    Name = x.Name,
                                    CampaignType = x.CampaignType,
                                    WebUrl = x.WebUrl,
                                    LeadGeneration = x.LeadGeneration,
                                    Sales = x.Sales,
                                    MoreTraffic = x.MoreTraffic,
                                    CompanyID = x.CompanyID,
                                    Ranking = x.Ranking,
                                    Gsc = x.Gsc,
                                    Conversions = x.Conversions,
                                    Traffic = x.Traffic,
                                    TrafficGa4 = x.TrafficGa4,
                                    CreatedBy = x.CreatedBy,
                                    CreatedOn = x.CreatedOn
                                })
                                .ToList();
            }

            return campaignList;
        }

        /// <summary>
        /// This will query and fetch all the Users that are created in server.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAllUsers", Name = "GetAllUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public List<CompanysUserDto> GetAllUsers(string userId, string companyId, string SuperAdmin, string campaignId = "")
        {
            List<CompanysUserDto> userData = new List<CompanysUserDto>();



            // if campaign name is null then return all users for only those campaign to which the Logged in user has access to.
            if (campaignId == null || String.IsNullOrEmpty(campaignId) || campaignId == "")
            {
                var userIdList = new List<AspUserDto>();
                var campaignIdList = new List<string>();



                if (SuperAdmin == "true")
                {
                    userIdList = _companyUserService.GetAllEntities()
                                    .Where(c => c.CompanyId.ToString() == companyId)
                                     .Select(x => new AspUserDto
                                     {
                                         Id = x.UserId
                                     })
                                    .ToList();
                }
                else
                {
                    //This will return those clients to which the logged in User has access.

                   

                    campaignIdList = _campaignuserService.GetAllEntities()
                                    .Where(c => c.UserId == userId
                                    && c.CompanyId.ToString() == companyId)
                                    .Select(x => x.CampaignId.ToString())
                                    .ToList();

                    if (campaignIdList.Count > 0)
                    {
                        userIdList = _campaignuserService.GetAllEntities(true)
                          .Where(c => campaignIdList.Contains(c.CampaignId.ToString()))
                          .Select(x => new AspUserDto
                          {
                              Id = x.UserId
                          })
                          .Distinct(new AspUserEqualityComparer())
                          .ToList();
                    }
                    else
                    {
                        userIdList = _companyUserService.GetAllEntities()
                                    .Where(c => c.CompanyId.ToString() == companyId)
                                     .Select(x => new AspUserDto
                                     {
                                         Id = x.UserId
                                     })
                                    .ToList();
                    }                   
                }


                for (int i = 0; i < userIdList.Count; i++)
                {
                    var user = _aspUserService.GetUserDetails(userIdList[i].Id);
                    if (user != null)
                    {
                        string role = _companyUserService.GetAllEntities().Where(x => x.CompanyId == new Guid(companyId) && x.UserId == user?.Id).FirstOrDefault()?.Role;



                        CompanysUserDto entity = new CompanysUserDto();
                        entity.Id = user.Id;
                        entity.Email = user.Email;
                        entity.EmailConfirmed = user.EmailConfirmed;
                        entity.FName = user.FName;
                        entity.LName = user.LName;
                        entity.CompanyRole = role;
                        entity.CompanyId = user.CompanyID;
                        entity.CreatedOn = user.CreatedOn;
                        entity.ShowDemoProject = user.ShowDemoProject;
                        userData.Add(entity);
                    }
                }
                return userData;
            }
            else
            {
                // Return Users for the sepecific client of an account.



                var userIdList = _campaignuserService.GetAllEntities(true)
                    .Where(c => c.CampaignId == new Guid(campaignId) && c.CompanyId.ToString() == companyId)
                    .Select(x => new AspUserDto
                    {
                        Id = x.UserId
                    })
                    .Distinct(new AspUserEqualityComparer())
                    .ToList();



                for (int i = 0; i < userIdList.Count; i++)
                {
                    var user = _aspUserService.GetUserDetails(userIdList[i].Id);
                    if (user != null)
                    {
                        string role = _companyUserService.GetAllEntities().Where(x => x.CompanyId == new Guid(companyId) && x.UserId == user?.Id).FirstOrDefault().Role;



                        CompanysUserDto entity = new CompanysUserDto();
                        entity.Id = user.Id;
                        entity.Email = user.Email;
                        entity.EmailConfirmed = user.EmailConfirmed;
                        entity.FName = user.FName;
                        entity.LName = user.LName;
                        entity.CompanyRole = role;
                        entity.CompanyId = user.CompanyID;
                        entity.ShowDemoProject = user.ShowDemoProject;
                        userData.Add(entity);
                    }
                }
                return userData;
            }
        }


  
        [HttpGet(Name = "GetFilteredCampaignUsers")]
        [Produces("application/vnd.tourmanagement.campaignusers.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CampaignUserDto>>> GetFilteredCampaignUsers([FromQuery] FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_campaignuserService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<CampaignUserDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var campaignusersFromRepo = await _campaignuserService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.campaignusers.hateoas+json")
            {
                //create HATEOAS links for each show.
                campaignusersFromRepo.ForEach(campaignuser =>
                {
                    var entityLinks = CreateLinksForCampaignUser(campaignuser.Id, filterOptionsModel.Fields);
                    campaignuser.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = campaignusersFromRepo.TotalCount,
                    pageSize = campaignusersFromRepo.PageSize,
                    currentPage = campaignusersFromRepo.CurrentPage,
                    totalPages = campaignusersFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForCampaignUsers(filterOptionsModel, campaignusersFromRepo.HasNext, campaignusersFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = campaignusersFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = campaignusersFromRepo.HasPrevious ?
                    CreateCampaignUsersResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = campaignusersFromRepo.HasNext ?
                    CreateCampaignUsersResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = campaignusersFromRepo.TotalCount,
                    pageSize = campaignusersFromRepo.PageSize,
                    currentPage = campaignusersFromRepo.CurrentPage,
                    totalPages = campaignusersFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(campaignusersFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.campaignusers.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetCampaignUser")]
        public async Task<ActionResult<CampaignUser>> GetCampaignUser(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object campaignuserEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetCampaignUser called");

                //then get the whole entity and map it to the Dto.
                campaignuserEntity = Mapper.Map<CampaignUserDto>(await _campaignuserService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                campaignuserEntity = await _campaignuserService.GetPartialEntityAsync(id, fields);
            }

            //if campaignuser not found.
            if (campaignuserEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.campaignusers.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForCampaignUser(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((CampaignUserDto)campaignuserEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = campaignuserEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = campaignuserEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        // Find disctinct Users based on EntityID
        public class AspUserEqualityComparer : IEqualityComparer<AspUserDto>
        {
            public bool Equals(AspUserDto x, AspUserDto y)
            {
                // Two items are equal if their keys are equal.
                return x.Id == y.Id;
            }

            public int GetHashCode(AspUserDto obj)
            {
                return obj.Id.GetHashCode();
            }
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateCampaignUser")]
        public async Task<IActionResult> UpdateCampaignUser(Guid id, [FromBody] CampaignUserForUpdate CampaignUserForUpdate)
        {

            //if show not found
            if (!await _campaignuserService.ExistAsync(x => x.Id == id))
            {
                //then return not found response.
                return NotFound();
            }

            //Update an entity.
            await _campaignuserService.UpdateEntityAsync(id, CampaignUserForUpdate);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateCampaignUser(Guid id, [FromBody] JsonPatchDocument<CampaignUserForUpdate> jsonPatchDocument)
        {
            CampaignUserForUpdate dto = new CampaignUserForUpdate();
            CampaignUser campaignuser = new CampaignUser();

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
            Mapper.Map(dto, campaignuser);

            //set the Id for the show model.
            campaignuser.Id = id;

            //partially update the chnages to the db. 
            await _campaignuserService.UpdatePartialEntityAsync(campaignuser, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("AddUserInSelectedCampaigns", Name = "AddUserInSelectedCampaigns")]
        public async Task<ActionResult<List<CampaignUserDto>>> AddUserInSelectedCampaigns(Guid CompanyId, Guid UserId, [FromBody] List<Guid> CampaignIds)
        {
            var returnData = new List<CampaignUserDto>();
            foreach (var campId in CampaignIds)
            {
                CampaignUser campaignEntity = new CampaignUser();
                campaignEntity.CreatedOn = DateTime.UtcNow;
                campaignEntity.CreatedBy = "system";
                campaignEntity.UpdatedOn = DateTime.UtcNow;
                campaignEntity.UpdatedBy = "system";
                campaignEntity.Id = new Guid();
                campaignEntity.CompanyId = CompanyId;
                campaignEntity.CampaignId = campId;
                campaignEntity.UserId = UserId.ToString();

                _campaignuserService.CreateEntity(campaignEntity);
                _campaignuserService.SaveChanges();

                var campDto = Mapper.Map<CampaignUserDto>(campaignEntity);
                returnData.Add(campDto);
            }
            //return the response.
            return Ok(returnData);
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("RemoveUserFromSelectedCampaigns", Name = "RemoveUserFromSelectedCampaigns")]
        public async Task<ActionResult<List<CampaignUserDto>>> RemoveUserFromSelectedCampaigns(Guid UserId, [FromBody] List<Guid> CampaignIds)
        {
            int returnData = -1;
            //if the serp exists
            if (await _campaignuserService.ExistAsync(x => CampaignIds.Contains(x.CampaignId) && x.UserId == UserId.ToString()))
            {
                //delete the serp from the db.  
                returnData = await _campaignuserService.DeleteBulkEntityAsync(x => CampaignIds.Contains(x.CampaignId) && x.UserId == UserId.ToString());
            }
            else
            {
                //if campaignuser doesn't exists then returns not found.
                return NotFound();
            }
            //return the response.
            return Ok(returnData);
        }

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("CreateAspUserWithCompanyAndCampaign", Name = "CreateAspUserWithCompanyAndCampaign")]
        //[Authorize(Policy = Policy.CreateUsers)]
        public async Task<ActionResult<AspUserForCreation>> CreateAspUserWithCompanyAndCampaign([FromBody] AspUserForCreation aspuser)
        {
            AspUserDto user = new AspUserDto();
            for (int i = 0; i < aspuser.CampaignID.Count(); i++)
            {
                var loggedInUser = _userInfoService;

                // Make Request
                var uri = _configuration.GetSection("IdentityServerUrl").Value;
                var client = new RestClient(uri + "/Account/");

                // make call for Update or Create User

                // make Request
                var requestPost = new RestRequest("InviteUserMail", Method.Post);

                //if user available or not.
                var userExists = _aspUserService.GetAllEntities().Where(x => x.Email == aspuser.Email).FirstOrDefault();

                var campaignName = _campaignService.GetAllEntities(true)
                                   .Where(c => c.Id == aspuser.CampaignID[i]).Select(x => x.Name).FirstOrDefault();

                var code = Guid.NewGuid();

                var res = _companyService.GetCustomDomainCompanyInfo(aspuser.CustomDomain);

                var fromWhom1 = string.Empty;
                var emailWhite_Label = _emailWhitelabelRepository.GetFilteredEntities().Where(x => x.CompanyID == aspuser.CompanyID && x.IsVerify == true).FirstOrDefault();
                if (emailWhite_Label != null)
                {
                    fromWhom1 = "no-reply@" + emailWhite_Label.DomainName;
                }
                else
                {
                    fromWhom1 = _configuration.GetSection("MailFrom").Value;
                }


                if (userExists == null)
                {
                    string password = _aspUserService.GeneratePassword(true, true, true, false, 10);

                    var hashPassword = _aspUserService.GetHash(password);

                    //create a show in db.
                    aspuser.Id = Guid.NewGuid().ToString();
                    aspuser.UserName = aspuser.Email.ToLower();
                    aspuser.NormalizedEmail = aspuser.Email.ToUpper();
                    aspuser.NormalizedUserName = aspuser.Email.ToUpper();
                    aspuser.RowGuid = code;
                    aspuser.PasswordHash = hashPassword;
                    aspuser.Email = aspuser.Email.ToLower();
                    aspuser.SecurityStamp = "QBNZZTFG4NWSXHGGZKUEICAREMXKLZ7A";
                    aspuser.ShowDemoProject = aspuser.ShowDemoProject;
                    var aspuserToReturn = await _aspUserService.CreateEntityAsync<AspUserDto, AspUserForCreation>(aspuser);
                    user = Mapper.Map<AspUserDto>(aspuserToReturn);

                    // add Parameter
                    requestPost.AddParameter("NewUser", true);
                    requestPost.AddParameter("Email", user.Email.ToLower());
                    requestPost.AddParameter("AdminName", loggedInUser.FirstName + " " + loggedInUser.LastName);
                    requestPost.AddParameter("Password", password);
                    requestPost.AddParameter("CampaignName", campaignName);
                    requestPost.AddParameter("FromWhom", fromWhom1);
                    //requestPost.AddParameter("ReturnURL", aspuser.CustomDomain);
                    requestPost.AddParameter("RequestedUrl", aspuser.CustomDomain);
                    requestPost.AddParameter("Code", code);
                    requestPost.AddParameter("CompanyName", res.Name);
                    requestPost.AddParameter("DashboardLogo", res.CompanyImageUrl);
                    requestPost.AddParameter("IsSendEmailToSuperAdmin", false);
                }
                else
                {
                    user = Mapper.Map<AspUserDto>(userExists);

                    var fromWhom = string.Empty;
                    var emailWhiteLabel = _emailWhitelabelRepository.GetFilteredEntities().Where(x => x.CompanyID == userExists.CompanyID && x.IsVerify == true).FirstOrDefault();
                    if (emailWhiteLabel != null)
                    {
                        fromWhom = "no-reply@" + emailWhiteLabel.DomainName;
                    }
                    else
                    {
                        fromWhom = _configuration.GetSection("MailFrom").Value;
                    }

                    // add Parameter
                    requestPost.AddParameter("NewUser", false);
                    requestPost.AddParameter("Email", user.Email.ToLower());
                    requestPost.AddParameter("AdminName", loggedInUser.FirstName + " " + loggedInUser.LastName);
                    requestPost.AddParameter("CampaignName", campaignName);
                    requestPost.AddParameter("FromWhom", fromWhom);
                    //requestPost.AddParameter("ReturnURL", aspuser.CustomDomain);
                    requestPost.AddParameter("RequestedUrl", aspuser.CustomDomain);
                    requestPost.AddParameter("Code", code);
                    requestPost.AddParameter("CompanyName", res.Name);
                    requestPost.AddParameter("DashboardLogo", res.CompanyImageUrl);
                    requestPost.AddParameter("IsSendEmailToSuperAdmin", false);
                }
                // call Marvin Endpoint
                var responsePost = await client.PostAsync(requestPost);

                var isUserExist = await _companyUserService.ExistAsync(x => x.CompanyId == aspuser.CompanyID && x.UserId == user.Id);
                if (!isUserExist)
                {
                    // create UserAccount Entity and stored in DB
                    CompanyUser companyEntity = new CompanyUser();
                    companyEntity.CreatedOn = DateTime.UtcNow;
                    companyEntity.CreatedBy = "system";
                    companyEntity.UpdatedOn = DateTime.UtcNow;
                    companyEntity.UpdatedBy = "system";
                    companyEntity.Id = new Guid();
                    companyEntity.CompanyId = aspuser.CompanyID;
                    companyEntity.UserId = user.Id;
                    companyEntity.Role = aspuser.CompanyRole;

                    _companyUserService.CreateEntity(companyEntity);
                    _companyUserService.SaveChanges();
                }

                // create ClientUser Entity and stored in DB
                if (!String.IsNullOrEmpty(aspuser.CampaignID.ToString()))
                {
                    CampaignUser campaignEntity = new CampaignUser();
                    campaignEntity.CreatedOn = DateTime.UtcNow;
                    campaignEntity.CreatedBy = "system";
                    campaignEntity.UpdatedOn = DateTime.UtcNow;
                    campaignEntity.UpdatedBy = "system";
                    campaignEntity.Id = new Guid();
                    campaignEntity.CompanyId = aspuser.CompanyID;
                    campaignEntity.CampaignId = aspuser.CampaignID[i];
                    campaignEntity.UserId = user.Id;

                    _campaignuserService.CreateEntity(campaignEntity);
                    _campaignuserService.SaveChanges();
                }
            }

            //return the show created response.
            return Ok(user);

        }

        /// <summary>
        /// Add multiple users in selected client.
        /// </summary>
        /// <returns>it will return users list</returns>
        [HttpPost("AddUsersInSelectedCampaign", Name = "AddUsersInSelectedCampaign")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AspUserDto>>> AddUsersInSelectedCampaign(string campaignId, string companyId, [FromBody] List<AspUserDto> usersList)
        {
            var loggedInUser = _userInfoService;
            // Make Request
            var uri = _configuration.GetSection("IdentityServerUrl").Value;
            var client = new RestClient(uri + "/Account/");

            var fromWhom = string.Empty;
            var emailWhiteLabel = _emailWhitelabelRepository.GetFilteredEntities().Where(x => x.CompanyID == new Guid(companyId) && x.IsVerify == true).FirstOrDefault();
            if (emailWhiteLabel != null)
            {
                fromWhom = "no-reply@" + emailWhiteLabel.DomainName;
            }
            else
            {
                fromWhom = _configuration.GetSection("MailFrom").Value;
            }

            // make Request
            var requestPost = new RestRequest("InviteUserMail", Method.Post);
            for (int i = 0; i < usersList.Count; i++)
            {
                // add Parameter
                requestPost.AddOrUpdateParameter("NewUser", !usersList[i].EmailConfirmed);
                requestPost.AddOrUpdateParameter("Email", usersList[i].Email);
                requestPost.AddOrUpdateParameter("AdminName", loggedInUser.FirstName + " " + loggedInUser.LastName);
                requestPost.AddParameter("FromWhom", fromWhom);

                // call Marvin Endpoint
                var responsePost = await client.PostAsync(requestPost);

                CampaignUser userEntity = new CampaignUser();
                userEntity.CreatedBy = "system";
                userEntity.CreatedOn = DateTime.UtcNow;
                userEntity.UpdatedBy = "system";
                userEntity.UpdatedOn = DateTime.UtcNow;
                userEntity.CompanyId = new Guid(companyId);
                userEntity.UserId = usersList[i].Id;
                userEntity.CampaignId = new Guid(campaignId);

                _campaignuserService.CreateEntity(userEntity);
                _campaignuserService.SaveChanges();
            }
            return Ok(usersList);
        }

        [HttpPost("updateUserRole", Name = "updateUserRole")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> updateUserRole(string userId, string companyId, string role)
        {
            var companyUser = _companyUserService.GetAllEntities(true).Where(u => u.CompanyId == new Guid(companyId) && u.UserId == userId).FirstOrDefault();

            // update user role
            if (companyUser != null)
            {
                companyUser.Role = role;
                companyUser.UpdatedOn = DateTime.UtcNow;
                _companyUserService.UpdateEntity(companyUser);
                _companyUserService.SaveChanges();

                return Ok(true);
            }
            else
            {
                return Ok(false);
            }
        }

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateCampaignUser")]
        public async Task<ActionResult<CampaignUserDto>> CreateCampaignUser([FromBody] CampaignUserForCreation campaignuser)
        {
            //create a show in db.
            var campaignuserToReturn = await _campaignuserService.CreateEntityAsync<CampaignUserDto, CampaignUserForCreation>(campaignuser);

            //return the show created response.
            return CreatedAtRoute("GetCampaignUser", new { id = campaignuserToReturn.Id }, campaignuserToReturn);
        }

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteCampaignUserById")]
        public async Task<IActionResult> DeleteCampaignUserById(string id, string campaignId)
        {
            //if the campaignuser exists
            var campaignUser = _campaignuserService.GetAllEntities(true).Where(u => u.CampaignId == new Guid(campaignId) && u.UserId == id).FirstOrDefault();
            if (await _campaignuserService.ExistAsync(x => x.Id == campaignUser.Id))
            {
                //delete the campaignuser from the db.
                await _campaignuserService.DeleteEntityAsync(campaignUser.Id);
            }
            else
            {
                //if campaignuser doesn't exists then returns not found.
                return NotFound();
            }
            //return the response.
            return Ok(true);
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForCampaignUser(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignUser", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCampaignUser", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteCampaignUserById", new { id = id }),
              "delete_campaignuser",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateCampaignUser", new { id = id }),
             "update_campaignuser",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateCampaignUser", new { }),
              "create_campaignuser",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCampaignUsers(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateCampaignUsersResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateCampaignUsersResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateCampaignUsersResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateCampaignUsersResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredCampaignUsers",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredCampaignUsers",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredCampaignUsers",
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
