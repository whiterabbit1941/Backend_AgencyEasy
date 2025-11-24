using System;
using System.Collections.Generic;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using EventManagement.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using AutoMapper;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Amazon.S3;
using Amazon;
using Amazon.S3.Model;
using SendGrid;
using SendGrid.Helpers.Mail;
using static IdentityServer4.Models.IdentityResources;
using IdentityModel.Client;

namespace EventManagement.Service
{
    public class CompanyService : ServiceBase<Company, Guid>, ICompanyService
    {

        #region PRIVATE MEMBERS

        private readonly ICompanyRepository _companyRepository;
        private readonly IConfiguration _configuration;
        private readonly IAspUserService _aspuserService;
        private readonly IAspUserRepository _aspuserRepository;
        private readonly ICompanyUserRepository _companyUserRepository;
        private IDomainWhitelabelRepository _domainWhitelabelRepository;
        private readonly ICompanyPlanRepository _companyPlanRepository;


        #endregion


        #region CONSTRUCTOR

        public CompanyService(IDomainWhitelabelRepository domainWhitelabelRepository, ICompanyUserRepository companyUserRepository, 
            ICompanyRepository companyRepository, ILogger<CompanyService> logger, 
            IConfiguration configuration, IAspUserService aspuserService,
            IAspUserRepository aspuserRepository, ICompanyPlanRepository companyPlanRepository) : base(companyRepository, logger)
        {
            _companyRepository = companyRepository;
            _configuration = configuration;
            _aspuserService = aspuserService;
            _aspuserRepository = aspuserRepository;
            _companyUserRepository = companyUserRepository;
            _domainWhitelabelRepository = domainWhitelabelRepository;
            _companyPlanRepository = companyPlanRepository;
        }

        #endregion


        #region PUBLIC MEMBERS   

