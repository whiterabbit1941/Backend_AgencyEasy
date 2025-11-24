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
using EventManagement.Domain;
using RestSharp;
using Microsoft.Extensions.Configuration;
using System.Linq;
using IdentityServer4.AccessTokenValidation;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// Company endpoint
    /// </summary>
    [Route("api/companys")]
    [Produces("application/json")]
    [ApiController]

    public class CompanyController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly ICompanyService _companyService;
        private readonly ICompanyRepository _companyRepository;
        private ILogger<CompanyController> _logger;
        private readonly IUrlHelper _urlHelper;
        private readonly IUserInfoService _userInfoService;
        private IConfiguration _configuration;
        private readonly IAspUserService _aspuserService;

        #endregion


        #region CONSTRUCTOR

        public CompanyController(ICompanyService companyService, ILogger<CompanyController> logger, ICompanyRepository _companyRepository, IUrlHelper urlHelper, IUserInfoService userinfoService, IConfiguration configuration, IAspUserService aspuserService)
        {
            _logger = logger;
            _companyService = companyService;
            _companyRepository = _companyRepository;
            _urlHelper = urlHelper;
            _userInfoService = userinfoService;
            _configuration = configuration;
            _aspuserService = aspuserService;

        }

        #endregion


        #region HTTPGET

        [HttpGet(Name = "GetFilteredCompanys")]
        [Produces("application/vnd.tourmanagement.companys.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<ActionResult<List<CompanyDto>>> GetFilteredCompanys([FromQuery] FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_companyService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<CompanyDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var companysFromRepo = await _companyService.GetFilteredEntities(filterOptionsModel);

            //if HATEOAS links are required.
            if (mediaType == "application/vnd.tourmanagement.companys.hateoas+json")
            {
                //create HATEOAS links for each show.
                companysFromRepo.ForEach(company =>
                {
                    var entityLinks = CreateLinksForCompany(company.Id, filterOptionsModel.Fields);
                    company.links = entityLinks;
                });

                //prepare pagination metadata.
                var paginationMetadata = new
                {
                    totalCount = companysFromRepo.TotalCount,
                    pageSize = companysFromRepo.PageSize,
                    currentPage = companysFromRepo.CurrentPage,
                    totalPages = companysFromRepo.TotalPages,
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //create links for shows.
                var links = CreateLinksForCompanys(filterOptionsModel, companysFromRepo.HasNext, companysFromRepo.HasPrevious);

                //prepare model with data and HATEOAS links.
                var linkedCollectionResource = new
                {
                    value = companysFromRepo,
                    links = links
                };

                //return the data with Ok response.
                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = companysFromRepo.HasPrevious ?
                    CreateCompanysResourceUri(filterOptionsModel, ResourceUriType.PreviousPage) : null;

                var nextPageLink = companysFromRepo.HasNext ?
                    CreateCompanysResourceUri(filterOptionsModel, ResourceUriType.NextPage) : null;

                //prepare the pagination metadata.
                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = companysFromRepo.TotalCount,
                    pageSize = companysFromRepo.PageSize,
                    currentPage = companysFromRepo.CurrentPage,
                    totalPages = companysFromRepo.TotalPages
                };

                //add pagination meta data to response header.
                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                //return the data with Ok response.
                return Ok(companysFromRepo);
            }
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.companys.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetCompany")]
        [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Company>> GetCompany(Guid id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object companyEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetCompany called");

                //then get the whole entity and map it to the Dto.
                companyEntity = Mapper.Map<CompanyDto>(await _companyService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                companyEntity = await _companyService.GetPartialEntityAsync(id, fields);
            }

            //if company not found.
            if (companyEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.companys.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForCompany(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((CompanyDto)companyEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = companyEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = companyEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetCompanyDetails", Name = "GetCompanyDetails")]
        [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<ActionResult<List<CompanyDto>>> GetCompanyDetails([FromQuery] string userId)
        {
            var companyInfo = _companyService.GetCompany(userId);

            return Ok(companyInfo);
        }


        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetCompanyDetailsByDomain", Name = "GetCompanyDetailsByDomain")]
        [AllowAnonymous]
        public async Task<ActionResult<List<CompanyDto>>> GetCompanyDetailsByDomain([FromQuery] string domain)
        {            
            var companyInfo = _companyService.GetCompanyDetailsByDomain(domain);

            return Ok(companyInfo);
        }

        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[HttpGet("GetSuperAdminDashboard", Name = "GetSuperAdminDashboard")]
        //[AllowAnonymous]
        //public async Task<ActionResult<List<SuperAdminDashboard>>> GetSuperAdminDashboard(bool superadmin, string userId)
        //{        
        //    var companyList = await _companyService.GetAdminDashboard(userId, superadmin);
                       
        //    return Ok(companyList);
        //}

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateCompany")]
        [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] CompanyForUpdate CompanyForUpdate)
        {
            try
            {
                //if show not found
                if (!await _companyService.ExistAsync(x => x.Id == id))
                {
                    //then return not found response.
                    return NotFound();
                }

                //Update an entity.
                await _companyService.UpdateEntityAsync(id, CompanyForUpdate);
            }
            catch (Exception ex)
            {
                return Ok(false);
            }

            //return the response.
            return Ok(true);
        }

        #endregion


        #region HTTPPATCH

        [Consumes("application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PartiallyUpdateCompany(Guid id, [FromBody] JsonPatchDocument<CompanyForUpdate> jsonPatchDocument)
        {
            CompanyForUpdate dto = new CompanyForUpdate();
            Company company = new Company();

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
            Mapper.Map(dto, company);

            //set the Id for the show model.
            company.Id = id;

            //partially update the chnages to the db. 
            await _companyService.UpdatePartialEntityAsync(company, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("ActivateAgency", Name = "ActivateAgency")]
        [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ActivateAgency(Guid id, Guid code, String returnUrl,string requestedUrl)
        {

            var IsExist = _companyService.GetAllEntities().Where(x => x.RowGuid == code).Any();

            if (IsExist)

            {

                await _companyService.UpdateCompanyPartially(id,requestedUrl);  

            }
            else
            {

            }

            var redirectUrl = returnUrl;

            //return the response.

            return Redirect(redirectUrl);
        }

        #endregion


        #region HTTPPOST

        [HttpPost("UploadImageToAWS", Name = "UploadImageToAWS")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<ActionResult<AwsImageUrl>> UploadImageToAWS([FromBody] WhiteLabelImageDto whiteLabelImageDto)
        {
            var feviconUrl = string.Empty;

            var imageUrl = string.Empty;

            if (!string.IsNullOrEmpty(whiteLabelImageDto.ImageBase64))
            {
                 imageUrl = await _companyService.UploadImageToAws(whiteLabelImageDto.CompanyId, whiteLabelImageDto.ImageBase64, whiteLabelImageDto.FileName);
            }

            if (!string.IsNullOrEmpty(whiteLabelImageDto.Fevicon))
            {
                feviconUrl = await _companyService.UploadImageToAws(whiteLabelImageDto.CompanyId, whiteLabelImageDto.Fevicon, "fevicon_" + whiteLabelImageDto.FileName);
            }            

            var companyData = _companyService.GetAllEntities().Where(x => x.Id == whiteLabelImageDto.CompanyId).FirstOrDefault();
            var companyDto = new CompanyForUpdate();

            companyDto.Id = companyData.Id;
            companyDto.CompanyID = companyData.Id;
            companyDto.CompanyImageUrl = string.IsNullOrEmpty(imageUrl) ? companyData.CompanyImageUrl : imageUrl ;
            companyDto.Name = companyData.Name;
            companyDto.Website = companyData.Website;
            companyDto.Phone = companyData.Phone;
            companyDto.Timezone = companyData.Timezone;
            companyDto.CompanyType = companyData.CompanyType;
            companyDto.Description = companyData.Description;
            companyDto.Branding = companyData.Branding;
            companyDto.Address = companyData.Address;
            companyDto.Country = companyData.Country;
            companyDto.ZipCode = companyData.ZipCode;
            companyDto.City = companyData.City;
            companyDto.State = companyData.State;
            companyDto.IsApproved = companyData.IsApproved;
            companyDto.VatNo = companyData.VatNo;
            companyDto.Theme = companyData.Theme;
            companyDto.Fevicon = string.IsNullOrEmpty(feviconUrl) ? companyData.Fevicon  : feviconUrl;

            await _companyService.UpdateEntityAsync(companyData.Id, companyDto);

            var returnResponse = new AwsImageUrl { Fevicon = feviconUrl, CompanyImageUrl = imageUrl };
            return Ok(returnResponse);
        }

        /// <summary>
        /// This will query and update company settings.
        /// </summary>
        /// <returns>it will return company data</returns>
        [HttpPost("UpdateCompanySettings", Name = "UpdateCompanySettings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<ActionResult<CompanyDto>> UpdateCompanySettings(string companyId, [FromBody] CompanyDto companyDto)
        {
            var companyData = _companyService.GetEntityById(new Guid(companyId));
            if (companyData != null)
            {
                companyData.IsAllowMarketPlace = companyDto.IsAllowMarketPlace;
                _companyService.UpdateEntity(companyData);
                _companyService.SaveChanges();
            }
            return Ok(companyData);
        }

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateCompany")]
        [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<ActionResult<CompanyDto>> CreateCompany([FromBody] CompanyForCreation company)
        {
            //create a show in db.
            var companyToReturn = await _companyService.CreateEntityAsync<CompanyDto, CompanyForCreation>(company);

            //return the show created response.
            return CreatedAtRoute("GetCompany", new { id = companyToReturn.Id }, companyToReturn);
        }

        #endregion

        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteCompanyById")]
        [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteCompanyById(Guid id)
        {
            //if the company exists
            if (await _companyService.ExistAsync(x => x.Id == id))
            {
                //delete the company from the db.
                await _companyService.DeleteEntityAsync(id);
            }
            else
            {
                //if company doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForCompany(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCompany", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCompany", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteCompanyById", new { id = id }),
              "delete_company",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateCompany", new { id = id }),
             "update_company",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateCompany", new { }),
              "create_company",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCompanys(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateCompanysResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateCompanysResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateCompanysResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateCompanysResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredCompanys",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredCompanys",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredCompanys",
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
