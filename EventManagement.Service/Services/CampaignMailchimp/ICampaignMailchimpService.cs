using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;

namespace EventManagement.Service
{
    public interface ICampaignMailchimpService : IService<CampaignMailchimp, Guid>
    {
        string MailchimpAuth();

        Task<string> GetAccessTokenUsingCode(string code);

        Task<MailchimpMetadataDto> GetMailchimpAccount(string access_token);

        Task<MCRootCampaignList> GetCampaignListReport(Guid campaignId);

        Task<MCCampaignList> GetCampaignList(Guid campaignId);

        Task<MCRootList> GetListReport(Guid campaignId);

        Task<SingleCampaignReport> GetSingleCampaignReport(Guid campaignId,string mcCampaignId);

        Task<RootSingleList> GetSingleListReport(Guid campaignId, string id);

        Task<CampaignTableRoot> GetCampaignTable(Guid campaignId, string mcCampaignId, int offset, int count);

        Task<MailChimpMemberRoot> GetMemberOfListApi(Guid campaignId, string listId, int offset, int count, string status);

        Task<McListRoot> GetMcList(Guid campaignId);

        Task<CampaignTableRoot> GetCampaignTable(Guid campaignId, string mcCampaignId);

        Task<MailChimpMemberRoot> GetMemberOfListApi(Guid campaignId, string listId);

        Task<ClickDetailsDto> GetTopLinksByCampaign(Guid campaignId, string mcCampaignId);
    }
}
