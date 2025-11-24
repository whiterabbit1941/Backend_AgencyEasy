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
using Amazon.CloudFront.Model;
using Amazon.CloudFront;
using Amazon.CertificateManager.Model;
using Tag = Amazon.CloudFront.Model.Tag;
using Amazon.CertificateManager;
using Amazon.Amplify;
using Amazon.Runtime.Internal;
using Amazon.Amplify.Model;
using System.ComponentModel.Design;

namespace EventManagement.Service
{
    public class DomainWhitelabelService : ServiceBase<DomainWhitelabel, Guid>, IDomainWhitelabelService
    {

        #region PRIVATE MEMBERS

        private readonly IDomainWhitelabelRepository _domainwhitelabelRepository;
        private readonly IConfiguration _configuration;
        static AmazonCloudFrontClient cfClient = null;
        static string accessKeyID = null;
        static string secretAccessKeyID = null;
        static string existingDistributionId = null;
        static string amplifyAppId = null;
        static string branchName = null;


        #endregion


        #region CONSTRUCTOR

        public DomainWhitelabelService(IDomainWhitelabelRepository domainwhitelabelRepository,
            ILogger<DomainWhitelabelService> logger,
            IConfiguration configuration) : base(domainwhitelabelRepository, logger)
        {
            _domainwhitelabelRepository = domainwhitelabelRepository;
            _configuration = configuration;
            accessKeyID = _configuration.GetSection("AWS:AccessKeyID").Value;
            secretAccessKeyID = _configuration.GetSection("AWS:SecretAccessKeyID").Value;
            existingDistributionId = _configuration.GetSection("AWS:DistributionId").Value;
            // amplifyAppId = _configuration.GetSection("AWS:AmplifyAppId").Value;
            branchName = _configuration.GetSection("AWS:BranchName").Value;
        }

        #endregion


        #region PUBLIC MEMBERS   

        /// <summary>
        /// Create a Certificate in AWS
        /// </summary>
        public async Task<RequestCertificateResponse> CreateCertificate(string domainName)
        {
            var basicAWSCredentials = new Amazon.Runtime.BasicAWSCredentials(accessKeyID, secretAccessKeyID);

            var certManagerClient = new Amazon.CertificateManager.AmazonCertificateManagerClient(basicAWSCredentials, Amazon.RegionEndpoint.USEast1);

            var responseCert = await certManagerClient.RequestCertificateAsync(new RequestCertificateRequest
            {
                DomainName = domainName,
                ValidationMethod = Amazon.CertificateManager.ValidationMethod.DNS,

            });
            //var responseCert = requestedCer.Result;

            Console.WriteLine(responseCert.CertificateArn);
            Console.WriteLine(responseCert.ContentLength);
            Console.WriteLine(responseCert.HttpStatusCode);
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(responseCert.ResponseMetadata));

            return responseCert;
        }

        public async Task<DomainWhitelabelDto> CreateCustomDomain(Guid companyId, string customDomain)
        {           
                var basicAWSCredentials = new Amazon.Runtime.BasicAWSCredentials(accessKeyID, secretAccessKeyID);

                var amplifyClient = new Amazon.Amplify.AmazonAmplifyClient(basicAWSCredentials, Amazon.RegionEndpoint.USEast1);

                var amplif_app_id = await GetValidLimitAplifyAppId();

                if (!string.IsNullOrEmpty(amplif_app_id))
                {
                    var responseCert = await amplifyClient.CreateDomainAssociationAsync(new Amazon.Amplify.Model.CreateDomainAssociationRequest
                    {
                        DomainName = customDomain,
                        AppId = amplif_app_id,
                        SubDomainSettings = new List<Amazon.Amplify.Model.SubDomainSetting>
                        {
                            new Amazon.Amplify.Model.SubDomainSetting
                            {
                                BranchName = branchName,
                                Prefix = ""
                            }
                        }
                    });

                    if (responseCert.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        // create entity
                        DomainWhitelabelForCreation entity = new DomainWhitelabelForCreation();
                        entity.CompanyID = companyId;

                        // set value
                        entity.CnameType = "CNAME";
                        entity.Status = responseCert.DomainAssociation.DomainStatus;
                        entity.AlternateDomainName = customDomain;
                        entity.AmplifyAppId = amplif_app_id;

                        //create a show in db.
                        return await CreateEntityAsync<DomainWhitelabelDto, DomainWhitelabelForCreation>(entity);
                    }
                    else
                    {
                        return new DomainWhitelabelDto();
                    }
                }
                else
                {
                    return new DomainWhitelabelDto();
                }
            }
            