        public List<SuperAdminDashboard> GetAdminDashboard(string userId,bool isAdmin)
        {
            IEnumerable<SuperAdminDashboard> myCompanyList;

            if (isAdmin)
            {
                var query = from company in _companyRepository.GetAllEntities()
                            
                            join plan in _companyPlanRepository.GetAllEntities().Where(p => p.Active)
                                on company.Id equals plan.CompanyId into plans
                            from plan in plans.DefaultIfEmpty() // Left join for plans

                                // Get users without ordering here
                            join user in _aspuserRepository.GetAllEntities()
                                on company.Id equals user.CompanyID into users
                            from user in users.DefaultIfEmpty() // Left join for users

                            select new SuperAdminDashboard
                            {
                                Id = company.Id,
                                CompanyID = company.Id,
                                Name = company.Name,
                                PlanName = plan != null ? plan.DefaultPlan.Name : null,
                                Active = plan != null ? plan.Active : (bool?)null,
                                OldestUserEmail = user != null ? user.Email : null, // Include the user ID for later ordering
                                OldestUserCreatedOn = user != null ? user.CreatedOn : (DateTime?)null,
                                ExpiredOn = plan != null ? plan.ExpiredOn : (DateTime?)null,
                                Role = "Super Admin",
                                CompanyImageUrl = company != null ? company.CompanyImageUrl : null,
                                IsAllowMarketPlace = company != null ? company.IsAllowMarketPlace : false,
                                Address = company != null ? company.Address : "",
                                State = company != null ? company.State : "",
                                Phone = company != null ? company.Phone : "",
                                City = company != null ? company.City : "",
                                ZipCode = company != null ? company.ZipCode : "",
                                Theme = company != null ? company.Theme : "",
                                CreatedOn = company.CreatedOn,
                                CompanyType = company.CompanyType,
                                Fevicon = company.Fevicon
                            };

                myCompanyList = query.ToList()
                    .GroupBy(x => x.CompanyID)
                    .Select(group => group.OrderBy(x => x.OldestUserCreatedOn).FirstOrDefault())
                    .Select(x => new SuperAdminDashboard
                    {
                        Id = x.Id,
                        CompanyID = x.Id,
                        Name = x.Name,
                        PlanName = x.PlanName,
                        Active = x.Active,
                        OldestUserEmail = x.OldestUserEmail,
                        OldestUserCreatedOn = x.OldestUserCreatedOn,
                        ExpiredOn = x.ExpiredOn,
                        Role = "Super Admin",
                        CompanyImageUrl = x != null ? x.CompanyImageUrl : null,
                        IsAllowMarketPlace = x != null ? x.IsAllowMarketPlace : false,
                        Address = x != null ? x.Address : "",
                        State = x != null ? x.State : "",
                        Phone = x != null ? x.Phone : "",
                        City = x != null ? x.City : "",
                        ZipCode = x != null ? x.ZipCode : "",
                        Theme = x != null ? x.Theme : "",
                        CreatedOn = x.CreatedOn,
                        CompanyType = x.CompanyType,
                        Fevicon = x.Fevicon
                    });
            }
            else
            {
                var companyIdList = _companyUserRepository.GetAllEntities(true)
                .Where(c => c.UserId == userId).FirstOrDefault();

                var query = from company in _companyRepository.GetAllEntities()
                            where company.Id == companyIdList.CompanyId
                            join plan in _companyPlanRepository.GetAllEntities().Where(p => p.Active)
                                on company.Id equals plan.CompanyId into plans
                            from plan in plans.DefaultIfEmpty() // Left join for plans

                                // Get users without ordering here
                            join user in _aspuserRepository.GetAllEntities()
                                on company.Id equals user.CompanyID into users
                            from user in users.DefaultIfEmpty() // Left join for users

                            select new SuperAdminDashboard
                            {
                                Id = company.Id,
                                CompanyID = company.Id,
                                Name = company.Name,
                                PlanName = plan != null ? plan.DefaultPlan.Name : null,
                                Active = plan != null ? plan.Active : (bool?)null,
                                OldestUserEmail = user != null ? user.Email : null, // Include the user ID for later ordering
                                OldestUserCreatedOn = user != null ? user.CreatedOn : (DateTime?)null,
                                ExpiredOn = plan != null ? plan.ExpiredOn : (DateTime?)null,
                                Role = companyIdList.Role,
                                CompanyImageUrl = company != null ? company.CompanyImageUrl : null,
                                IsAllowMarketPlace = company != null ? company.IsAllowMarketPlace : false,
                                Address = company != null ? company.Address : "",
                                State = company != null ? company.State : "",
                                Phone = company != null ? company.Phone : "",
                                City = company != null ? company.City : "",
                                ZipCode = company != null ? company.ZipCode : "",
                                Theme = company != null ? company.Theme : "",
                                CreatedOn = company.CreatedOn,
                                CompanyType = company.CompanyType,
                                Fevicon = company.Fevicon


                            };

                myCompanyList = query.ToList()
                    .GroupBy(x => x.CompanyID)
                    .Select(group => group.OrderBy(x => x.OldestUserCreatedOn).FirstOrDefault())
                    .Select(x => new SuperAdminDashboard
                    {
                        Id = x.Id,
                        CompanyID = x.CompanyID,
                        Name = x.Name,
                        PlanName = x.PlanName,
                        Active = x.Active,
                        OldestUserEmail = x.OldestUserEmail,
                        OldestUserCreatedOn = x.OldestUserCreatedOn,
                        ExpiredOn = x.ExpiredOn,                      
                        Role = companyIdList.Role,
                        CompanyImageUrl = x != null ? x.CompanyImageUrl : null,
                        IsAllowMarketPlace = x != null ? x.IsAllowMarketPlace : false,
                        Address = x != null ? x.Address : "",
                        State = x != null ? x.State : "",
                        Phone = x != null ? x.Phone : "",
                        City = x != null ? x.City : "",
                        ZipCode = x != null ? x.ZipCode : "",
                        Theme = x != null ? x.Theme : "",
                        CreatedOn = x.CreatedOn,
                        CompanyType = x.CompanyType,
                        Fevicon = x.Fevicon
                    });
            }               

            var result = myCompanyList.ToList();

            return result ;
        }

