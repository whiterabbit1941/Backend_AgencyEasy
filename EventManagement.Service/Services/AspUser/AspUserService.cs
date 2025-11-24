using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using FinanaceManagement.API.Models;
using EventManagement.Service;
using EventManagement.Dto;
using EventManagement.Domain;
using System.Security.Cryptography;
using RestSharp;
using static IdentityServer4.Models.IdentityResources;
using Microsoft.AspNetCore.Identity;
using SendGrid.Helpers.Mail;
using SendGrid;
using MailChimp.Net.Interfaces;
using MailChimp.Net;
using MailChimp.Net.Models;
using Method = RestSharp.Method;

namespace EventManagement.Service
{
    public class AspUserService : ServiceBase<AspNetUsers, string>, IAspUserService
    {

        #region PRIVATE MEMBERS

        private readonly IAspUserRepository _aspuserRepository;
        private readonly IConfiguration _configuration;           
        private readonly ICampaignRepository _campaignRepository;
        private readonly ISerpRepository _serpRepository;

        #endregion


        #region CONSTRUCTOR

        public AspUserService(IAspUserRepository aspuserRepository, ILogger<AspUserService> logger, IConfiguration configuration,
            ICampaignRepository campaignRepository,ISerpRepository serpRepository) : base(aspuserRepository, logger)
        {
            _aspuserRepository = aspuserRepository;
            _configuration = configuration;
            _serpRepository = serpRepository;
            _campaignRepository = campaignRepository;
        }

        #endregion


        #region PUBLIC MEMBERS   


        public AspUserDto GetUserDetails(string userId)
        {
            //then get the whole entity and map it to the Dto.
            var aspuserEntity = _aspuserRepository.GetAllEntities(true).Where(x => x.Id == userId).Select(
                user => new AspUserDto
                {
                    CompanyID = user.CompanyID,
                    FName = user.FName,
                    LName = user.LName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Id = user.Id,
                    EmailConfirmed = user.EmailConfirmed,
                    Role = user.Role,
                    ImageUrl = user.ImageUrl,
                    UserName = user.UserName,
                    NormalizedEmail = user.NormalizedEmail,
                    NormalizedUserName = user.NormalizedUserName,
                    PasswordHash = user.PasswordHash,
                    SecurityStamp = user.SecurityStamp,
                    AccessFailedCount = user.AccessFailedCount,
                    LockoutEnabled = user.LockoutEnabled,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    ShowDemoProject = user.ShowDemoProject,
                    Birthday = user.Birthday,
                    Address = user.Address,
                    RowGuid = user.RowGuid,
                    CreatedOn = user.CreatedOn

                }

                ).FirstOrDefault();

            return aspuserEntity;
        }
    
        public int GetProjectCreatedCount(Guid companyID)
        {
           return _campaignRepository.GetAllEntities(false).Where(x => x.CompanyID == companyID).Count();
        }


        public int GetSerpCreatedCount(Guid companyID)
        {
            var campaignIds = _campaignRepository.GetAllEntities(false).Where(x => x.CompanyID == companyID).Select(x => x.Id).ToList();
            var latestKeywordListData = _serpRepository.GetAllEntities(true).Where(x => campaignIds.Contains(x.CampaignID)).Select(y => new SerpDto
            {
                Id = y.Id,
                Keywords = y.Keywords,
                LocationName = y.LocationName

            }).ToList();

            var count = latestKeywordListData.Distinct(new KeywordEqualityComparer()).Count();

            return count;
        }

        /// <summary>
        /// Generate password 
        /// </summary>
        /// <param name="useLowercase">useLowercase</param>
        /// <param name="useUppercase">useUppercase</param>
        /// <param name="useNumbers">useNumbers</param>
        /// <param name="useSpecial">useSpecial</param>
        /// <param name="passwordSize">passwordSize</param>
        /// <returns>status of the operation</returns>
        public string GeneratePassword(bool useLowercase, bool useUppercase, bool useNumbers, bool useSpecial,
           int passwordSize)
        {

            const string LOWER_CASE = "abcdefghijklmnopqursuvwxyz";
            const string UPPER_CAES = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string NUMBERS = "123456789";
            const string SPECIALS = @"!@$%^&*()#";

            char[] _password = new char[passwordSize];
            string charSet = ""; // Initialise to blank
            System.Random _random = new Random();
            int counter;

            // Build up the character set to choose from
            if (useLowercase) charSet += LOWER_CASE;

            if (useUppercase) charSet += UPPER_CAES;

            if (useNumbers) charSet += NUMBERS;

            if (useSpecial) charSet += SPECIALS;

            for (counter = 0; counter < passwordSize; counter++)
            {
                _password[counter] = charSet[_random.Next(charSet.Length - 1)];
            }

            return String.Join(null, _password);
        }

