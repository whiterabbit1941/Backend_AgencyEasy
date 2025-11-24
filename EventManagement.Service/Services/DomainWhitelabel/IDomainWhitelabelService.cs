using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using Amazon.CloudFront.Model;
using Amazon.CertificateManager.Model;
using Amazon.Amplify.Model;

namespace EventManagement.Service
{
    public interface IDomainWhitelabelService : IService<DomainWhitelabel, Guid>
    {
        Task<RequestCertificateResponse> CreateCertificate(string domainName);
        Task<DescribeCertificateResponse> GetCertificate(string certificateARN);
        Task<CreateDistributionWithTagsResponse> CreateDistributionRequestFromExisiting(string domainName, string comment);
        Task<CreateDistributionWithTagsResponse> CreateCloudDistributionS3Origin(string domainName, string comment);
        Task<UpdateDistributionResponse> UpdateCloudfrontDistribution(string domainName, string certificateARN, string distributionId);
        Company GetCompanyDetailsByDomain(string domain);
        Task<DomainWhitelabelDto> CreateCustomDomain(Guid companyId, string customDomain);
        Task<DomainWhitelabel> GetDomainDns(Guid companyId,string customDomain);
        Task<int> DeleteDomainAssociation(Guid id);
        Task<int> DeleteDomainAssociationByCompanyId(Guid id);

    }
}