        public async Task<string> GetValidLimitAplifyAppId()
        {
            var basicAWSCredentials = new Amazon.Runtime.BasicAWSCredentials(accessKeyID, secretAccessKeyID);

            var amplifyClient = new Amazon.Amplify.AmazonAmplifyClient(basicAWSCredentials, Amazon.RegionEndpoint.USEast1);

            var listOfAmplifyAppId = new List<string> { "d2sjwlavncc14i", "d1bozb0nsvkk1", "d38qe980f7dkt6", "d33z81dsm6hd2y", "dyl41z27w254h", "dy1izezj3b9jx", "d1qg5x1ykujk2g", "d1es71nktdjokz", "d19hfz4lgdo9zj" };

            foreach (var appid in listOfAmplifyAppId)
            {
                var domainList = await amplifyClient.ListDomainAssociationsAsync(new Amazon.Amplify.Model.ListDomainAssociationsRequest
                {
                    AppId = appid,
                    MaxResults = 42
                });

                if (domainList.DomainAssociations.Count() < 39)
                {
                    return appid;
                }
            }

            return "";
        }

        public async Task<DomainWhitelabel> GetDomainDns(Guid companyId, string customDomain)
        {
            var domainWhitelableEntity = GetAllEntities().Where(x => x.CompanyID == companyId).FirstOrDefault();

            if (domainWhitelableEntity != null && domainWhitelableEntity.Status != "AVAILABLE")
            {
                var basicAWSCredentials = new Amazon.Runtime.BasicAWSCredentials(accessKeyID, secretAccessKeyID);

                var amplifyClient = new Amazon.Amplify.AmazonAmplifyClient(basicAWSCredentials, Amazon.RegionEndpoint.USEast1);

                var req = new GetDomainAssociationRequest
                {
                    AppId = domainWhitelableEntity.AmplifyAppId,
                    DomainName = customDomain
                };

                var responseCert = await amplifyClient.GetDomainAssociationAsync(req);

                if (responseCert.HttpStatusCode == System.Net.HttpStatusCode.OK &&
                    responseCert.DomainAssociation.CertificateVerificationDNSRecord != null)
                {
                    var entity = new DomainWhitelabel();

                    var cnameValue = responseCert.DomainAssociation.CertificateVerificationDNSRecord.Split(" CNAME ");
                    var cloudFrontUrl = responseCert.DomainAssociation.SubDomains[0].DnsRecord.Split("CNAME ");

                    domainWhitelableEntity.CnameHost = cnameValue[0];
                    domainWhitelableEntity.CnamePointsTo = cnameValue[1];
                    domainWhitelableEntity.Status = responseCert.DomainAssociation.DomainStatus;
                    domainWhitelableEntity.DomainName = cloudFrontUrl[1];

                    UpdateEntity(domainWhitelableEntity);
                    await SaveChangesAsync();

                    //await UpdateEntityAsync(domainWhitelableEntity.Id, domainWhitelableEntity);
                }
            }

            return domainWhitelableEntity;
        }


        public async Task<int> DeleteDomainAssociation(Guid id)
        {
            var entity = GetAllEntities().Where(x => x.Id == id).FirstOrDefault();

            if (entity != null)
            {
                var basicAWSCredentials = new Amazon.Runtime.BasicAWSCredentials(accessKeyID, secretAccessKeyID);

                var amplifyClient = new Amazon.Amplify.AmazonAmplifyClient(basicAWSCredentials, Amazon.RegionEndpoint.USEast1);

                var req = new DeleteDomainAssociationRequest
                {
                    AppId = entity.AmplifyAppId,
                    DomainName = entity.AlternateDomainName
                };
                var responseCert = await amplifyClient.DeleteDomainAssociationAsync(req);

                if (responseCert.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    //delete the domainwhitelabel from the db.
                    return await DeleteEntityAsync(id);
                }
                else
                {
                    return 0;
                }

            }
            else { return 0; }
        }

