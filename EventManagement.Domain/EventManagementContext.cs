using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventManagement.Domain.Entities;
using FinanaceManagement.API.Models;

namespace EventManagement.Domain
{
    public class EventManagementContext : DbContext
    {

        #region PUBLIC MEMBERS

       public DbSet<CampaignMailchimp> CampaignMailchimps { get; set; }
       public DbSet<CampaignMicrosoftAd> CampaignMicrosoftAds { get; set; }
       public DbSet<CampaignCallRail> CampaignCallRails { get; set; }
       public DbSet<CampaignGoogleSheet> CampaignGoogleSheets { get; set; }
       public DbSet<CancellationReason> CancellationReasons { get; set; }
       public DbSet<StripeCoupon> StripeCoupons { get; set; }
       public DbSet<CampaignWooCommerce> CampaignWooCommerces { get; set; }
       public DbSet<CampaignGBP> CampaignGBPs { get; set; }
       public DbSet<LinkedinAd> LinkedinAds { get; set; }
        public DbSet<AppsumoPlan> AppsumoPlans { get; set; }
        public DbSet<TemplateSetting> TemplateSettings { get; set; }
        public DbSet<DomainWhitelabel> DomainWhitelabels { get; set; }
        public DbSet<CompanyPlan> CompanyPlans { get; set; }
        public DbSet<PlanDetail> PlanDetails { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<DefaultPlan> DefaultPlans { get; set; }
        public DbSet<EmailWhitelabel> EmailWhitelabels { get; set; }
        public DbSet<ReportScheduling> ReportSchedulings { get; set; }
        public DbSet<ReportSetting> ReportSettings { get; set; }
        public DbSet<CampaignLinkedin> CampaignLinkedins { get; set; }
        public DbSet<CampaignFacebookAds> CampaignFacebookAdss { get; set; }
        public DbSet<CampaignInstagram> CampaignInstagrams { get; set; }
        public DbSet<CompanyUser> CompanyUsers { get; set; }
        public DbSet<CampaignUser> CampaignUsers { get; set; }
        public DbSet<CampaignGoogleAds> CampaignGoogleAdss { get; set; }
        public DbSet<CampaignFacebook> CampaignFacebooks { get; set; }
        public DbSet<CampaignGSC> CampaignGSCs { get; set; }
        public DbSet<CampaignGoogleAnalytics> CampaignGoogleAnalyticss { get; set; }
        public DbSet<GoogleAdsSummary> GoogleAdsSummarys { get; set; }
        public DbSet<SocialMediaSUmmmary> SocialMediaSUmmmarys { get; set; }
        public DbSet<GscSummary> GscSummarys { get; set; }
        public DbSet<TrafficSummary> TrafficSummarys { get; set; }
        public DbSet<RankingGraph> RankingGraphs { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<StripePayment> StripePayments { get; set; }
        public DbSet<WhiteLabel> WhiteLabels { get; set; }
        public DbSet<EmailSetting> EmailSettings { get; set; }
        public DbSet<Company> Companys { get; set; }
        public DbSet<Serp> Serps { get; set; }
        public DbSet<Audits> Auditss { get; set; }
        public DbSet<GoogleAnalyticsAccount> GoogleAnalyticsAccounts { get; set; }
        public DbSet<GoogleAccountSetup> GoogleAccountSetups { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<Event> Events { get; set; }
        public virtual DbSet<ApiClaims> ApiClaims { get; set; }
        public virtual DbSet<ApiProperties> ApiProperties { get; set; }
        public virtual DbSet<ApiResources> ApiResources { get; set; }
        public virtual DbSet<ApiScopeClaims> ApiScopeClaims { get; set; }
        public virtual DbSet<ApiScopes> ApiScopes { get; set; }
        public virtual DbSet<ApiSecrets> ApiSecrets { get; set; }
        public virtual DbSet<AspNetRoleClaims> AspNetRoleClaims { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<AspNetUserTokens> AspNetUserTokens { get; set; }
        public virtual DbSet<ClientClaims> ClientClaims { get; set; }
        public virtual DbSet<ClientCorsOrigins> ClientCorsOrigins { get; set; }
        public virtual DbSet<ClientGrantTypes> ClientGrantTypes { get; set; }
        public virtual DbSet<ClientIdPrestrictions> ClientIdPrestrictions { get; set; }
        public virtual DbSet<ClientPostLogoutRedirectUris> ClientPostLogoutRedirectUris { get; set; }
        public virtual DbSet<ClientProperties> ClientProperties { get; set; }
        public virtual DbSet<ClientRedirectUris> ClientRedirectUris { get; set; }
        public virtual DbSet<Clients> Clients { get; set; }
        public virtual DbSet<ClientScopes> ClientScopes { get; set; }
        public virtual DbSet<ClientSecrets> ClientSecrets { get; set; }
        public virtual DbSet<DeviceCodes> DeviceCodes { get; set; }
        public virtual DbSet<IdentityClaims> IdentityClaims { get; set; }
        public virtual DbSet<IdentityProperties> IdentityProperties { get; set; }
        public virtual DbSet<IdentityResources> IdentityResources { get; set; }
        public virtual DbSet<PersistedGrants> PersistedGrants { get; set; }

        private readonly IUserInfoService _userInfoService;

        #endregion

        #region CONSTRUCTOR

        public EventManagementContext(DbContextOptions<EventManagementContext> options, IUserInfoService userInfoService)
         : base(options)
        {
            // userInfoService is a required argument
            _userInfoService = userInfoService ?? throw new ArgumentNullException(nameof(userInfoService));

        }

        #endregion
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppsumoPlan>().HasData(new AppsumoPlan[]
           {
                new AppsumoPlan
                {
                    Id = new Guid("934b6e66-337e-4944-9c6a-788a194f3f0b"),
                    Name = "License Tier 1",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2023,05,12),
                    Cost = 59,
                    MaxProjects = 3,
                    MaxTeamUsers = 0,
                    MaxClientUsers = 0,
                    MaxKeywordsPerProject = 50,
                    AppsumoPlanId="agencyeasy_tier1",
                    WhitelabelSupport=false,
                },
                new AppsumoPlan
                {
                    Id = new Guid("9f91c98c-207c-41e6-bb44-1ac37d8c6e53"),
                    Name = "License Tier 2",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2023,05,12),
                    Cost = 119,
                    MaxProjects = 6,
                    MaxTeamUsers = 0,
                    MaxClientUsers = 0,
                    MaxKeywordsPerProject = 100,
                    AppsumoPlanId="agencyeasy_tier2",
                    WhitelabelSupport=true,
                },
                new AppsumoPlan
                {
                    Id = new Guid("a2b83140-f803-45ca-9209-35fc1888175f"),
                    Name = "License Tier 3",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2023,05,12),
                    Cost = 179,
                    MaxProjects = 12,
                    MaxTeamUsers = 0,
                    MaxClientUsers = 0,
                    MaxKeywordsPerProject = 150,
                    AppsumoPlanId="agencyeasy_tier3",
                    WhitelabelSupport=true,
                },
                new AppsumoPlan
                {
                    Id = new Guid("1f5b1d53-8619-494d-821d-fa98ac9d8c11"),
                    Name = "License Tier 4",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2023,05,12),
                    Cost = 239,
                    MaxProjects = 24,
                    MaxTeamUsers = 0,
                    MaxClientUsers = 0,
                    MaxKeywordsPerProject = 200,
                    AppsumoPlanId="agencyeasy_tier4",
                    WhitelabelSupport=true,
                },
                new AppsumoPlan
                {
                    Id = new Guid("b6ddf793-e87f-4b2d-9822-92b8226d8301"),
                    Name = "License Tier 5",
                    CreatedBy = "Migration",
                    CreatedOn =  new DateTime(2023,05,12),
                    Cost = 299,
                    MaxProjects = 100,
                    MaxTeamUsers = 0,
                    MaxClientUsers = 0,
                    MaxKeywordsPerProject = 0,
                    AppsumoPlanId="agencyeasy_tier5",
                    WhitelabelSupport=true,
                }
           });