        public CompanyDto GetCompanyByUserEmail(string email)
        {
            var returnData = new CompanyDto();

            // get user
            var user = _aspuserService.GetAllEntities().AsQueryable().Where(x => x.Email == email).FirstOrDefault();
            if (user != null)
            {
                var company = _companyRepository.GetEntityById(user.CompanyID);
                returnData = Mapper.Map<CompanyDto>(company);
            }

            return returnData;
        }
        public CompanyDto GetCompany(string userId)
        {
            //then get the whole entity and map it to the Dto.
            var aspuserEntity = _aspuserRepository.GetAllEntities(true).Where(x => x.Id == userId).Select(user => new AspUserDto { CompanyID = user.CompanyID }).FirstOrDefault();


            //then get the whole entity and map it to the Dto.
            var company = _companyRepository.GetAllEntities(true).Where(x => x.Id == aspuserEntity.CompanyID).Select(y => new CompanyDto
            {
                Id = y.Id,
                Name = y.Name,
                Address = y.Address,
                Branding = y.Branding,
                City = y.City,
                CompanyID = y.Id,
                Country = y.Country,
                Description = y.Description,
                Phone = y.Phone,
                State = y.State,
                Timezone = y.Timezone,
                Website = y.Website,
                ZipCode = y.ZipCode,
                CompanyImageUrl = y.CompanyImageUrl,
                IsApproved = y.IsApproved,
                IsAllowMarketPlace = y.IsAllowMarketPlace,
                VatNo = y.VatNo,
                SubDomain = y.SubDomain,
                DashboardLogo = y.DashboardLogo,
                Fevicon = y.Fevicon
            }
            ).FirstOrDefault();

            return company;
        }
        /// <summary>
        /// Get Company Details By Domain
        /// </summary>
        /// <param name="domain">domain</param>
        /// <returns>Company</returns>
        public Company GetCompanyDetailsByDomain(string domain)
        {
            var company = _companyRepository.GetFilteredEntities(true).Where(x => x.SubDomain.Contains(domain)).FirstOrDefault();

            return company;
        }

        public CustomDomainCompanyInfo GetCustomDomainCompanyInfo(string domain)
        {
            //check requested url
            var companyImageUrl = "https://identity.agencyeasy.com/agencyLogo.png";
            var companyName = "Agencyeasy";
            var linkedAgencyCompanyId = string.Empty;
            if (domain.Contains(".whitelabelboard.com"))
            {
                var company = _companyRepository.GetAllEntities(true).Where(x => x.SubDomain.Contains(domain)).FirstOrDefault();
                companyImageUrl = company.CompanyImageUrl;
                companyName = company.Name;
                linkedAgencyCompanyId = company.Id.ToString();
            }
            else if (domain.Contains("app.agencyeasy.com") || domain.Contains("agencytest.e-intelligence.co")
                || domain.Contains("staging-ci-cd.d23ojfso9jathk.amplifyapp.com") || domain.Contains("localhost:3000"))
            {
                companyImageUrl = "https://identity.agencyeasy.com/agencyLogo.png";
                companyName = "Agencyeasy";
                linkedAgencyCompanyId = string.Empty;

            }
            else
            {
                var companyDetails = _domainWhitelabelRepository.GetAllEntities(true).Where(x => domain.Contains(x.AlternateDomainName)).Select(x => new DomainWhitelabel
                {
                    Company = x.Company

                }).FirstOrDefault();

                if (companyDetails == null)
                {
                    companyImageUrl = "https://identity.agencyeasy.com/agencyLogo.png";
                    companyName = "Agencyeasy";
                }
                else
                {
                    companyImageUrl = companyDetails.Company.CompanyImageUrl;
                    companyName = companyDetails.Company.Name;
                    linkedAgencyCompanyId = companyDetails.Company.Id.ToString();
                }
            }
            return new CustomDomainCompanyInfo() { CompanyImageUrl = companyImageUrl, LinkedAgencyCompanyId = linkedAgencyCompanyId, Name = companyName };
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        /// <summary>
        /// Convert Bitmap To Base64
        /// </summary>
        /// <param name="bitMap">bitMap</param>
        /// <returns>string</returns>
        public string ConvertBitmapToBase64(Bitmap bitMap)
        {
            Bitmap bImage = bitMap;  // Your Bitmap Image
            System.IO.MemoryStream ms = new MemoryStream();
            bImage.Save(ms, ImageFormat.Jpeg);
            byte[] byteImage = ms.ToArray();
            var SigBase64 = Convert.ToBase64String(byteImage);

            return SigBase64;
        }

        /// <summary>
        /// Convert Base64 To Image
        /// </summary>
        /// <param name="baseImageUrl">baseImageUrl</param>
        /// <returns>Image</returns>
        public Image ConvertBase64ToImage(string baseImageUrl)
        {
            //data:image/gif;base64,
            //this image is a single pixel (black)
            byte[] bytes = Convert.FromBase64String(baseImageUrl);

            Image image = null;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
            }

            var imageFile = image;
            image.Dispose();

            return imageFile;
        }