        public async Task<int> DeleteDomainAssociationByCompanyId(Guid companyId)
        {
            var entity = GetAllEntities().Where(x => x.CompanyID == companyId).FirstOrDefault();

            if (entity != null)
            {
                var basicAWSCredentials = new Amazon.Runtime.BasicAWSCredentials(accessKeyID, secretAccessKeyID);

                var amplifyClient = new Amazon.Amplify.AmazonAmplifyClient(basicAWSCredentials, Amazon.RegionEndpoint.USEast1);

                var req = new DeleteDomainAssociationRequest
                {
                    AppId = entity.AmplifyAppId,
                    DomainName = entity.AlternateDomainName
                };
                var responseCert = await amplifyClient.DeleteDomainAssociationAsync(req);

                if (responseCert.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    //delete the domainwhitelabel from the db.
                    return await DeleteEntityAsync(entity.Id);
                }
                else
                {
                    return 0;
                }

            }
            else { return 0; }
        }

        /// <summary>
        /// Get Company Details By Domain
        /// </summary>
        /// <param name="domain">domain</param>
        /// <returns>Company</returns>
        public Company GetCompanyDetailsByDomain(string domain)
        {
            var domainWhitelabel = _domainwhitelabelRepository.GetAllEntities(true).Where(x => domain.Contains(x.AlternateDomainName)).Select(x => new DomainWhitelabel
            {
                Company = x.Company

            }).FirstOrDefault();

            if (domainWhitelabel != null)
            {
                return domainWhitelabel.Company;
            }
            else
            {
                return new Company { CompanyImageUrl = "https://identity.agencyeasy.com/agencyLogo.png", Name = "Agencyeasy" };
            }
            
        }

        /// <summary>
        /// User the ARN from created Certificate and get the Certificate so as to get the CNAME to be set at the client end.
        /// </summary>
        public async Task<DescribeCertificateResponse> GetCertificate(string certificateARN)
        {
            var basicAWSCredentials = new Amazon.Runtime.BasicAWSCredentials(accessKeyID, secretAccessKeyID);

            var certManagerClient = new Amazon.CertificateManager.AmazonCertificateManagerClient(basicAWSCredentials, Amazon.RegionEndpoint.USEast1);

            var responseCert = await certManagerClient.DescribeCertificateAsync(certificateARN);
            //var responseCert = requestedCer.Result;

            Console.WriteLine(responseCert.Certificate.Status);
            if (responseCert.Certificate.Status == CertificateStatus.ISSUED)
            {
                Console.WriteLine(responseCert.Certificate);
                //Console.WriteLine(responseCert.CertificateChain);
                Console.WriteLine(responseCert.ContentLength);
                Console.WriteLine(responseCert.HttpStatusCode);
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(responseCert.ResponseMetadata));
            }

