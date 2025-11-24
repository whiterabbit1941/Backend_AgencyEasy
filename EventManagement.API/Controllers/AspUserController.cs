using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//using FinanaceManagement.API.Helpers;
using Microsoft.Extensions.Logging;
using FinanaceManagement.API.Models;
using IdentityServer4.AccessTokenValidation;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using EventManagement.Dto;
using EventManagement.Utility;
using RestSharp;
using EventManagement.Domain;
using EventManagement.Service;
using System.Security.Cryptography;
using EventManagement.Domain.Entities;
using System.Security;

namespace EventManagement.API.Controllers
{
    /// <summary>
    /// AspUser endpoint
    /// </summary>
    [Route("api/aspusers")]
    [Produces("application/json")]
    [ApiController]
    [Authorize(AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme)]

    public class AspUserController : Controller
    {

        #region PRIVATE MEMBERS

        private readonly IAspUserService _aspuserService;
        private readonly ICompanyService _companyService;
        private readonly ICompanyPlanService _companyPlanService;
        private ILogger<AspUserController> _logger;
        private readonly IUrlHelper _urlHelper;
        private readonly IUserInfoService _userInfoService;
        private readonly IStripeCouponService _stripeCouponService;
        //Note:   These are the related tables where you want to add the user reference.

        //private readonly Iapp_UserService _app_UserService;
        //private readonly IEdge_UserAccountService _edge_UserAccountService;
        //private readonly IEdge_ClientUserService _edge_ClientUserService;
        //private readonly IEdge_ClientAccountService _edge_ClientAccountService;
        private IConfiguration _configuration;

        // Note: The Blob Service where you want to store the user image. 
        // private readonly IBlobService _blobService;

        private IEmailWhitelabelRepository _emailWhitelabelRepository;
        private IDomainWhitelabelRepository _domainWhitelabelRepository;

        #endregion


        #region CONSTRUCTOR

        public AspUserController(
            //IBlobService blobService, 
            IConfiguration configuration,
            //IEdge_ClientAccountService edge_ClientAccountService, 
            //IEdge_ClientUserService edge_ClientUserService, 
            //IEdge_UserAccountService edge_UserAccountService, 
            //Iapp_UserService app_UserService, 
            IAspUserService aspuserService, ILogger<AspUserController> logger, IUrlHelper urlHelper, IUserInfoService userinfoService, ICompanyService companyService,
            ICompanyPlanService companyPlanService, IEmailWhitelabelRepository emailWhitelabelRepository, 
            IDomainWhitelabelRepository domainWhitelabelRepository, IStripeCouponService stripeCouponService)
        {
            _logger = logger;
            _aspuserService = aspuserService;
            _urlHelper = urlHelper;
            _configuration = configuration;
            _userInfoService = userinfoService;
            _companyService = companyService;
            _companyPlanService = companyPlanService;
            _emailWhitelabelRepository = emailWhitelabelRepository;
            _domainWhitelabelRepository = domainWhitelabelRepository;
            _stripeCouponService = stripeCouponService;

            //_app_UserService = app_UserService;
            //_edge_UserAccountService = edge_UserAccountService;
            //_edge_ClientUserService = edge_ClientUserService;
            //_edge_ClientAccountService = edge_ClientAccountService;
            //_blobService = blobService;
        }

        #endregion