        /// <summary>
        /// ResizeImage
        /// </summary>
        /// <param name="base64">base64</param>
        /// <param name="width">width</param>
        /// <param name="height">height</param>
        /// <returns>string</returns>
        public string ResizeImage(string base64, int width, int height)
        {
            var image = ConvertBase64ToImage(base64);

            var bmp = ResizeImage(image, width, height);

            var resizedBase64 = ConvertBitmapToBase64(bmp);

            return resizedBase64;
        }

        /// <summary>
        /// Create Company
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="type">type</param>
        /// <param name="imageUrl">imageUrl</param>
        /// <param name="subDomain">subDomain</param>
        /// <param name="dashboardLogo">dashboardLogo</param>
        /// <param name="feviconIcon">feviconIcon</param>
        /// <returns>Task<CompanyDto></returns>
        public async Task<CompanyDto> CreateCompany(string name, string type, string imageUrl, string subDomain, string dashboardLogo, string feviconIcon, string extension)
        {
            CompanyForCreation companyCreation = new CompanyForCreation();
            companyCreation.Name = name;
            companyCreation.CompanyType = type;
            companyCreation.CompanyImageUrl = imageUrl;
            companyCreation.SubDomain = subDomain;
            companyCreation.DashboardLogo = dashboardLogo;
            companyCreation.Fevicon = feviconIcon;
            companyCreation.IsAllowMarketPlace = false;
            companyCreation.IsApproved = true;

            var companyToReturn = await CreateEntityAsync<CompanyDto, CompanyForCreation>(companyCreation);
            //var uploadedUrl = await UploadImageToAws(companyToReturn.Id, imageUrl, "companyImageUrl."+ extension);
            //var dashboardLogosAwsUrl = await UploadImageToAws(companyToReturn.Id, dashboardLogo , "dashboardLogo." + extension);
            //var feviconIconAwsUrl = await UploadImageToAws(companyToReturn.Id, feviconIcon, "feviconIcon." + extension);

            //await _companyRepository.UpdateBulkEntityAsync(y => new Company { CompanyImageUrl = uploadedUrl,DashboardLogo =dashboardLogosAwsUrl,Fevicon=feviconIconAwsUrl }, x => x.Id == companyToReturn.Id);            

            return companyToReturn;
        }