            modelBuilder.Entity<DefaultPlan>().HasData(new DefaultPlan[]
            {
                new DefaultPlan
                {
                    Id = new Guid("ba57022f-eb81-4eb5-b590-ef6d418b1db9"),
                    Name = "FREE",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    Cost = 0,
                    MaxProjects = 0,
                    MaxTeamUsers = 0,
                    MaxClientUsers = 0,
                    MaxKeywordsPerProject = 0,
                    IsVisible= true
                },
                new DefaultPlan
                {
                    Id = new Guid("b32218c3-427a-4cd0-b2ab-c71455e63951"),
                    Name = "STARTUP",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    Cost = 29,
                    MaxProjects = 3,
                    MaxTeamUsers = 0,
                    MaxClientUsers = 0,
                    MaxKeywordsPerProject = 50,
                    IsVisible= true,
                },
                new DefaultPlan
                {
                    Id = new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"),
                    Name = "AGENCY",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    Cost = 79,
                    MaxProjects = 10,
                    MaxTeamUsers = 0,
                    MaxClientUsers = 0,
                    MaxKeywordsPerProject = 100,
                    IsVisible= true
                },
                new DefaultPlan
                {
                    Id = new Guid("88c06521-d10d-4509-81a2-771c58dbb88d"),
                    Name = "CUSTOM",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    Cost = 86.9M,
                    MaxProjects = 11,
                    MaxTeamUsers = 0,
                    MaxClientUsers = 0,
                    MaxKeywordsPerProject = 100,
                    IsVisible= true
                },
                new DefaultPlan
                {
                    Id = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"),
                    Name = "CUSTOM FREE",
                    CreatedBy = "Migration",
                    CreatedOn =  new DateTime(2022,01,19),
                    Cost = 0,
                    MaxProjects = 0,
                    MaxTeamUsers = 0,
                    MaxClientUsers = 0,
                    MaxKeywordsPerProject = 0,
                    IsVisible= false
                },
                new DefaultPlan
                {
                    Id = new Guid("934b6e66-337e-4944-9c6a-788a194f3f0b"),
                    Name = "agencyeasy_tier1",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2023,05,12),
                    Cost = 59,
                    MaxProjects = 3,
                    MaxTeamUsers = 0,
                    MaxClientUsers = 0,
                    MaxKeywordsPerProject = 50,
                    IsVisible= false,
                },
                new DefaultPlan
                {
                    Id = new Guid("9f91c98c-207c-41e6-bb44-1ac37d8c6e53"),
                    Name = "agencyeasy_tier2",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2023,05,12),
                    Cost = 119,
                    MaxProjects = 6,
                    MaxTeamUsers = 0,
                    MaxClientUsers = 0,
                    MaxKeywordsPerProject = 100,
                    IsVisible= false,
                },
                new DefaultPlan
                {
                    Id = new Guid("a2b83140-f803-45ca-9209-35fc1888175f"),
                    Name = "agencyeasy_tier3",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2023,05,12),
                    Cost = 179,
                    MaxProjects = 12,
                    MaxTeamUsers = 0,
                    MaxClientUsers = 0,
                    MaxKeywordsPerProject = 150,
                    IsVisible= false,
                },
                new DefaultPlan
                {
                    Id = new Guid("1f5b1d53-8619-494d-821d-fa98ac9d8c11"),
                    Name = "agencyeasy_tier4",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2023,05,12),
                    Cost = 239,
                    MaxProjects = 24,
                    MaxTeamUsers = 0,
                    MaxClientUsers = 0,
                    MaxKeywordsPerProject = 200,
                    IsVisible= false,
                },
                new DefaultPlan
                {
                    Id = new Guid("b6ddf793-e87f-4b2d-9822-92b8226d8301"),
                    Name = "agencyeasy_tier5",
                    CreatedBy = "Migration",
                    CreatedOn =  new DateTime(2023,05,12),
                    Cost = 299,
                    MaxProjects = 100,
                    MaxTeamUsers = 0,
                    MaxClientUsers = 0,
                    MaxKeywordsPerProject = 0,
                    IsVisible= false,
                }
            });

            modelBuilder.Entity<CompanyPlan>().HasData(new CompanyPlan[]
                {
                new CompanyPlan
                {
                    Id = new Guid("6d5f4ea9-7d37-41dc-985b-054c929d9761"),
                    CompanyId =new Guid("5F5FE5C0-C10A-4DDE-BB82-08D95AFBD55C") ,
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e") ,
                    ExpiredOn = new DateTime(2023,01,19),
                    PaymentProfileId = "customfree",
                    Active=true,
                    UpdatedBy="Migration",
                    CreatedBy = "Migration",
                    MaxClientUsers=0,
                    MaxKeywordsPerProject=0,
                    MaxProjects=0,
                    MaxTeamUsers=0,
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedOn = new DateTime(2022,01,19)

                },
                new CompanyPlan
                {
                    Id = new Guid("e0b4f46e-871d-448f-ad62-24d50bfd7c49"),
                    CompanyId =new Guid("DDD31200-0A7A-4719-D49B-08D96C484D6B") ,
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e") ,
                    ExpiredOn = new DateTime(2023,01,19),
                    PaymentProfileId = "customfree",
                    Active=true,
                    UpdatedBy="Migration",
                    CreatedBy = "Migration",
                    MaxClientUsers=0,
                    MaxKeywordsPerProject=0,
                    MaxProjects=0,
                    MaxTeamUsers=0,
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedOn = new DateTime(2022,01,19)

                },
                new CompanyPlan
                {
                    Id = new Guid("f1bc45d4-1bb0-425f-9aec-b3cc522fa26c"),
                    CompanyId =new Guid("01041FCF-182F-41FD-476A-08D979207A67") ,
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e") ,
                    ExpiredOn = new DateTime(2023,01,19),
                    PaymentProfileId = "customfree",
                    Active=true,
                    UpdatedBy="Migration",
                    CreatedBy = "Migration",
                    MaxClientUsers=0,
                    MaxKeywordsPerProject=0,
                    MaxProjects=0,
                    MaxTeamUsers=0,
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedOn = new DateTime(2022,01,19)

                },
                new CompanyPlan
                {
                    Id = new Guid("0b429d02-8402-4bbd-aa6b-6cb5a4da1e6b"),
                    CompanyId =new Guid("4587A7E9-B546-40B2-476B-08D979207A67") ,
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e") ,
                    ExpiredOn = new DateTime(2023,01,19),
                    PaymentProfileId = "customfree",
                    Active=true,
                    UpdatedBy="Migration",
                    CreatedBy = "Migration",
                    MaxClientUsers=0,
                    MaxKeywordsPerProject=0,
                    MaxProjects=0,
                    MaxTeamUsers=0,
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedOn = new DateTime(2022,01,19)

                },
                new CompanyPlan
                {
                    Id = new Guid("01d42519-ab4a-46e0-9bdb-b0e979df2a5c"),
                    CompanyId =new Guid("9CAE5FEB-F0BE-4FE9-476C-08D979207A67") ,
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e") ,
                    ExpiredOn = new DateTime(2023,01,19),
                    PaymentProfileId = "customfree",
                    Active=true,
                    UpdatedBy="Migration",
                    CreatedBy = "Migration",
                    MaxClientUsers=0,
                    MaxKeywordsPerProject=0,
                    MaxProjects=0,
                    MaxTeamUsers=0,
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedOn = new DateTime(2022,01,19)

                },
                new CompanyPlan
                {
                    Id = new Guid("d477ffa6-1404-40c0-b6f2-a5b0dd0e3c99"),
                    CompanyId =new Guid("081011A2-CB77-4B16-476D-08D979207A67") ,
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e") ,
                    ExpiredOn = new DateTime(2023,01,19),
                    PaymentProfileId = "customfree",
                    Active=true,
                    UpdatedBy="Migration",
                    CreatedBy = "Migration",
                    MaxClientUsers=0,
                    MaxKeywordsPerProject=0,
                    MaxProjects=0,
                    MaxTeamUsers=0,
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedOn = new DateTime(2022,01,19)

                },
                new CompanyPlan
                {
                    Id = new Guid("e3564bd5-b6f2-423c-aebd-d594aa964112"),
                    CompanyId =new Guid("7D4B196C-4564-4A79-476E-08D979207A67") ,
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e") ,
                    ExpiredOn = new DateTime(2023,01,19),
                    PaymentProfileId = "customfree",
                    Active=true,
                    UpdatedBy="Migration",
                    CreatedBy = "Migration",
                    MaxClientUsers=0,
                    MaxKeywordsPerProject=0,
                    MaxProjects=0,
                    MaxTeamUsers=0,
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedOn = new DateTime(2022,01,19)

                },
                new CompanyPlan
                {
                    Id = new Guid("eb4044c5-4f45-4b7f-b5cc-af35abf6eac8"),
                    CompanyId =new Guid("1703C418-74C0-4BB2-476F-08D979207A67") ,
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e") ,
                    ExpiredOn = new DateTime(2023,01,19),
                    PaymentProfileId = "customfree",
                    Active=true,
                    UpdatedBy="Migration",
                    CreatedBy = "Migration",
                    MaxClientUsers=0,
                    MaxKeywordsPerProject=0,
                    MaxProjects=0,
                    MaxTeamUsers=0,
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedOn = new DateTime(2022,01,19)

                },
                new CompanyPlan
                {
                    Id = new Guid("6aac525a-fe27-445a-b87d-ecd561cff29b"),
                    CompanyId =new Guid("29689F6E-E003-4DBF-C131-08D97E7DD203") ,
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e") ,
                    ExpiredOn = new DateTime(2023,01,19),
                    PaymentProfileId = "customfree",
                    Active=true,
                    UpdatedBy="Migration",
                    CreatedBy = "Migration",
                    MaxClientUsers=0,
                    MaxKeywordsPerProject=0,
                    MaxProjects=0,
                    MaxTeamUsers=0,
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedOn = new DateTime(2022,01,19)

                },
                new CompanyPlan
                {
                    Id = new Guid("0f1973f5-a38a-4269-8ba8-6cb09a749d0b"),
                    CompanyId =new Guid("0F43C3E6-4E78-4301-F76A-08D98C837F57") ,
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e") ,
                    ExpiredOn = new DateTime(2023,01,19),
                    PaymentProfileId = "customfree",
                    Active=true,
                    UpdatedBy="Migration",
                    CreatedBy = "Migration",
                    MaxClientUsers=0,
                    MaxKeywordsPerProject=0,
                    MaxProjects=0,
                    MaxTeamUsers=0,
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedOn = new DateTime(2022,01,19)

                },
                new CompanyPlan
                {
                    Id = new Guid("66349c8d-df5b-4154-be15-10575417f761"),
                    CompanyId =new Guid("26F8E007-677A-415E-F76B-08D98C837F57") ,
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e") ,
                    ExpiredOn = new DateTime(2023,01,19),
                    PaymentProfileId = "customfree",
                    Active=true,
                    UpdatedBy="Migration",
                    CreatedBy = "Migration",
                    MaxClientUsers=0,
                    MaxKeywordsPerProject=0,
                    MaxProjects=0,
                    MaxTeamUsers=0,
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedOn = new DateTime(2022,01,19)

                },
                new CompanyPlan
                {
                    Id = new Guid("b0fd86bc-a818-4b06-a597-ddeabe3c820f"),
                    CompanyId =new Guid("479F00DF-DBB7-47E5-A205-08D9AA8CAE87") ,
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e") ,
                    ExpiredOn = new DateTime(2023,01,19),
                    PaymentProfileId = "customfree",
                    Active=true,
                    UpdatedBy="Migration",
                    CreatedBy = "Migration",
                    MaxClientUsers=0,
                    MaxKeywordsPerProject=0,
                    MaxProjects=0,
                    MaxTeamUsers=0,
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedOn = new DateTime(2022,01,19)

                },
                new CompanyPlan
                {
                    Id = new Guid("d0904499-8110-4dbb-9c18-5f17c7c51acc"),
                    CompanyId =new Guid("96FB88FC-7C25-4DB1-A206-08D9AA8CAE87") ,
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e") ,
                    ExpiredOn = new DateTime(2023,01,19),
                    PaymentProfileId = "customfree",
                    Active=true,
                    UpdatedBy="Migration",
                    CreatedBy = "Migration",
                    MaxClientUsers=0,
                    MaxKeywordsPerProject=0,
                    MaxProjects=0,
                    MaxTeamUsers=0,
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedOn = new DateTime(2022,01,19)

                }
                });

            modelBuilder.Entity<Feature>().HasData(new Feature[]
            {
                new Feature
                {
                    Id = new Guid("7b9f8f77-8c12-4864-85ce-1b38ce0c3b74"),
                    Descriptions = "WhiteLableClientDashboard",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new Feature
                {
                    Id = new Guid("5f42dc18-ac21-43dd-bf95-c1a778cfd1fe"),
                    Descriptions = "WeeklyRankTracing",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new Feature
                {
                    Id = new Guid("e8e2f9b3-bafd-468f-b767-ea6dcb7e761d"),
                    Descriptions = "GoogleAnalyticsIntegration",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new Feature
                {
                    Id = new Guid("6744ce57-c589-499a-aca2-d8f1cb28a4bf"),
                    Descriptions = "GoogleSearchConsoleIntegration",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new Feature
                {
                    Id = new Guid("f558f102-2408-4a80-b7a9-e195adefba55"),
                    Descriptions = "GoogleAdsIntegration",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new Feature
                {
                    Id = new Guid("516712fd-47b4-4d8b-bb2d-ff1050eca630"),
                    Descriptions = "FacebookAnalyticsIntegration",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new Feature
                {
                    Id = new Guid("b0c8e90d-b68b-401f-87fb-e4b4c63dd30d"),
                    Descriptions = "LinkedInIntegration",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new Feature
                {
                    Id = new Guid("a7d399f1-e11d-4ad4-b9c8-c84a116a78c1"),
                    Descriptions = "InstagramIntegration",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new Feature
                {
                    Id = new Guid("787a7d6d-49cd-4dad-afa0-ddcf126a8e8b"),
                    Descriptions = "FacebookAdsIntegration",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new Feature
                {
                    Id = new Guid("8c72a5bf-bc5c-4eeb-93c9-b33e0116387b"),
                    Descriptions = "CustomBrandedReportCreator",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new Feature
                {
                    Id = new Guid("f9963444-780f-4b98-85fe-1fb8c4d9dae2"),
                    Descriptions = "AutomatedReporting",
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
            });

            modelBuilder.Entity<PlanDetail>().HasData(new PlanDetail[]
           {

                new PlanDetail
                {
                    Id = new Guid("a2417fde-fe5a-4843-9f44-8679acb8071e"),
                    DefaultPlanId = new Guid("88C06521-D10D-4509-81A2-771C58DBB88D"),
                    FeatureID = new Guid("7b9f8f77-8c12-4864-85ce-1b38ce0c3b74"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("87941bfd-58e0-4a6b-a514-076174350d23"),
                    DefaultPlanId = new Guid("4de3eafa-1c0c-4b8a-9c9c-4c4cae5f3a26"),
                    FeatureID = new Guid("7b9f8f77-8c12-4864-85ce-1b38ce0c3b74"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("3b19feae-4a59-4249-8f74-09916864d9f2"),
                    DefaultPlanId = new Guid("4DE3EAFA-1C0C-4B8A-9C9C-4C4CAE5F3A26"),
                    FeatureID = new Guid("5f42dc18-ac21-43dd-bf95-c1a778cfd1fe"),
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                    Visibility = true,
                },
                new PlanDetail
                {
                    Id = new Guid("3e41c640-745f-420b-bfad-0f6d54d45212"),
                    DefaultPlanId = new Guid("88C06521-D10D-4509-81A2-771C58DBB88D"),
                    FeatureID = new Guid("5f42dc18-ac21-43dd-bf95-c1a778cfd1fe"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("7659538c-fb4f-4d81-a013-3905dfc9754b"),
                    DefaultPlanId = new Guid("B32218C3-427A-4CD0-B2AB-C71455E63951"),
                    FeatureID = new Guid("5f42dc18-ac21-43dd-bf95-c1a778cfd1fe"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("e1ac379b-c116-41a5-8614-c01adcb484d3"),
                    DefaultPlanId = new Guid("4DE3EAFA-1C0C-4B8A-9C9C-4C4CAE5F3A26"),
                    FeatureID = new Guid("e8e2f9b3-bafd-468f-b767-ea6dcb7e761d"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("9a8f1cba-5143-4ef8-b83c-eb676fd35b7d"),
                    DefaultPlanId = new Guid("88C06521-D10D-4509-81A2-771C58DBB88D"),
                    FeatureID = new Guid("e8e2f9b3-bafd-468f-b767-ea6dcb7e761d"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("c488bded-f58e-4896-9e65-add7ded51fd6"),
                    DefaultPlanId = new Guid("B32218C3-427A-4CD0-B2AB-C71455E63951"),
                    FeatureID = new Guid("e8e2f9b3-bafd-468f-b767-ea6dcb7e761d"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("f597de30-e986-4bf8-a4a7-5cee04c99a74"),
                    DefaultPlanId = new Guid("4DE3EAFA-1C0C-4B8A-9C9C-4C4CAE5F3A26"),
                    FeatureID = new Guid("6744ce57-c589-499a-aca2-d8f1cb28a4bf"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("f2424157-ddfd-4642-95d1-d0f45c988ddd"),
                    DefaultPlanId = new Guid("88C06521-D10D-4509-81A2-771C58DBB88D"),
                    FeatureID = new Guid("6744ce57-c589-499a-aca2-d8f1cb28a4bf"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("fb37a693-0fec-40c2-b77b-738407a38eb4"),
                    DefaultPlanId = new Guid("B32218C3-427A-4CD0-B2AB-C71455E63951"),
                    FeatureID = new Guid("6744ce57-c589-499a-aca2-d8f1cb28a4bf"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("ee432552-d73b-4f14-b2e4-6e4d60762e4e"),
                    DefaultPlanId = new Guid("4DE3EAFA-1C0C-4B8A-9C9C-4C4CAE5F3A26"),
                    FeatureID = new Guid("f558f102-2408-4a80-b7a9-e195adefba55"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("c82f65ec-c284-4bbd-9ec1-9ae728554e05"),
                    DefaultPlanId = new Guid("88C06521-D10D-4509-81A2-771C58DBB88D"),
                    FeatureID = new Guid("f558f102-2408-4a80-b7a9-e195adefba55"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("2ea0b2eb-b1ea-470e-a6bb-8ebf3b7f023b"),
                    DefaultPlanId = new Guid("B32218C3-427A-4CD0-B2AB-C71455E63951"),
                    FeatureID = new Guid("f558f102-2408-4a80-b7a9-e195adefba55"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("eb513d35-cd9c-4858-b2b7-c23791f05248"),
                    DefaultPlanId = new Guid("4DE3EAFA-1C0C-4B8A-9C9C-4C4CAE5F3A26"),
                    FeatureID = new Guid("516712fd-47b4-4d8b-bb2d-ff1050eca630"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("88997ec9-05cf-4786-8701-7196bd66f692"),
                    DefaultPlanId = new Guid("88C06521-D10D-4509-81A2-771C58DBB88D"),
                    FeatureID = new Guid("516712fd-47b4-4d8b-bb2d-ff1050eca630"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("13bbcd8b-4c4e-4fbe-a633-be877f2a7af6"),
                    DefaultPlanId = new Guid("B32218C3-427A-4CD0-B2AB-C71455E63951"),
                    FeatureID = new Guid("516712fd-47b4-4d8b-bb2d-ff1050eca630"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("3d361b1e-61e7-4859-9b71-f68c651fdefa"),
                    DefaultPlanId = new Guid("4DE3EAFA-1C0C-4B8A-9C9C-4C4CAE5F3A26"),
                    FeatureID = new Guid("b0c8e90d-b68b-401f-87fb-e4b4c63dd30d"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("47d1580e-34c8-43ad-a161-6cd089a5e367"),
                    DefaultPlanId = new Guid("88C06521-D10D-4509-81A2-771C58DBB88D"),
                    FeatureID = new Guid("b0c8e90d-b68b-401f-87fb-e4b4c63dd30d"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("34091aff-4bf0-419b-9975-98121ee34c20"),
                    DefaultPlanId = new Guid("B32218C3-427A-4CD0-B2AB-C71455E63951"),
                    FeatureID = new Guid("b0c8e90d-b68b-401f-87fb-e4b4c63dd30d"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("6f6c4c83-428c-423c-8229-b78ca2e2b6c7"),
                    DefaultPlanId = new Guid("4DE3EAFA-1C0C-4B8A-9C9C-4C4CAE5F3A26"),
                    FeatureID = new Guid("a7d399f1-e11d-4ad4-b9c8-c84a116a78c1"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("62c6d75b-b852-4016-a0ab-aaaab522e921"),
                    DefaultPlanId = new Guid("88C06521-D10D-4509-81A2-771C58DBB88D"),
                    FeatureID = new Guid("a7d399f1-e11d-4ad4-b9c8-c84a116a78c1"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("f13e1e86-a095-4392-9106-a78c8bcba87e"),
                    DefaultPlanId = new Guid("B32218C3-427A-4CD0-B2AB-C71455E63951"),
                    FeatureID = new Guid("a7d399f1-e11d-4ad4-b9c8-c84a116a78c1"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("30cf91b2-298f-4914-a389-478a6ccdbf0b"),
                    DefaultPlanId = new Guid("4DE3EAFA-1C0C-4B8A-9C9C-4C4CAE5F3A26"),
                    FeatureID = new Guid("787a7d6d-49cd-4dad-afa0-ddcf126a8e8b"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("ac3adbb3-7264-4b28-87e4-9dc34f7ea4c6"),
                    DefaultPlanId = new Guid("88C06521-D10D-4509-81A2-771C58DBB88D"),
                    FeatureID = new Guid("787a7d6d-49cd-4dad-afa0-ddcf126a8e8b"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("5a66f69d-352b-4d6e-9e08-83e421785456"),
                    DefaultPlanId = new Guid("B32218C3-427A-4CD0-B2AB-C71455E63951"),
                    FeatureID = new Guid("787a7d6d-49cd-4dad-afa0-ddcf126a8e8b"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("b865b74a-4d67-41c2-8f64-6465547c2193"),
                    DefaultPlanId = new Guid("4DE3EAFA-1C0C-4B8A-9C9C-4C4CAE5F3A26"),
                    FeatureID = new Guid("8c72a5bf-bc5c-4eeb-93c9-b33e0116387b"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("cb5530f3-2534-4b64-b6e2-8616ea013bc4"),
                    DefaultPlanId = new Guid("88C06521-D10D-4509-81A2-771C58DBB88D"),
                    FeatureID = new Guid("8c72a5bf-bc5c-4eeb-93c9-b33e0116387b"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("cdf22bd6-658c-4cf0-8d46-a5b9b9b39c9e"),
                    DefaultPlanId = new Guid("B32218C3-427A-4CD0-B2AB-C71455E63951"),
                    FeatureID = new Guid("8c72a5bf-bc5c-4eeb-93c9-b33e0116387b"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("9c6f9a3a-acb7-4e6c-97b9-4b4cd7af057b"),
                    DefaultPlanId = new Guid("4DE3EAFA-1C0C-4B8A-9C9C-4C4CAE5F3A26"),
                    FeatureID = new Guid("f9963444-780f-4b98-85fe-1fb8c4d9dae2"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("a7242d9a-c43f-4c53-8e21-a5b2374574d6"),
                    DefaultPlanId = new Guid("88C06521-D10D-4509-81A2-771C58DBB88D"),
                    FeatureID = new Guid("f9963444-780f-4b98-85fe-1fb8c4d9dae2"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },
                new PlanDetail
                {
                    Id = new Guid("8a1ad249-96fb-488b-b747-4418f0064e86"),
                    DefaultPlanId = new Guid("B32218C3-427A-4CD0-B2AB-C71455E63951"),
                    FeatureID = new Guid("f9963444-780f-4b98-85fe-1fb8c4d9dae2"),
                    Visibility = true,
                    CreatedBy = "Migration",
                    CreatedOn = new DateTime(2021,12,28),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2021,12,28),
                },

                //start
                new PlanDetail
                {
                    Id = new Guid("3896a69e-ac9a-4574-b21d-72a1c6011152"),
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"),
                    FeatureID = new Guid("7B9F8F77-8C12-4864-85CE-1B38CE0C3B74"),
                    Visibility = true,
                    CreatedBy = "Migration",
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2022,01,19),
                },
                new PlanDetail
                {
                    Id = new Guid("a1e61d02-d1b3-4697-be38-aaf830a8eae2"),
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"),
                    FeatureID = new Guid("F9963444-780F-4B98-85FE-1FB8C4D9DAE2"),
                    Visibility = true,
                    CreatedBy = "Migration",
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2022,01,19),
                },
                new PlanDetail
                {
                    Id = new Guid("f60045ec-467e-41db-8ad6-8a99a374cab7"),
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"),
                    FeatureID = new Guid("8C72A5BF-BC5C-4EEB-93C9-B33E0116387B"),
                    Visibility = true,
                    CreatedBy = "Migration",
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2022,01,19),
                },
                new PlanDetail
                {
                    Id = new Guid("48ceaedb-4c56-4976-8873-f34f3046418e"),
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"),
                    FeatureID = new Guid("5F42DC18-AC21-43DD-BF95-C1A778CFD1FE"),
                    Visibility = true,
                    CreatedBy = "Migration",
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2022,01,19),
                },
                new PlanDetail
                {
                    Id = new Guid("9e6fb990-13b3-4945-b863-c936af8ff209"),
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"),
                    FeatureID = new Guid("A7D399F1-E11D-4AD4-B9C8-C84A116A78C1"),
                    Visibility = true,
                    CreatedBy = "Migration",
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2022,01,19),
                },
                new PlanDetail
                {
                    Id = new Guid("5e339f81-e5af-4382-a03e-fbbf558f4114"),
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"),
                    FeatureID = new Guid("6744CE57-C589-499A-ACA2-D8F1CB28A4BF"),
                    Visibility = true,
                    CreatedBy = "Migration",
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2022,01,19),
                },
                new PlanDetail
                {
                    Id = new Guid("b01ca26a-dbbb-43a2-a7d5-ab60760ef350"),
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"),
                    FeatureID = new Guid("787A7D6D-49CD-4DAD-AFA0-DDCF126A8E8B"),
                    Visibility = true,
                    CreatedBy = "Migration",
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2022,01,19),
                },
                new PlanDetail
                {
                    Id = new Guid("2c496c99-796e-4906-a283-1b400e82babb"),
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"),
                    FeatureID = new Guid("F558F102-2408-4A80-B7A9-E195ADEFBA55"),
                    Visibility = true,
                    CreatedBy = "Migration",
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2022,01,19),
                },
                new PlanDetail
                {
                    Id = new Guid("49c9b4f4-fcd2-4472-9d9c-f18053ad0e79"),
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"),
                    FeatureID = new Guid("B0C8E90D-B68B-401F-87FB-E4B4C63DD30D"),
                    Visibility = true,
                    CreatedBy = "Migration",
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2022,01,19),
                },
                new PlanDetail
                {
                    Id = new Guid("e1a4d829-a85a-44e3-a0ca-05e66b9cbcfd"),
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"),
                    FeatureID = new Guid("E8E2F9B3-BAFD-468F-B767-EA6DCB7E761D"),
                    Visibility = true,
                    CreatedBy = "Migration",
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2022,01,19),
                },
                new PlanDetail
                {
                    Id = new Guid("85ad5833-5d35-48a2-a313-f7c4d3388938"),
                    DefaultPlanId = new Guid("f4bcd332-c9a9-4f19-a93e-f63810d8626e"),
                    FeatureID = new Guid("516712FD-47B4-4D8B-BB2D-FF1050ECA630"),
                    Visibility = true,
                    CreatedBy = "Migration",
                     CreatedOn = new DateTime(2022,01,19),
                    UpdatedBy = "Migration",
                    UpdatedOn = new DateTime(2022,01,19),
                }


           });

            // create index column for DomainWhitelabel Table
            modelBuilder.Entity<DomainWhitelabel>().HasIndex(c => new { c.DistributionId });

            // create index column for Company User Table
            modelBuilder.Entity<CompanyUser>().HasIndex(c => new { c.Role, c.UserId });

            // create index column for Campaign User Table
            modelBuilder.Entity<CampaignUser>().HasIndex(c => new { c.CompanyId, c.UserId });

            modelBuilder.Entity<ApiClaims>(entity =>
            {
                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasOne(d => d.ApiResource)
                    .WithMany(p => p.ApiClaims)
                    .HasForeignKey(d => d.ApiResourceId);
            });

            modelBuilder.Entity<ApiProperties>(entity =>
            {
                entity.Property(e => e.Key)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.HasOne(d => d.ApiResource)
                    .WithMany(p => p.ApiProperties)
                    .HasForeignKey(d => d.ApiResourceId);
            });

            modelBuilder.Entity<ApiResources>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(1000);

                entity.Property(e => e.DisplayName).HasMaxLength(200);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<ApiScopeClaims>(entity =>
            {
                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasOne(d => d.ApiScope)
                    .WithMany(p => p.ApiScopeClaims)
                    .HasForeignKey(d => d.ApiScopeId);
            });

            modelBuilder.Entity<ApiScopes>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(1000);

                entity.Property(e => e.DisplayName).HasMaxLength(200);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasOne(d => d.ApiResource)
                    .WithMany(p => p.ApiScopes)
                    .HasForeignKey(d => d.ApiResourceId);
            });

            modelBuilder.Entity<ApiSecrets>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(1000);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(4000);

                entity.HasOne(d => d.ApiResource)
                    .WithMany(p => p.ApiSecrets)
                    .HasForeignKey(d => d.ApiResourceId);
            });

            modelBuilder.Entity<AspNetRoleClaims>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.RoleId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetRoleClaims)
                    .HasForeignKey(d => d.RoleId);
            });

            modelBuilder.Entity<AspNetRoles>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name).HasMaxLength(256);

                entity.Property(e => e.NormalizedName).HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetUserClaims>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserClaims)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserLogins>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserLogins)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.RoleId);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUsers>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Birthday).HasDefaultValueSql("('0001-01-01T00:00:00.0000000')");

                entity.Property(e => e.Birthplace).HasMaxLength(200);

                entity.Property(e => e.Database).HasMaxLength(200);

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.Gender).HasMaxLength(200);

                entity.Property(e => e.LivesIn).HasMaxLength(200);

                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

                entity.Property(e => e.Occupation).HasMaxLength(200);

                entity.Property(e => e.PhoneNumber).HasMaxLength(200);

                entity.Property(e => e.UserName).HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetUserTokens>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserTokens)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<ClientClaims>(entity =>
            {
                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.ClientClaims)
                    .HasForeignKey(d => d.ClientId);
            });

            modelBuilder.Entity<ClientCorsOrigins>(entity =>
            {
                entity.Property(e => e.Origin)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.ClientCorsOrigins)
                    .HasForeignKey(d => d.ClientId);
            });

            modelBuilder.Entity<ClientGrantTypes>(entity =>
            {
                entity.Property(e => e.GrantType)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.ClientGrantTypes)
                    .HasForeignKey(d => d.ClientId);
            });

            modelBuilder.Entity<ClientIdPrestrictions>(entity =>
            {
                entity.ToTable("ClientIdPRestrictions");

                entity.Property(e => e.Provider)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.ClientIdPrestrictions)
                    .HasForeignKey(d => d.ClientId);
            });

            modelBuilder.Entity<ClientPostLogoutRedirectUris>(entity =>
            {
                entity.Property(e => e.PostLogoutRedirectUri)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.ClientPostLogoutRedirectUris)
                    .HasForeignKey(d => d.ClientId);
            });

            modelBuilder.Entity<ClientProperties>(entity =>
            {
                entity.Property(e => e.Key)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.ClientProperties)
                    .HasForeignKey(d => d.ClientId);
            });

            modelBuilder.Entity<ClientRedirectUris>(entity =>
            {
                entity.Property(e => e.RedirectUri)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.ClientRedirectUris)
                    .HasForeignKey(d => d.ClientId);
            });

            modelBuilder.Entity<Clients>(entity =>
            {
                entity.Property(e => e.BackChannelLogoutUri).HasMaxLength(2000);

                entity.Property(e => e.ClientClaimsPrefix).HasMaxLength(200);

                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.ClientName).HasMaxLength(200);

                entity.Property(e => e.ClientUri).HasMaxLength(2000);

                entity.Property(e => e.Description).HasMaxLength(1000);

                entity.Property(e => e.FrontChannelLogoutUri).HasMaxLength(2000);

                entity.Property(e => e.LogoUri).HasMaxLength(2000);

                entity.Property(e => e.PairWiseSubjectSalt).HasMaxLength(200);

                entity.Property(e => e.ProtocolType)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.UserCodeType).HasMaxLength(100);
            });

            modelBuilder.Entity<ClientScopes>(entity =>
            {
                entity.Property(e => e.Scope)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.ClientScopes)
                    .HasForeignKey(d => d.ClientId);
            });

            modelBuilder.Entity<ClientSecrets>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(2000);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(4000);

                entity.HasOne(d => d.Client)
                    .WithMany(p => p.ClientSecrets)
                    .HasForeignKey(d => d.ClientId);
            });

            modelBuilder.Entity<DeviceCodes>(entity =>
            {
                entity.HasKey(e => e.UserCode);

                entity.Property(e => e.UserCode)
                    .HasMaxLength(200)
                    .ValueGeneratedNever();

                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Data).IsRequired();

                entity.Property(e => e.DeviceCode)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.SubjectId).HasMaxLength(200);
            });

            modelBuilder.Entity<IdentityClaims>(entity =>
            {
                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasOne(d => d.IdentityResource)
                    .WithMany(p => p.IdentityClaims)
                    .HasForeignKey(d => d.IdentityResourceId);
            });

            modelBuilder.Entity<IdentityProperties>(entity =>
            {
                entity.Property(e => e.Key)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.HasOne(d => d.IdentityResource)
                    .WithMany(p => p.IdentityProperties)
                    .HasForeignKey(d => d.IdentityResourceId);
            });

            modelBuilder.Entity<IdentityResources>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(1000);

                entity.Property(e => e.DisplayName).HasMaxLength(200);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<PersistedGrants>(entity =>
            {
                entity.HasKey(e => e.Key);

                entity.Property(e => e.Key)
                    .HasMaxLength(200)
                    .ValueGeneratedNever();

                entity.Property(e => e.ClientId)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Data).IsRequired();

                entity.Property(e => e.SubjectId).HasMaxLength(200);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50);
            });





        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {

            // get added or updated entries
            var addedOrUpdatedEntries = ChangeTracker.Entries()
                    .Where(x => (x.State == EntityState.Added || x.State == EntityState.Modified));

            // fill out the audit fields
            foreach (var entry in addedOrUpdatedEntries)
            {
                var entity = entry.Entity as AuditableEntity;

                if (entry.State == EntityState.Added)
                {
                    if (_userInfoService.UserId == null)
                    {
                        entity.CreatedBy = "system";
                    }
                    else
                    {
                        entity.CreatedBy = _userInfoService.UserId;
                    }
                    entity.CreatedOn = DateTime.UtcNow;
                }

                entity.UpdatedBy = _userInfoService.UserId;
                entity.UpdatedOn = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