            return responseCert;
        }

        /// <summary>
        /// Create a New Distribution From Exisiting Distribution
        /// </summary>
        public async Task<CreateDistributionWithTagsResponse> CreateDistributionRequestFromExisiting(string domainName, string comment)
        {
            var response = new CreateDistributionWithTagsResponse();
            {
                try
                {
                    var basicAWSCredentials = new Amazon.Runtime.BasicAWSCredentials(accessKeyID, secretAccessKeyID);

                    using (cfClient = new AmazonCloudFrontClient(basicAWSCredentials, Amazon.RegionEndpoint.USEast1))
                    {
                        var currentTime = DateTime.UtcNow.ToString();
                        CreateDistributionWithTagsRequest req = new CreateDistributionWithTagsRequest();
                        var distConfig = await cfClient.GetDistributionConfigAsync(new GetDistributionConfigRequest
                        {
                            Id = existingDistributionId
                        });

                        List<Origin> origins = new List<Origin>();
                        foreach (var item in distConfig.DistributionConfig.Origins.Items)
                        {
                            string origin = "whitelabelboard-test.s3.us-east-1.amazonaws.com";
                            if (_configuration.GetSection("Env").Value == "Live")
                            {
                                origin = "whitelabelboard.s3.us-east-2.amazonaws.com";
                            }
                            if (item.DomainName == origin)
                            {
                                item.Id = "whitelabelboard_" + currentTime;
                                origins.Add(item);
                            }
                        }

                        distConfig.DistributionConfig.Origins.Items = null;
                        distConfig.DistributionConfig.Origins.Items = origins;
                        distConfig.DistributionConfig.Origins.Quantity = 1;
                        distConfig.DistributionConfig.Comment = comment;
                        distConfig.DistributionConfig.Aliases = null;
                        distConfig.DistributionConfig.ViewerCertificate = null;
                        distConfig.DistributionConfig.DefaultCacheBehavior.TargetOriginId = "whitelabelboard_" + currentTime;

                        distConfig.DistributionConfig.CallerReference = DateTime.Now.ToString();

                        req.DistributionConfigWithTags = new DistributionConfigWithTags
                        {
                            DistributionConfig = distConfig.DistributionConfig,
                            Tags = new Tags { Items = new List<Tag> { new Tag { Key = "purpose", Value = domainName + currentTime } } }
                        };

                        response = await cfClient.CreateDistributionWithTagsAsync(req);
                    }
                }
                catch (AmazonCloudFrontException cfEx)
                {
                    Console.WriteLine("An Error, number {0}, occurred when create cloud distribution with the message '{1}", cfEx.ErrorCode, cfEx.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("UnknownError:{0}", ex.Message);
                }

                return response;
            }
        }

        /// <summary>
        /// Create a Cloud Distribution. [ Need to store the ID]
        /// </summary>
        public async Task<CreateDistributionWithTagsResponse> CreateCloudDistributionS3Origin(string domainName, string comment)
        {
            var response = new CreateDistributionWithTagsResponse();
            try
            {
                var basicAWSCredentials = new Amazon.Runtime.BasicAWSCredentials(accessKeyID, secretAccessKeyID);

                using (cfClient = new AmazonCloudFrontClient(basicAWSCredentials, Amazon.RegionEndpoint.USEast1))
                {
                    var currentTime = DateTime.UtcNow.ToString();
                    CreateDistributionWithTagsRequest req = new CreateDistributionWithTagsRequest();

                    Origins origins = new Origins();
                    origins.Quantity = 1;
                    origins.Items = new List<Origin>();

                    Origin origin = new Origin();
                    origin.DomainName = domainName; // Check 
                    origin.Id = "whitelabelboard_" + currentTime;
                    origin.S3OriginConfig = new S3OriginConfig { OriginAccessIdentity = "" };

                    origins.Items.Add(origin);

                    DefaultCacheBehavior defaultCacheBehaviour = new DefaultCacheBehavior();

                    defaultCacheBehaviour.ForwardedValues = new ForwardedValues
                    {
                        Cookies = new CookiePreference { Forward = new ItemSelection("none") },
                        QueryString = false
                    };

                    defaultCacheBehaviour.MinTTL = 0;
                    defaultCacheBehaviour.TargetOriginId = "whitelabelboard_" + currentTime;
                    defaultCacheBehaviour.TrustedSigners = new TrustedSigners
                    {
                        Quantity = 0,
                        Enabled = false
                    };
                    defaultCacheBehaviour.ViewerProtocolPolicy = new ViewerProtocolPolicy("redirect-to-https");
                    defaultCacheBehaviour.AllowedMethods = new AllowedMethods
                    {
                        Quantity = 7,
                        Items = new List<string> { "GET", "HEAD", "OPTIONS", "PUT", "POST", "PATCH", "DELETE" },
                        CachedMethods = new CachedMethods
                        {
                            Quantity = 2,
                            Items = new List<string> { "GET", "HEAD" }
                        }
                    };

                    var DistributionConfig = new DistributionConfig
                    {
                        CallerReference = DateTime.Now.ToString(),
                        Comment = comment,
                        DefaultCacheBehavior = defaultCacheBehaviour,
                        Origins = origins,
                        PriceClass = "PriceClass_100",
                        Enabled = true,
                        DefaultRootObject = "index.html",
                        //Aliases = new Aliases
                        //{
                        //    Quantity = 1,
                        //    Items = new List<string> { customDomain }
                        //},
                        CustomErrorResponses = new CustomErrorResponses
                        {
                            Quantity = 2,
                            Items = new List<CustomErrorResponse>
                        {
                            new CustomErrorResponse{ErrorCode = 403, ResponsePagePath = "/index.html", ErrorCachingMinTTL = 300, ResponseCode = "200"},
                            new CustomErrorResponse{ErrorCode = 400, ResponsePagePath = "/index.html", ErrorCachingMinTTL = 300, ResponseCode = "200"}
                        }
                        },
                        HttpVersion = HttpVersion.Http2,
                        //ViewerCertificate = new ViewerCertificate { 
                        //   ACMCertificateArn = certificateARN ,
                        //   CloudFrontDefaultCertificate = false,
                        //   SSLSupportMethod = SSLSupportMethod.SniOnly

                        //}
                    };

                    req.DistributionConfigWithTags = new DistributionConfigWithTags
                    {
                        DistributionConfig = DistributionConfig,
                        Tags = new Tags { Items = new List<Tag> { new Tag { Key = "purpose", Value = domainName } } }
                    };

                    response = await cfClient.CreateDistributionWithTagsAsync(req);
                }
            }
            catch (AmazonCloudFrontException cfEx)
            {
                Console.WriteLine("An Error, number {0}, occurred when create cloud distribution with the message '{1}", cfEx.ErrorCode, cfEx.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("UnknownError:{0}", ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Once the client has set the CNAME record in his custom domain and added a redirection to the cloud front url
        /// Then we need to update the current cloudfront distribution with the above created certificate and the custom domain. 
        /// </summary>
        public async Task<UpdateDistributionResponse> UpdateCloudfrontDistribution(string domainName, string certificateARN, string distributionId)
        {
            var response = new UpdateDistributionResponse();
            try
            {
                var basicAWSCredentials = new Amazon.Runtime.BasicAWSCredentials(accessKeyID, secretAccessKeyID);

                using (cfClient = new AmazonCloudFrontClient(basicAWSCredentials, Amazon.RegionEndpoint.USEast1))
                {
                    var distConfig = await cfClient.GetDistributionConfigAsync(new GetDistributionConfigRequest
                    {
                        Id = distributionId
                    });

                    distConfig.DistributionConfig.Aliases = new Aliases
                    {
                        Quantity = 1,
                        Items = new List<string> { domainName }
                    };
                    distConfig.DistributionConfig.ViewerCertificate = new ViewerCertificate
                    {
                        ACMCertificateArn = certificateARN,
                        CloudFrontDefaultCertificate = false,
                        SSLSupportMethod = SSLSupportMethod.SniOnly,
                        MinimumProtocolVersion = MinimumProtocolVersion.TLSv12_2021

                    };

                    response = await cfClient.UpdateDistributionAsync(new UpdateDistributionRequest
                    {
                        DistributionConfig = distConfig.DistributionConfig,
                        Id = distributionId,
                        IfMatch = distConfig.ETag
                    });

                    Console.WriteLine(response.HttpStatusCode);
                }
            }
            catch (AmazonCloudFrontException cfEx)
            {
                Console.WriteLine("An Error, number {0}, occurred when create cloud distribution with the message '{1}", cfEx.ErrorCode, cfEx.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("UnknownError:{0}", ex.Message);
            }

            return response;
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
            return "Id,CompanyID,DomainID,DomainName,AlternateDomainName,Origin,CnameHost,CnameType,CnamePointsTo,CertificateARN,DistributionId,Status,Certificate";
        }

        #endregion
    }
}