        public async Task<string> UploadImageToAws(Guid cid, string base64, string fileName)
        {
            var preparedUrl = string.Empty;
            string configaccess = _configuration.GetSection("AWS:AccessKeyID").Value;
            string configsecret = _configuration.GetSection("AWS:SecretAccessKeyID").Value;

            var s3Client = new AmazonS3Client(
                configaccess,
                configsecret,
                RegionEndpoint.USEast1
            );


            try
            {
                byte[] bytes = Convert.FromBase64String(base64);

                using (s3Client)
                {
                    var request = new PutObjectRequest
                    {
                        BucketName = "whitelabelboardimages",
                        CannedACL = S3CannedACL.PublicRead,
                        Key = string.Format(cid.ToString() + "/{0}", fileName)
                    };
                    using (var ms = new MemoryStream(bytes))
                    {
                        request.InputStream = ms;
                        await s3Client.PutObjectAsync(request);
                        preparedUrl = "https://whitelabelboardimages.s3.amazonaws.com/" + cid + "/" + fileName;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("AWS Fail");
            }

            return preparedUrl;

        }

        public async Task<CompanyUserDto> AddUserToCompany(string userId, string companyId, string role)
        {
            // create CompanyUser Entity and stored in DB
            CompanyUser companyEntity = new CompanyUser();
            companyEntity.CreatedOn = DateTime.UtcNow;
            companyEntity.CreatedBy = "system";
            companyEntity.UpdatedOn = DateTime.UtcNow;
            companyEntity.UpdatedBy = "system";
            companyEntity.Id = new Guid();
            companyEntity.CompanyId = new Guid(companyId);
            companyEntity.UserId = userId;
            companyEntity.Role = role;

            _companyUserRepository.CreateEntity(companyEntity);
            _companyUserRepository.SaveChanges();

            var returnData = Mapper.Map<CompanyUserDto>(companyEntity);

            return returnData;
        }


        public async Task<CompanyDto> UpdateCompanyPartially(Guid id, string requestedUrl)
        {
            var companyToReturn = await _companyRepository.UpdateBulkEntityAsync(y => new Company { IsApproved = true }, x => x.Id == id);
            var FromWhom = _configuration.GetSection("MailFrom").Value;
            var callbackUrl = requestedUrl + _configuration.GetSection("ReturnUrlSuperAdminAgencyVerification").Value;
            var companyDetails1 = _companyRepository.GetAllEntities().Where(x => x.Id == id).FirstOrDefault();

            var res = GetCustomDomainCompanyInfo(requestedUrl);

            var user = _aspuserService.GetAllEntities().Where(x => x.CompanyID == id).FirstOrDefault();

            var msgContent = "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"table-layout:fixed; background-color:#f9f9f9\" id=\"bodyTable\">" +
                "<tbody>" +
                "<tr>" +
                "<td style=\"padding-right:10px;padding-left:10px;\" align=\"center\" valign=\"top\" id=\"bodyCell\">" +
                "<table border =\"0\" cellpadding =\"0\" cellspacing=\"0\" width=\"100%\" class=\"wrapperWebview\" style=\"max-width:600px\">" +
                "<tbody>" +
                "<tr>" +
                "<td align=\"center\" valign=\"top\">" +
                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">" +
                "<tbody>" +
                "<tr>" +
                "<td style=\"padding-top: 20px; padding-bottom: 20px; padding-right: 0px;\" align=\"right\" valign=\"middle\" class=\"webview\"> <a href=\"#\" style=\"color:#bbb;font-family:Montserrat, sans-serif;font-size:12px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:20px;text-transform:none;text-align:right;text-decoration:underline;padding:0;margin:0\" target=\"_blank\" class=\"text\" hideOnMobile></a>" +
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
                "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableCard\" style=\"background-color:#fff;border-color:#e5e5e5;border-style:solid;border-width:0 1px 1px 1px;\">" +
                "<tbody>" +
                "<tr>" +
                "<td style = \"background-color:#2f40f6;font-size:1px;line-height:3px\" class=\"topBorder\" height=\"3\">&nbsp;</td>" +
                "</tr>" +
                "<tr>" +
                "<td style = \"padding-top: 60px; padding-bottom: 20px;\" align=\"center\" valign=\"middle\" class=\"emailLogo\">" +
                "<a href =\" #\" style=\"text-decoration:none\" target=\"_blank\">" +
                "<img border=\"0\" src='" + res.CompanyImageUrl + "' alt style=\"width:100%;max-width:200px;height:auto;display:block\" width=\"150\">" +
                "</a>" +
                "</td>" +
                "</tr>" +
                "<tr>" +
                "<td style = \"padding-bottom: 5px; padding-left: 20px; padding-right: 20px;\" align=\"center\" valign=\"top\" class=\"mainTitle\">" +
                "<h2 class=\"text\" style=\"color:#000;font-family:'Montserrat', sans-serif;font-size:28px;font-weight:500;font-style:normal;letter-spacing:normal;line-height:36px;text-transform:none;text-align:center;padding:0;margin:0;margin-bottom: 20px\">Welcome to " + res.Name + "</h2>" +
                "</td>" +
                "</tr>" +
                "<tr>" +
                "<td style =\" padding-left:20px;padding-right:20px;\" align=\"center\" valign=\"top\" class=\"containtTable ui-sortable\">" +
                "<table border =\" 0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableDescription\" style=\"margin-top: 40px\">" +
                "<tbody>" +
                "<tr>" +
                "<td style = \"padding-bottom: 15px;\" align=\"center\" valign=\"top\" class=\"description\">" +
                "<p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:14px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\">Congratulations! Your agency has been activated.</p>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "<table border =\" 0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableButton\">" +
                "<tbody>" +
                "<tr>" +
                "<td style = \"padding-top:20px;padding-bottom:20px\" align=\"center\" valign=\"top\">" +
                "<table border = \"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\">" +
                "<tbody>" +
                "<tr>" +
                "</tr>" +
                "<tr>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "<table border = \"0\" cellpadding=\" 0\" cellspacing=\" 0\" width= \"100%\" class=\"tableButton\">" +
                "<tbody>" +
                "<tr>" +
                "<td style = \"padding-top:20px;padding-bottom:20px\" align=\"center\" valign=\"top\">" +
                "<table border = \"0\" cellpadding=\"0\" cellspacing=\"0\" align=\"center\">" +
                "<tbody>" +
                "<tr>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "<table border = \"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"tableDescription\">" +
                "<tbody>" +
                "</tbody>" +
                "<tbody>" +
                "<tr>" +
                "<td style = \"padding-bottom: 20px;\" align= \"center\" valign=\" top\" class=\"description\">" +
                "<p class=\"text\" style=\"color:#666;font-family:'Montserrat', sans-serif;font-size:14px;font-weight:400;font-style:normal;letter-spacing:normal;line-height:22px;text-transform:none;text-align:center;padding:0;margin:0\">If you didn't request this email, there's nothing to worry about.you can safely ignore it. </p>" +
                "</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "</td>" +
                "</tr>" +
                "<tr>" +
                "<td style = \"font-size:1px;line-height:1px height=20\">&nbsp;</td>" +
                "</tr>" +
                "</tbody>" +
                "</table>" +
                "<table border = \"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" class=\"space\">" +
                "<tbody>" +
                "<tr>" +
                "<td style = \"font-size:1px;line-height:1px\" height=\"30\">&nbsp;</td>" +
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

            var subject = "Notification: " + res.Name + " has been activated your agency request";
            if (companyToReturn > 0)
            {
                var client = new SendGridClient(_configuration.GetSection("Client").Value);
                var msg = MailHelper.CreateSingleEmail(new EmailAddress(FromWhom), new EmailAddress(user.Email), subject, "", msgContent);
                var response = client.SendEmailAsync(msg);
            }

            return null;
        }

        public async Task<CompanyDto> UpdateCompanyRowGuid(Guid id, Guid rowGuId)
        {
            var companyToReturn = await _companyRepository.UpdateBulkEntityAsync(y => new Company { RowGuid = rowGuId }, x => x.Id == id);
            return null;
        }
        #endregion


        #region OVERRIDDEN IMPLEMENTATION

        public override Dictionary<string, PropertyMappingValue> GetPropertyMapping()
        {
            return new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Id", new PropertyMappingValue(new List<string>() { "Id" } ) },
                        { "Name", new PropertyMappingValue(new List<string>() { "Name" } ) },
                        { "Website", new PropertyMappingValue(new List<string>() { "Website" } ) },
                        { "Phone", new PropertyMappingValue(new List<string>() { "Phone" } ) },
                        { "Timezone", new PropertyMappingValue(new List<string>() { "Timezone" } ) },
                        { "Address", new PropertyMappingValue(new List<string>() { "Address" } ) },
                        { "ZipCode", new PropertyMappingValue(new List<string>() { "ZipCode" } ) },
                        { "City", new PropertyMappingValue(new List<string>() { "City" } ) },
                        { "State", new PropertyMappingValue(new List<string>() { "State" } ) },
                        { "Country", new PropertyMappingValue(new List<string>() { "Country" } ) },
                        { "Description", new PropertyMappingValue(new List<string>() { "Description" } ) },
                        { "Branding", new PropertyMappingValue(new List<string>() { "Branding" } ) },
                        { "IsAllowMarketPlace", new PropertyMappingValue(new List<string>() { "IsAllowMarketPlace" } ) }
                    };
        }

        public override string GetDefaultOrderByColumn()
        {
            return "UpdatedOn";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "Id,Name,Website,Phone,Timezone,Address,ZipCode,City,State,Country,Description,Branding,VatNumber,Theme,IsAllowMarketPlace";
        }

        #endregion       
    }
}