        #region HTTPGET

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("ConfirmEmail", Name = "ConfirmEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(Guid id, Guid code, string returnUrl)
        {
            var redirectUrl = returnUrl;
            var IsExist = _aspuserService.GetAllEntities().Where(x => x.RowGuid == code).Any();

            if (IsExist)
            {
                await _aspuserService.UpdateUserPartially(id.ToString());
            }

            //return the response.

            return Redirect(redirectUrl);
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("GetUserAccessToken", Name = "GetUserAccessToken")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserAccessToken(string username, string password)
        {
            var uri = _configuration.GetSection("IdentityServerUrl").Value;

            // create client
            var client = new RestClient(uri + "/Account/");

            // create request
            var request = new RestRequest("GetROPCAccessToken", Method.Get);

            // add header
            request.AddHeader("Content-Type", "application/json");

            // add params
            request.AddParameter("username", username);
            request.AddParameter("password", password);

            // execute request
            var response = await client.ExecuteAsync(request);

            var rootobj = JsonConvert.DeserializeObject<dynamic>(response.Content);

            return Ok(rootobj);
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("CheckUserPassword", Name = "CheckUserPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckUserPassword(string username, string password)
        {
            var uri = _configuration.GetSection("IdentityServerUrl").Value;

            // create client
            var client = new RestClient(uri + "/Account/");

            // create request
            var request = new RestRequest("CheckCurrentPassword", Method.Get);

            // add header
            request.AddHeader("Content-Type", "application/json");

            // add params
            request.AddParameter("username", username);
            request.AddParameter("password", password);

            // execute request
            var response = await client.ExecuteAsync(request);

            var rootobj = JsonConvert.DeserializeObject<dynamic>(response.Content);

            return Ok(rootobj);
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("ResetUserPassword", Name = "ResetUserPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetUserPassword(string email, string password, string confirmPassword, string token)
        {
            var uri = _configuration.GetSection("IdentityServerUrl").Value;

            // create client
            var client = new RestClient(uri + "/Account/");

            // create request
            var request = new RestRequest("ResetUserPassword", Method.Post);

            // add header
            //request.AddHeader("Content-Type", "application/json");

            // add params
            request.AddParameter("Email", email);
            request.AddParameter("Password", password);
            request.AddParameter("ConfirmPassword", confirmPassword);
            request.AddParameter("Token", token);

            // execute request
            var response = await client.ExecuteAsync(request);

            var rootobj = JsonConvert.DeserializeObject<dynamic>(response.Content);

            return Ok(rootobj);
        }

        /// <summary>
        /// Call from frontend for send forgot password email
        /// </summary>
        /// <param name="email">email</param>
        /// <returns>true or false</returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("ForgotPassword", Name = "ForgotPassword")]
        [AllowAnonymous]
        public IActionResult ForgotPassword(string email, string baseUrl)
        {
            var uri = _configuration.GetSection("IdentityServerUrl").Value;

            // create client
            var client = new RestClient(uri + "/Account/");

            // create request
            var request = new RestRequest("SendEmailForForgotPassword", Method.Post);

            // add header
            //request.AddHeader("Content-Type", "application/json");

            var company = _companyService.GetCustomDomainCompanyInfo(baseUrl);

            // add params
            request.AddParameter("Email", email);
            request.AddParameter("BaseUrl", baseUrl);
            request.AddParameter("CompanyLogo", company.CompanyImageUrl);

            // execute request
            var response = client.ExecuteAsync(request).Result;

            var rootobj = JsonConvert.DeserializeObject<dynamic>(response.Content);

            return Ok(rootobj);
        }


        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("GetUserById/{id}", Name = "GetUserById")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = _aspuserService.GetEntityById(id);

            var userData = Mapper.Map<AspUserDto>(user);

            return Ok(userData);
        }

        [HttpGet(Name = "GetFilteredAspUsers")]
        [Produces("application/vnd.tourmanagement.aspusers.hateoas+json", "application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]        
        public async Task<ActionResult<List<AspUserDto>>> GetFilteredAspUsers([FromQuery] FilterOptionsModel filterOptionsModel, [FromHeader(Name = "Accept")] string mediaType)
        {

            //if order by fields are not valid.
            if (!_aspuserService.ValidMappingExists(filterOptionsModel.OrderBy))
            {
                //then return bad request.
                return BadRequest();
            }

            //if fields are not valid.
            if (!EventManagementUtils.TypeHasProperties<AspUserDto>(filterOptionsModel.Fields))
            {
                //then return bad request.
                return BadRequest();
            }

            //get the paged/filtered show from db. 
            var aspusersFromRepo = await _aspuserService.GetFilteredEntities(filterOptionsModel);


            //return the data with Ok response.
            return Ok(aspusersFromRepo);
        }




        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/vnd.tourmanagement.aspusers.hateoas+json", "application/json")]
        [HttpGet("{id}", Name = "GetAspUser")]
        [AllowAnonymous]
        public async Task<ActionResult<AspNetUsers>> GetAspUser(string id, [FromQuery] string fields, [FromHeader(Name = "Accept")] string mediaType)
        {
            object aspuserEntity = null;
            object linkedResourceToReturn = null;

            //if fields are not passed.
            if (string.IsNullOrEmpty(fields))
            {
                _logger.LogInformation("GetAspUser called");

                //then get the whole entity and map it to the Dto.
                aspuserEntity = Mapper.Map<AspUserDto>(await _aspuserService.GetEntityByIdAsync(id));
            }
            else
            {
                //if fields are passed then get the partial show.
                aspuserEntity = await _aspuserService.GetPartialEntityAsync(id, fields);
            }

            //if aspuser not found.
            if (aspuserEntity == null)
            {
                //then return not found response.
                return NotFound();
            }

            if (mediaType == "application/vnd.tourmanagement.aspusers.hateoas+json")
            {
                //create HATEOS links
                var links = CreateLinksForAspUser(id, fields);

                //if fields are not passed.
                if (string.IsNullOrEmpty(fields))
                {
                    //convert the typed object to expando object.
                    linkedResourceToReturn = ((AspUserDto)aspuserEntity).ShapeData("") as IDictionary<string, object>;

                    //add the HATEOAS links to the model.
                    ((IDictionary<string, object>)linkedResourceToReturn).Add("links", links);
                }
                else
                {
                    linkedResourceToReturn = aspuserEntity;

                    //add the HATEOAS links to the model.
                    ((dynamic)linkedResourceToReturn).links = links;

                }
            }
            else
            {
                linkedResourceToReturn = aspuserEntity;
            }

            //return the Ok response.
            return Ok(linkedResourceToReturn);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("GetUserDetails", Name = "GetUserDetails")]
        public async Task<ActionResult<AspUserDto>> GetUserDetails([FromQuery] string userId)
        {
            var userInfo = _aspuserService.GetUserDetails(userId);

            return Ok(userInfo);
        }

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("exists/{id}", Name = "Exists")]
        public async Task<ActionResult<bool>> Exists(string id)
        {
            //if fields are passed then get the partial show.
            bool userExists = await _aspuserService.ExistAsync(x => x.Id == id);
            //return the Ok response.
            return Ok(userExists);
        }

        #endregion


        #region HTTPPUT

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPut("{id}", Name = "UpdateAspUser")]
        public async Task<IActionResult> UpdateAspUser(string id, [FromBody] AspUserForUpdate AspUserForUpdate)
        {
            try
            {
                //if show not found
                if (!await _aspuserService.ExistAsync(x => x.Id == id.ToString()))
                {
                    //then return not found response.
                    return NotFound();
                }

                //Update an entity.
                await _aspuserService.UpdateEntityAsync(id, AspUserForUpdate);
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
        public async Task<IActionResult> PartiallyUpdateAspUser(string id, [FromBody] JsonPatchDocument<AspUserForUpdate> jsonPatchDocument)
        {
            AspUserForUpdate dto = new AspUserForUpdate();
            AspNetUsers aspuser = new AspNetUsers();

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
            Mapper.Map(dto, aspuser);

            //set the Id for the show model.
            aspuser.Id = id.ToString();

            //partially update the chnages to the db. 
            await _aspuserService.UpdatePartialEntityAsync(aspuser, jsonPatchDocument);

            //return the response.
            return NoContent();
        }

        #endregion


        #region HTTPPOST
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("GetUserAccessTokenForAppsumo", Name = "GetUserAccessTokenForAppsumo")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserAccessTokenForAppsumo()
        {
            string username = Request.Form["username"];
            string password = Request.Form["password"];
            AppsumoTokenResponseDto returnData = new AppsumoTokenResponseDto();
            var uri = _configuration.GetSection("IdentityServerUrl").Value;

            // create client
            var client = new RestClient(uri + "/Account/");

            // create request
            var request = new RestRequest("GetROPCAccessToken", Method.Get);

            // add header
            request.AddHeader("Content-Type", "application/json");

            // add params
            request.AddParameter("username", username);
            request.AddParameter("password", password);

            // execute request
            var response = await client.ExecuteAsync(request);

            var rootobj = JsonConvert.DeserializeObject<dynamic>(response.Content);
            returnData.access = rootobj.tokenData.accessToken;

            return Ok(returnData);
        }

        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("NotificationRequestFromAppsumo", Name = "NotificationRequestFromAppsumo")]
        public async Task<IActionResult> NotificationRequestFromAppsumo()
        {


            var isDowngrade = false;
            string action = Request.Form["action"];
            string plan_id = Request.Form["plan_id"];
            string uuid = Request.Form["uuid"];
            string activation_email = Request.Form["activation_email"];
            string invoice_item_uuid = Request.Form["invoice_item_uuid"];

            //string formAsString = string.Empty;

            //foreach (string key in Request.Form.Keys)
            //{
            //    string value = Request.Form[key];
            //    formAsString += $"{key}: {value}{Environment.NewLine}";
            //}

            // SendGrid client should be initialized with configuration
            // var client = new SendGridClient(_configuration["Client"]);

            // Example email sending code (commented out)
            // var msg = MailHelper.CreateSingleEmailToMultipleRecipients(...);
            //   "Webhook recieved : action performed" + action, "", formAsString);

            //var response = await client.SendEmailAsync(msg);

            var frontendUrl = _configuration.GetSection("FrontendUrl").Value;

            AppsumoNotificationResponseDto returnData = new AppsumoNotificationResponseDto();
            if (action == "activate")
            {
                returnData.message = "product activated";
                returnData.redirect_url = String.Format("{0}/appSumoRegister?plan_id={1}&invoice_id={2}&email={3}", frontendUrl, plan_id, invoice_item_uuid, activation_email);
                return Created("", returnData);
            }
            else if (action == "enhance_tier")
            {
                var companyData = _companyService.GetCompanyByUserEmail(activation_email);

                // expired current plan immidiatly
                var currentPlan = _companyPlanService.GetAllEntities().AsQueryable().Where(x => x.CompanyId == companyData.Id && x.Active).FirstOrDefault();
                currentPlan.Active = false;
                currentPlan.ExpiredOn = DateTime.Now;
                _companyPlanService.UpdateEntity(currentPlan);
                _companyPlanService.SaveChanges();

                // assign new company plan
                var res = await _companyPlanService.createCompanyPlan(companyData.Id.ToString(), false, invoice_item_uuid, plan_id, false);

                if (res)
                {
                    returnData.message = "product enhanced";
                    returnData.redirect_url = String.Format("{0}/company/{1}/appSumoSubscription", frontendUrl, companyData.Id);
                }
                return Ok(returnData);
            }
            else if (action == "reduce_tier")
            {
                var companyData = _companyService.GetCompanyByUserEmail(activation_email);

                // expired current plan immidiatly
                var currentPlan = _companyPlanService.GetAllEntities().AsQueryable().Where(x => x.CompanyId == companyData.Id && x.Active).FirstOrDefault();
                currentPlan.Active = false;
                currentPlan.ExpiredOn = DateTime.Now;
                currentPlan.IsDowngradeAppsumo = false;               
                _companyPlanService.UpdateEntity(currentPlan);
                _companyPlanService.SaveChanges();


                var defaultPlan = _companyPlanService.GetDefaultPlanForAppsumo(plan_id);

                var currentCreatedProjectCount = _aspuserService.GetProjectCreatedCount(companyData.Id);
                var currentCreatedKeywordsCount = _aspuserService.GetSerpCreatedCount(companyData.Id);

                if (currentCreatedKeywordsCount > (defaultPlan.MaxKeywordsPerProject * currentCreatedProjectCount) 
                    || (currentCreatedProjectCount > defaultPlan.MaxProjects))
                {
                    isDowngrade = true;
                }
                else
                {
                    isDowngrade = false;
                }

                // assign new company plan
                var res = await _companyPlanService.createCompanyPlan(companyData.Id.ToString(), false, currentPlan.PaymentProfileId , plan_id, isDowngrade);

                if (res)
                {
                    returnData.message = "product reduced";
                    returnData.redirect_url = String.Format("{0}/company/{1}/appSumoSubscription", frontendUrl, companyData.Id);
                }
                return Ok(returnData);
            }
            else if (action == "refund")
            {
                var companyData = _companyService.GetCompanyByUserEmail(activation_email);

                // expired current plan immidiatly
                var currentPlan = _companyPlanService.GetAllEntities().AsQueryable().Where(x => x.CompanyId == companyData.Id && x.Active).FirstOrDefault();
                currentPlan.ExpiredOn = DateTime.Now;
                currentPlan.PaymentProfileId = currentPlan.PaymentProfileId;
                _companyPlanService.UpdateEntity(currentPlan);
                var res = _companyPlanService.SaveChanges();

                if (res)
                {
                    returnData.message = "product refunded";
                    returnData.redirect_url = String.Format("{0}/company/{1}/home", frontendUrl, companyData.Id);
                }
                return Ok(returnData);
            }
            else if (action == "update")
            {
                var companyData = _companyService.GetCompanyByUserEmail(activation_email);

                // expired current plan immidiatly
                var currentPlan = _companyPlanService.GetAllEntities().AsQueryable().Where(x => x.CompanyId == companyData.Id && x.Active).FirstOrDefault();
                currentPlan.PaymentProfileId = invoice_item_uuid;
                _companyPlanService.UpdateEntity(currentPlan);
                var res = _companyPlanService.SaveChanges();

                if (res)
                {
                    returnData.message = "product reduced and update";
                    returnData.redirect_url = String.Format("{0}/company/{1}/appSumoSubscription", frontendUrl, companyData.Id);
                }
                return Ok(returnData);
            }
            else
            {
                return NoContent();
            }
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("AppsumoSignup", Name = "AppsumoSignup")]
        [AllowAnonymous]
        //[Authorize(Policy = Policy.CreateUsers)]
        public async Task<ActionResult<AspUserDto>> AppsumoSignup()
        {
            AspUserDto user = new AspUserDto();
            string fileExtension = string.Empty;

            try
            {
                AspUserForCreation aspuser = new AspUserForCreation();

                aspuser.CName = Request.Form["companyName"];
                aspuser.Companytype = Request.Form["companytype"];
                aspuser.LName = Request.Form["lName"];
                aspuser.CustomDomain = Request.Form["customDomain"];
                aspuser.Email = Request.Form["email"];
                aspuser.FName = Request.Form["fName"];
                aspuser.Password = Request.Form["password"];
                aspuser.PasswordHash = _aspuserService.GetHash(Request.Form["password"]);
                aspuser.NewPlanName = Request.Form["newPlanName"];
                aspuser.NewPlanPaymentId = Request.Form["newPlanPaymentId"];

                // get custom domain info
                var response = _companyService.GetCustomDomainCompanyInfo(aspuser.CustomDomain);

                // create company
                var companyToReturn = await _companyService.CreateCompany(aspuser.CName, aspuser.Companytype, aspuser.CompanyImageUrl, aspuser.SubDomain, aspuser.CompanyImageUrl, aspuser.CompanyImageUrl, fileExtension);

                //create a show in db.
                aspuser.Id = Guid.NewGuid().ToString();
                aspuser.UserName = aspuser.Email;
                aspuser.NormalizedEmail = aspuser.Email.ToUpper();
                aspuser.NormalizedUserName = aspuser.Email.ToUpper();
                aspuser.RowGuid = Guid.NewGuid();
                aspuser.SecurityStamp = "QBNZZTFG4NWSXHGGZKUEICAREMXKLZ7A";
                aspuser.TwoFactorEnabled = true;
                aspuser.ShowDemoProject = true;
                aspuser.EmailConfirmed = true;
                aspuser.Role = true; //true for admin
                aspuser.CompanyID = companyToReturn.Id;

                // crete user
                var aspuserToReturn = await _aspuserService.CreateEntityAsync<AspUserDto, AspUserForCreation>(aspuser);
                user = Mapper.Map<AspUserDto>(aspuserToReturn);

                // add user in company
                var companyUser = await _companyService.AddUserToCompany(user.Id, companyToReturn.Id.ToString(), "Admin");

                // send email to superadmins for new signup
                await _aspuserService.SendEmailToSuperAdminsForAppsumoSignup(aspuser.FName, aspuser.LName, aspuser.Email, aspuser.Password, aspuser.CName, response.CompanyImageUrl);

                // binding company RowGuid from user RowGuid
                await _companyService.UpdateCompanyRowGuid(aspuser.CompanyID, aspuser.RowGuid);

                // create company plan
                await _companyPlanService.createCompanyPlan(aspuser.CompanyID.ToString(), false, aspuser.NewPlanPaymentId, aspuser.NewPlanName);

                string tag = "Appsumo Deal";
                await _aspuserService.CreateUserInMailchimp(aspuser.Email, aspuser.FName, aspuser.LName, aspuser.CName, tag);

                //await _aspuserService.RegisterUserInGoHighLevel(aspuser.Email, aspuser.FName, aspuser.LName, aspuser.CName, tag);

                // return response
                return Ok(true);

            }
            catch (Exception ex)
            {
                AspUserDto test = new AspUserDto();
                test.Address = ex.Message;
                test.Email = ex.StackTrace;
                return test;
            }
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("UpdateAspUserwithImage", Name = "UpdateAspUserwithImage")]
        public ActionResult UpdateAspUserwithImage()
        {
            string subjectID = Request.Form["ID"];
            string oldFileName = Request.Form["ImageUrl"];
            string imageUri = "";
            AspUserDto user = new AspUserDto();

            try
            {
                // fetch file from Response
                if (Request.Form.Files.Count > 0)
                {
                    var file = Request.Form.Files[0];

                    // for add to TimeStamp in file Name
                    DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
                    TimeSpan span = (DateTime.UtcNow - epoch);
                    var timeStamp = span.TotalSeconds;

                    // set file credential for Upload to Azure Blob
                    Stream fileStream = file.OpenReadStream();
                    string contentType = file.ContentType;

                    //Users/95a54ae7-9853-42c2-aecc-51ab72ad7248_1573277141.78148.jpg
                    string fileName = "Users/" + subjectID + "_" + timeStamp + ".jpg";
                    string containerName = _configuration.GetSection("AzureBlob:AzureBlobContainer").Value;

                    if (!String.IsNullOrEmpty(oldFileName) && oldFileName != "undefined")
                    {
                        string rootDirectory = oldFileName.Substring(0, oldFileName.LastIndexOf('/') - 5);
                        oldFileName = oldFileName.Replace(rootDirectory, "");
                        // bool isDelete = _blobService.DeleteFromAzureBlob(oldFileName, containerName);
                    }

                    // call blobservice to Upload File and it will Return Image URL
                    //   imageUri = _blobService.UploadToAzureBlobFromStream(fileStream, contentType, fileName, containerName);
                }

                // get user data
                var userData = _aspuserService.GetEntityById(subjectID);

                string iDate = Request.Form["Birthday"];
                userData.Birthday = DateTime.ParseExact(iDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                if (Request.Form.Files.Count > 0)
                {
                    userData.ImageUrl = imageUri;
                }
                // fill the data
                if (!String.IsNullOrEmpty(Request.Form["FirstName"].ToString()))
                {
                    userData.FName = Request.Form["FirstName"];
                }
                if (!String.IsNullOrEmpty(Request.Form["LastName"].ToString()))
                {
                    userData.LName = Request.Form["LastName"];
                }
                if (!String.IsNullOrEmpty(Request.Form["Birthplace"].ToString()))
                {
                    userData.Birthplace = Request.Form["Birthplace"];
                }
                if (!String.IsNullOrEmpty(Request.Form["Gender"].ToString()))
                {
                    userData.Gender = Request.Form["Gender"];
                }
                if (!String.IsNullOrEmpty(Request.Form["Occupation"].ToString()))
                {
                    userData.Occupation = Request.Form["Occupation"];
                }
                if (!String.IsNullOrEmpty(Request.Form["PhoneNumber"].ToString()))
                {
                    userData.PhoneNumber = Request.Form["PhoneNumber"];
                }
                if (!String.IsNullOrEmpty(Request.Form["LivesIn"].ToString()))
                {
                    userData.LivesIn = Request.Form["LivesIn"];
                }

                _aspuserService.UpdateEntity(userData);
                _aspuserService.SaveChanges();

                user = Mapper.Map<AspUserDto>(userData);
            }
            catch (Exception ex)
            {
                return null;
            }

            return Ok(user);
        }

        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost(Name = "CreateAspUser")]
        //[Authorize(Policy = Policy.CreateUsers)]
        public async Task<ActionResult<AspUserDto>> CreateAspUser([FromBody] AspUserForCreation aspuser)
        {
            var loggedInUser = _userInfoService;
            AspUserDto user = new AspUserDto();

            // Make Request
            var uri = _configuration.GetSection("IdentityServerUrl").Value;
            var client = new RestClient(uri + "/Account/");

            // make call for Update or Create User


            // make Request
            var requestPost = new RestRequest("InviteUserMail", Method.Post);

            //if user available or not.
            var userExists = _aspuserService.GetAllEntities().Where(x => x.Email == aspuser.Email).FirstOrDefault();

            if (userExists == null)
            {
                //create a show in db.
                aspuser.Id = Guid.NewGuid().ToString();
                aspuser.UserName = aspuser.Email;
                aspuser.NormalizedEmail = aspuser.Email.ToUpper();
                aspuser.NormalizedUserName = aspuser.Email.ToUpper();
                aspuser.RowGuid = Guid.NewGuid();
                aspuser.PasswordHash = "AO/JuU7z5uQGvK9I7k5RH//hk2L+1fO0MgVbzSHuk1R5rK6dUL84EQN0stu5vd5/FQ==";
                aspuser.CompanyID = Guid.NewGuid();
                var aspuserToReturn = await _aspuserService.CreateEntityAsync<AspUserDto, AspUserForCreation>(aspuser);
                user = Mapper.Map<AspUserDto>(aspuserToReturn);

                // add Parameter
                requestPost.AddParameter("NewUser", true);
                requestPost.AddParameter("Email", user.Email);
                requestPost.AddParameter("AdminName", loggedInUser.FirstName + " " + loggedInUser.LastName);

            }
            else
            {
                user = Mapper.Map<AspUserDto>(userExists);

                // add Parameter
                requestPost.AddParameter("NewUser", false);
                requestPost.AddParameter("Email", user.Email);
                requestPost.AddParameter("AdminName", loggedInUser.FirstName + " " + loggedInUser.LastName);
                // create free companyPlan
            }

            // call Marvin Endpoint
            var responsePost = await client.PostAsync(requestPost);

            #region Add to other related tables.

            //var isUserExist = await _edge_UserAccountService.ExistAsync(x => x.AccountId == new Guid(loggedInUser.SelectedAccountId) && x.UserId == user.Id);
            //if (!isUserExist)
            //{
            //    // create UserAccount Entity and stored in DB
            //    Edge_UserAccount accountEntity = new Edge_UserAccount();
            //    accountEntity.CreatedOn = DateTime.UtcNow;
            //    accountEntity.CreatedBy = "system";
            //    accountEntity.UpdatedOn = DateTime.UtcNow;
            //    accountEntity.CompanyName = loggedInUser.SelectedDatabase;
            //    accountEntity.Id = new Guid();
            //    accountEntity.AccountId = new Guid(loggedInUser.SelectedAccountId);
            //    accountEntity.UserId = user.Id;
            //    accountEntity.PrimaryUser = false;

            //    _edge_UserAccountService.CreateEntity(accountEntity);
            //    _edge_UserAccountService.SaveChanges();
            //}

            //// create ClientUser Entity and stored in DB
            //if (!String.IsNullOrEmpty(loggedInUser.SelectedDatabase))
            //{
            //    var clientData = _edge_ClientAccountService.GetAllEntities()
            //        .Where(c => c.DBName == loggedInUser.SelectedDatabase && c.AccountId.ToString() == loggedInUser.SelectedAccountId)
            //        .FirstOrDefault();

            //    Edge_ClientUser clientEntity = new Edge_ClientUser();
            //    clientEntity.CreatedOn = DateTime.UtcNow;
            //    clientEntity.CreatedBy = "system";
            //    clientEntity.UpdatedOn = DateTime.UtcNow;
            //    clientEntity.CompanyName = loggedInUser.SelectedDatabase;
            //    clientEntity.Id = new Guid();
            //    clientEntity.AccountId = new Guid(loggedInUser.SelectedAccountId);
            //    clientEntity.UserID = user.Id;
            //    clientEntity.ClientId = clientData.Id;
            //    clientEntity.Role = aspuser.Role;

            //    _edge_ClientUserService.CreateEntity(clientEntity);
            //    _edge_ClientUserService.SaveChanges();
            //}
            #endregion
            //return the show created response.
            return CreatedAtRoute("GetAspUser", new { id = user.Id }, user);
        }


        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("InviteUser", Name = "InviteUser")]
        [AllowAnonymous]
        //[Authorize(Policy = Policy.CreateUsers)]
        public async Task<ActionResult<AspUserDto>> InviteUser()
        {
            var loggedInUser = _userInfoService;
            AspUserDto user = new AspUserDto();
            string fileExtension = string.Empty;

            try
            {
                AspUserForCreation aspuser = new AspUserForCreation();

                aspuser.CName = Request.Form["cName"];
                aspuser.Companytype = Request.Form["companytype"];
                aspuser.LName = Request.Form["lName"];
                aspuser.CustomDomain = Request.Form["customDomain"];
                aspuser.Email = Request.Form["email"];
                aspuser.FName = Request.Form["fName"];

                aspuser.NewPlanName = Request.Form["newPlanName"];
                aspuser.NewPlanPaymentId = Request.Form["newPlanPaymentId"];                

                // Make Request
                var uri = _configuration.GetSection("IdentityServerUrl").Value;
                var client = new RestClient(uri + "/Account/");
                var code = Guid.NewGuid();

                // make call for Update or Create User

                // make Request
                var requestPost = new RestRequest("InviteUserMail", Method.Post);

                //if user available or not.
                var userExists = _aspuserService.GetAllEntities(true).Where(x => x.Email == aspuser.Email).FirstOrDefault();

                string password = _aspuserService.GeneratePassword(true, true, true, false, 10);

                var image = aspuser.CompanyImageUrl;

                //var resizedForDashboard = image[0] + "base64," + _companyService.ResizeImage(image[1], 75, 75);

                //var resizedForFevicon = image[0] + "base64," + _companyService.ResizeImage(image[1], 32, 32);

                //Get dashboard logo by host url and pass into DashboardLogo

                var response = _companyService.GetCustomDomainCompanyInfo(aspuser.CustomDomain);


                if (userExists == null)
                {
                    var companyToReturn = await _companyService.CreateCompany(aspuser.CName, aspuser.Companytype, image, aspuser.SubDomain, image, image, fileExtension);

                    var hashPassword = _aspuserService.GetHash(password);
                    //create a show in db.
                    aspuser.Id = Guid.NewGuid().ToString();
                    aspuser.UserName = aspuser.Email;
                    aspuser.NormalizedEmail = aspuser.Email.ToUpper();
                    aspuser.NormalizedUserName = aspuser.Email.ToUpper();
                    aspuser.RowGuid = code;
                    aspuser.SecurityStamp = "QBNZZTFG4NWSXHGGZKUEICAREMXKLZ7A";
                    aspuser.PasswordHash = hashPassword;
                    aspuser.TwoFactorEnabled = true;
                    aspuser.ShowDemoProject = true;
                    aspuser.CompanyID = companyToReturn.Id;

                    if (aspuser.Role == false)
                    {
                        aspuser.Role = true; //true for admin
                    }


                    var aspuserToReturn = await _aspuserService.CreateEntityAsync<AspUserDto, AspUserForCreation>(aspuser);
                    user = Mapper.Map<AspUserDto>(aspuserToReturn);

                    var companyUser = await _companyService.AddUserToCompany(user.Id, companyToReturn.Id.ToString(), "Admin");


                    // add Parameter
                    requestPost.AddParameter("NewUser", true);
                    requestPost.AddParameter("Email", user.Email);
                    requestPost.AddParameter("AdminName", loggedInUser.FirstName + " " + loggedInUser.LastName);
                    requestPost.AddParameter("CompanyName", response.Name);
                    requestPost.AddParameter("CompanyId", aspuser.CompanyID);
                    requestPost.AddParameter("Code", code);
                    requestPost.AddParameter("Password", password);
                    requestPost.AddParameter("FromWhom", _configuration.GetSection("MailFrom").Value);
                    //requestPost.AddParameter("ReturnURL", aspuser.SubDomain);
                    requestPost.AddParameter("DashboardLogo", response.CompanyImageUrl);
                    requestPost.AddParameter("RequestedUrl", aspuser.CustomDomain);
                    requestPost.AddParameter("IsSendEmailToSuperAdmin", true);

                    await _companyService.UpdateCompanyRowGuid(aspuser.CompanyID, code);

                    // create free / fortynineusd companyPlan / Black friday deal
                    if (aspuser.NewPlanName != null && aspuser.NewPlanPaymentId != null && aspuser.NewPlanName != "" && aspuser.NewPlanPaymentId != "")
                    {
                        await _companyPlanService.createCompanyPlan(aspuser.CompanyID.ToString(), false, aspuser.NewPlanPaymentId, aspuser.NewPlanName);
                    }      
                    else
                    {
                        await _companyPlanService.createCompanyPlan(aspuser.CompanyID.ToString(), true);
                    }

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
                    requestPost.AddParameter("Email", user.Email);
                    requestPost.AddParameter("AdminName", loggedInUser.FirstName + " " + loggedInUser.LastName);
                    requestPost.AddParameter("CompanyName", response.Name);
                    requestPost.AddParameter("CompanyId", userExists.CompanyID);
                    requestPost.AddParameter("Code", code);
                    requestPost.AddParameter("Password", password);
                    requestPost.AddParameter("FromWhom", fromWhom);
                    //requestPost.AddParameter("ReturnURL", aspuser.SubDomain);
                    requestPost.AddParameter("DashboardLogo", response.CompanyImageUrl);
                    requestPost.AddParameter("RequestedUrl", aspuser.CustomDomain);
                    requestPost.AddParameter("IsSendEmailToSuperAdmin", true);

                    await _companyService.UpdateCompanyRowGuid(userExists.CompanyID, code);

                    await _companyPlanService.createCompanyPlan(userExists.CompanyID.ToString(), true);
                }
                // call Marvin Endpoint
                var responsePost = await client.PostAsync(requestPost);
                if (responsePost.IsSuccessful)
                {
                    var tag = string.Empty;
                    if (aspuser.NewPlanName != null && aspuser.NewPlanPaymentId != null && aspuser.NewPlanName != "" && aspuser.NewPlanPaymentId != "")
                    {
                        if (aspuser.NewPlanName == "LIFETIMEDEAL" || aspuser.NewPlanName == "LifeTimeDeal" || aspuser.NewPlanName == "lifetimedeal")
                        {
                            tag = "LTD website signup";
                            await _aspuserService.CreateUserInMailchimp(aspuser.Email, aspuser.FName, aspuser.LName, aspuser.CName, tag);

                            //await _aspuserService.RegisterUserInGoHighLevel(aspuser.Email, aspuser.FName, aspuser.LName, response.Name, tag);

                        }
                    }
                    else
                    {
                        tag = "14-days free trial";

                        await _aspuserService.CreateUserInMailchimp(aspuser.Email, aspuser.FName, aspuser.LName, aspuser.CName, tag);

                        //await _aspuserService.RegisterUserInGoHighLevel(aspuser.Email, aspuser.FName, aspuser.LName, response.Name, tag);

                        await _stripeCouponService.GenerateFiftyPercentCoupon(aspuser.CompanyID);
                    }

                    return Ok(true);
                }
                else
                {
                    return Ok(false);
                }

                #region related entites handling

                //var isUserExist = await _edge_UserAccountService.ExistAsync(x => x.AccountId == new Guid(loggedInUser.SelectedAccountId) && x.UserId == user.Id);
                //if (!isUserExist)
                //{
                //    // create UserAccount Entity and stored in DB
                //    Edge_UserAccount accountEntity = new Edge_UserAccount();
                //    accountEntity.CreatedOn = DateTime.UtcNow;
                //    accountEntity.CreatedBy = "system";
                //    accountEntity.UpdatedOn = DateTime.UtcNow;
                //    accountEntity.CompanyName = loggedInUser.SelectedDatabase;
                //    accountEntity.Id = new Guid();
                //    accountEntity.AccountId = new Guid(loggedInUser.SelectedAccountId);
                //    accountEntity.UserId = user.Id;
                //    accountEntity.PrimaryUser = false;

                //    _edge_UserAccountService.CreateEntity(accountEntity);
                //    _edge_UserAccountService.SaveChanges();
                //}

                //if (!String.IsNullOrEmpty(clientId))
                //{
                //    Edge_ClientUser clientEntity = new Edge_ClientUser();
                //    clientEntity.CreatedOn = DateTime.UtcNow;
                //    clientEntity.CreatedBy = "system";
                //    clientEntity.UpdatedOn = DateTime.UtcNow;
                //    clientEntity.CompanyName = loggedInUser.SelectedDatabase;
                //    clientEntity.Id = new Guid();
                //    clientEntity.AccountId = new Guid(loggedInUser.SelectedAccountId);
                //    clientEntity.UserID = user.Id;
                //    clientEntity.ClientId = new Guid(clientId);
                //    clientEntity.Role = aspuser.Role;

                //    _edge_ClientUserService.CreateEntity(clientEntity);
                //    _edge_ClientUserService.SaveChanges();
                //}

                #endregion

                //return the show created response.
                //return Ok(true); //CreatedAtRoute("GetAspUser", new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                AspUserDto test = new AspUserDto();
                test.Address = ex.Message;
                test.Email = ex.StackTrace;
                return test;
            }
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("ReSendUserInvite", Name = "ReSendUserInvite")]        
        //[Authorize(Policy = Policy.CreateUsers)]
        public async Task<ActionResult<bool>> ReSendUserInvite(string userId, string baseUrl)
        {
            var loggedInUser = _userInfoService;
            var retValue = false;

            try
            {
                if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(baseUrl))
                {
                    // Make Request
                    var uri = _configuration.GetSection("IdentityServerUrl").Value;
                    var client = new RestClient(uri + "/Account/");
                    var code = Guid.NewGuid();


                    // make Request
                    var requestPost = new RestRequest("ReSendUserInvite", Method.Post);

                    //if user available or not.
                    var userExistsData = _aspuserService.GetAllEntities(true).Where(x => x.Id == userId).FirstOrDefault();

                    if (userExistsData != null && !userExistsData.EmailConfirmed)
                    {
                        string password = _aspuserService.GeneratePassword(true, true, true, false, 10);

                        var response = _companyService.GetCustomDomainCompanyInfo(baseUrl);

                        var hashPassword = _aspuserService.GetHash(password);

                        userExistsData.PasswordHash = hashPassword;

                        userExistsData.RowGuid = code;

                        await _aspuserService.UpdateEntityAsync(userId, userExistsData);

                        var fromWhom = string.Empty;
                        var emailWhiteLabel = _emailWhitelabelRepository.GetFilteredEntities().Where(x => x.CompanyID == userExistsData.CompanyID && x.IsVerify == true).FirstOrDefault();
                        if (emailWhiteLabel != null)
                        {
                            fromWhom = "no-reply@" + emailWhiteLabel.DomainName;
                        }
                        else
                        {
                            {
                                fromWhom = _configuration.GetSection("MailFrom").Value;
                            }

                        }

                        requestPost.AddParameter("Email", userExistsData.Email);
                        requestPost.AddParameter("AdminName", loggedInUser.FirstName + " " + loggedInUser.LastName);
                        requestPost.AddParameter("CompanyName", response.Name);
                        requestPost.AddParameter("CompanyId", userExistsData.CompanyID);
                        requestPost.AddParameter("Code", code);
                        requestPost.AddParameter("Password", password);
                        requestPost.AddParameter("FromWhom", fromWhom);
                        requestPost.AddParameter("DashboardLogo", response.CompanyImageUrl);
                        requestPost.AddParameter("RequestedUrl", baseUrl);

                        await _companyService.UpdateCompanyRowGuid(userExistsData.CompanyID, code);
                        // call Marvin Endpoint
                        var responsePost =await client.PostAsync(requestPost);

                        if (responsePost.IsSuccessful)
                        {
                            retValue = true;

                        }
                        else
                        {
                            retValue = false;
                        }
                    }

                }
               
                return retValue;
              
            }
            catch (Exception ex)
            {
                AspUserDto test = new AspUserDto();
                test.Address = ex.Message;
                test.Email = ex.StackTrace;
                return false;
            }
        }
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("IsUserExists", Name = "IsUserExists")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> IsUserExists(string emailId, bool isInviteUser)
        {
            var isCompanyIdExist = Request.Headers.ContainsKey("SelectedCompanyId");
            if (isInviteUser && isCompanyIdExist)
            {
                Request.Headers.TryGetValue("SelectedCompanyId", out Microsoft.Extensions.Primitives.StringValues companyId);
                return await _aspuserService.ExistAsync(x => x.Email == emailId.ToLower() && x.CompanyID == new Guid(companyId));

            }
            else
            {
                return await _aspuserService.ExistAsync(x => x.Email == emailId.ToLower());
            }

        }

    

        #endregion


        #region HTTPDELETE
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}", Name = "DeleteAspUserById")]
        public async Task<IActionResult> DeleteAspUserById(string id)
        {
            //if the aspuser exists
            if (await _aspuserService.ExistAsync(x => x.Id == id.ToString()))
            {
                //delete the aspuser from the db.
                await _aspuserService.DeleteEntityAsync(id);
            }
            else
            {
                //if aspuser doesn't exists then returns not found.
                return NotFound();
            }

            //return the response.
            return NoContent();
        }

        #endregion


        #region PRIVATE METHODS

        private IEnumerable<LinkDto> CreateLinksForAspUser(string id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetAspUser", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetAspUser", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteAspUserById", new { id = id }),
              "delete_aspuser",
              "DELETE"));

            links.Add(
             new LinkDto(_urlHelper.Link("UpdateAspUser", new { id = id }),
             "update_aspuser",
             "PUT"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateAspUser", new { }),
              "create_aspuser",
              "POST"));

            //links.Add(
            //   new LinkDto(_urlHelper.Link("GetShowsForTour", new { }),
            //   "shows",
            //   "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForAspUsers(FilterOptionsModel filterOptionsModel, bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(CreateAspUsersResourceUri(filterOptionsModel, ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateAspUsersResourceUri(filterOptionsModel, ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateAspUsersResourceUri(filterOptionsModel, ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }

        private string CreateAspUsersResourceUri(FilterOptionsModel filterOptionsModel, ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetFilteredAspUsers",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber - 1,
                          pageSize = filterOptionsModel.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetFilteredAspUsers",
                      new
                      {
                          fields = filterOptionsModel.Fields,
                          orderBy = filterOptionsModel.OrderBy,
                          searchQuery = filterOptionsModel.SearchQuery,
                          pageNumber = filterOptionsModel.PageNumber + 1,
                          pageSize = filterOptionsModel.PageSize
                      });

                default:
                    return _urlHelper.Link("GetFilteredAspUsers",
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
