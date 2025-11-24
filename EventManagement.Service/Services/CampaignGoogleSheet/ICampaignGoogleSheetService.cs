using System;
using System.Threading.Tasks;
using EventManagement.Dto;
using EventManagement.Domain.Entities;
using RestSharp;
using System.Collections.Generic;

namespace EventManagement.Service
{
    public interface ICampaignGoogleSheetService : IService<CampaignGoogleSheet, Guid>
    {
        Task<GoogleAccountDto> GetGoogleAccountDetails(Guid campaignId);
        Task<List<DriveFile>> GetListSpreadSheet(Guid campaignId);
        Task<List<SheetProperties>> GetListSheets(Guid campaignId, string spreadSheetId);
        Task<List<GoogleSheetData>> GetGoogleSheetReport(List<GoogleSheetSettingsDto> googleSheetSettingsDto);
    }
}