        /// <summary>
        /// Preparing hash password
        /// </summary>
        /// <param name="password">plain text</param>
        /// <returns>Hash password</returns>
        public string GetHash(string password)
        {
            byte[] salt;
            byte[] bytes;
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            using (Rfc2898DeriveBytes rfc2898DeriveByte = new Rfc2898DeriveBytes(password, 16, 1000))
            {
                salt = rfc2898DeriveByte.Salt;
                bytes = rfc2898DeriveByte.GetBytes(32);
            }
            byte[] numArray = new byte[49];
            Buffer.BlockCopy(salt, 0, numArray, 1, 16);
            Buffer.BlockCopy(bytes, 0, numArray, 17, 32);
            return Convert.ToBase64String(numArray);
        }

        public async Task<bool> CreateUserInMailchimp(string email, string fname, string lname,string companyName, string tagData)
        {
            var mailchimpListId = _configuration.GetSection("MailChimp:ListId").Value;

            var apiKey = _configuration.GetSection("MailChimp:ApiKey").Value;

            try
            {
                //Instantiate new manager
                IMailChimpManager mailChimpManager = new MailChimpManager(apiKey);
                bool res = false;
                List<MemberTag> tag = new List<MemberTag>();
                Member member = new Member();
               
                tag = new List<MemberTag> { new MemberTag { Name = tagData } };
                member = mailChimpManager.Members.GetAllAsync(mailchimpListId).Result.Where(m => m.EmailAddress == email && m.Tags.Equals(tagData)).FirstOrDefault();
       
                if (member == null)
                {
                    member = new Member { EmailAddress = email, StatusIfNew = Status.Subscribed, Tags = tag };
                }
                else
                {
                    if (member.Status != Status.Subscribed)
                    {
                        member.Status = Status.Subscribed;
                    }
                }

                member.MergeFields.Add("FNAME", fname);
                member.MergeFields.Add("LNAME", lname);
                member.MergeFields.Add("CNAME", companyName);

                var resMember = mailChimpManager.Members.AddOrUpdateAsync(mailchimpListId, member).Result;

                if (resMember != null)
                {
                    res = true;
                }

                return res;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task RegisterUserInGoHighLevel(string emailid, string firstName, string lastName, string companyName, string tag)
        {
            var client = new RestClient("https://rest.gohighlevel.com");
            var request = new RestRequest("/v1/contacts/", Method.Post);
            request.AddHeader("Authorization", "Bearer " + _configuration.GetSection("GoHighLevelApiKey").Value);

            var obj = new
            {
                email = emailid,
                firstName = firstName,
                lastName = lastName,
                companyName = companyName,
                tags = new string[] { tag },
            };

            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(obj);

            var response = await client.ExecuteAsync(request);
        }

        /// <summary>
        /// Call this API when user Click a Confirm from confirmation email
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>Status of the operation</returns>
        public async Task<int> UpdateUserPartially(string id)
        {
            return await _aspuserRepository.UpdateBulkEntityAsync(y => new AspNetUsers { EmailConfirmed = true }, x => x.Id == id);

        }

        public async Task<bool> SendEmailToSuperAdminsForAppsumoSignup(string FName, string LName, string Email, string Password, string CompanyName, string DashboardLogo)
        {
            var emailContent = string.Empty;
            emailContent = "<table border=\"0\" cellpadding =\"0\" cellspacing =\"0\" width =\"100%\" style =\"table-layout:fixed;background-color:#f9f9f9\" id =\"bodyTable\">" +
            "<tbody>" +
            "<tr>" +
            "<td style=\"padding-right:10px;padding-left:10px;\" align=\"center\" valign=\"top\" id=\"bodyCell\">" +
            "<table border=\"0\" cellpadding =\"0\" cellspacing=\"0\" width=\"100%\" class=\"wrapperWebview\" style =\"max-width:600px\">" +
            "<tbody>" +
            "<tr>" +
            "<td align=\"center\" valign=\"top\">" +
            "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">" +
            "<tbody>" +
            "<tr>" +
            "<td style=\"padding-top: 20px; padding-bottom: 20px; padding-right: 0px;\" align=\"right\" valign =\"middle\" class=\"webview\"> <a href=\"#\" style=\"color:#bbb;font-family:'Montserrat', sans-serif;font-size:12px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:20px;text-transform:none;text-align:right;text-decoration:underline;padding:0;margin:0\" target=\"_blank\" class=\"text hideOnMobile\"></a>" +
            "</td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            "</td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"wrapperBody\" style=\"max-width:600px\">" +
            "<tbody>" +
            "<tr>" +
            "<td align=\"center\" valign=\"top\">" +
            "<table border=\"0\" cellpadding =\"0\" cellspacing =\"0\" width =\"100%\" class=\"tableCard\" style =\"background-color:#fff;border-color:#e5e5e5;border-style:solid;border-width:0 1px 1px 1px;\" > " +
            "<tbody>" +
            "<tr>" +
            "<td style=\"background-color:#2f40f6;font-size:1px;line-height:3px\" class=\"topBorder\" height =\"3\" > &nbsp;</td>" +
            "</tr>" +
            "<tr>" +
            "<td style=\"padding-top: 60px; padding-bottom: 20px;\" align=\"center\" valign=\"middle\" class=\"emailLogo\">" +
            "<a href=\"#\" style=\"text-decoration:none\" target =\"_blank\">" +
            "<img border=\"0\" src='" + DashboardLogo + "' style=\"width:100%;max-width:200px;height:auto;display:block\" width=\"150\">" +
            "</a>" +
            "</td>" +
            "</tr>" +
            "<tr>" +
            "<td style=\"padding-bottom: 5px; padding-left: 20px; padding-right: 20px;\" align=\"center\" valign=\"top\" class=\"mainTitle\">" +
            "<h2 class=\"text\" style=\"color:#000;font-family:'Montserrat', sans-serif;font-size:28px;font-weight:500;font-style:normal;letter-spacing:normal;line-height:36px;text-transform:none;text-align:center;padding:0;margin:0;margin-bottom: 0px;margin-top: 30px;\">New Agency Registered</h2>" +
            "</td>" +
            "</tr>" +
            "<tr>" +
            "<td style=\"padding-left:20px;padding-right:20px\" align =\"center\" valign =\"top\" class=\"containtTable ui-sortable\"> " +
            "<table border=\"0\" cellpadding =\"0\" cellspacing =\"0\" width =\"100%\" class=\"tableDescription\" style=\"margin-top: 40px\">" +
            "<tbody>" +
            "<tr>" +
            "<td style=\"padding-bottom: 15px;\" align=\"center\" valign=\"top\" class=\"description\">" +
            "<h3 class=\"text\" style =\"color:#000;font-family:'Montserrat', sans-serif;font-size:22px;font-weight:500;font-style:normal;letter-spacing:normal;line-height:36px;text-transform:none;text-align:center;padding:0;margin:0;margin-bottom: 10px\"> Congratulations! " + FName + " " + LName + " has signed up on " + CompanyName + " from Appsumo Deal.</h3></tr>" +
            "<tr><td style = \"padding-bottom: 20px;\" align = \"center\" valign = \"top\" class=\"description\"><p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:18px;font-weight:700;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\">User Email = " + Email + "</p></td>" +
            "<tr><td style = \"padding-bottom: 20px;\" align = \"center\" valign = \"top\" class=\"description\"><p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:18px;font-weight:700;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\">Password = " + Password + "</p></td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableButton\">" +
            "<tbody>" +
            "<tr>" +
            "<td style=\"padding-top:20px;padding-bottom:20px\" align=\"center\" valign=\"top\">" +
            "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\">" +
            "<tbody>" +
            "</tbody>" +
            "</table>" +
            "</td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableDescription\">" +
            "<tbody>" +
            "</tbody>" +
            "</table>" +
            "</td>" +
            "</tr>" +
            "<tr>" +
            "<td style=\"font-size:1px;line-height:1px\" height=\"20\">&nbsp;</td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"space\">" +
            "<tbody>" +
            "<tr>" +
            "<td style=\"font-size:1px;line-height:1px\" height=\"30\">&nbsp;</td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            "</td>" +
            "</tr>" +
            "</tbody>" +
            "</table>" +
            "</td>" +
            "</tr>" +
            "</tbody>" +
            "</table>";

            var client = new SendGridClient(_configuration.GetSection("Client").Value);

            List<EmailAddress> listOfEmail = new List<EmailAddress>();
            var notificationEmails = _configuration.GetSection("NotificationEmails").Value;
            if (!string.IsNullOrEmpty(notificationEmails))
            {
                foreach (var email in notificationEmails.Split(','))
                {
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        listOfEmail.Add(new EmailAddress(email.Trim()));
                    }
                }
            }

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(new EmailAddress(_configuration.GetSection("MailFrom").Value), listOfEmail,
                "New Agency Registered: " + FName + " " + LName + "  has signed up on " + CompanyName, "", emailContent);

            var response = await client.SendEmailAsync(msg);

            return response.IsSuccessStatusCode;
        }
        #endregion


        #region OVERRIDDEN IMPLEMENTATION

        public override Dictionary<string, PropertyMappingValue> GetPropertyMapping()
        {
            return new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Id", new PropertyMappingValue(new List<string>() { "Id" } ) },
                        { "Name", new PropertyMappingValue(new List<string>() { "Name" } )},
                    };
        }

        public override string GetDefaultOrderByColumn()
        {
            return "Id";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "Id,Email,UserName, FName, LName, ImageUrl, PhoneNumber,  Gender, Birthplace, Birthday, LivesIn, Occupation, EmailConfirmed, TwoFactorEnabled, Company,Address,ShowDemoProject,CreatedOn";
        }

        #endregion


    }
}
