using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
using EventManagement.Domain;
using EventManagement.Domain.Entities;
using EventManagement.Dto;
using EventManagement.Repository;
using EventManagement.Service;
using System;
using System.Reflection;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;
using IdentityServer4.AccessTokenValidation;
using IdentityModel.AspNetCore.OAuth2Introspection;
using FinanaceManagement.API.Models;
using Microsoft.OpenApi.Models;

namespace EventManagement.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers(setupAction => 
            {
                setupAction.ReturnHttpNotAcceptable = true;

                var jsonOutputFormatter = setupAction.OutputFormatters
               .OfType<NewtonsoftJsonOutputFormatter>().FirstOrDefault();

                if (jsonOutputFormatter != null)
                {
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.ChatGptReports.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.CampaignMailchimps.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.CampaignMicrosoftAds.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.CampaignCallRails.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.CampaignGoogleSheets.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.CancellationReasons.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.StripeCoupons.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.CampaignWooCommerces.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.CampaignGBPs.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.LinkedinAds.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.AppsumoPlans.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.TemplateSettings.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.ClientPostLogoutRedirectUris.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.ClientRedirectUris.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.DomainWhitelabels.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.EmailWhitelabels.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.CompanyPlans.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.PlanDetails.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.Features.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.DefaultPlans.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.ReportSchedulings.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.ReportSettings.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.CampaignLinkedins.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.CampaignFacebookAdss.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.CampaignInstagrams.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.CompanyUsers.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.CampaignUsers.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.CampaignGoogleAdss.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.CampaignFacebooks.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.CampaignGSCs.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.CampaignGoogleAnalyticss.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.GoogleAdsSummarys.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.SocialMediaSUmmmarys.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.GscSummarys.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.TrafficSummarys.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.RankingGraphs.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.Plans.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.Products.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.StripePayments.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.WhiteLabels.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.EmailSettings.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.Companys.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.Serps.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.Auditss.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.GoogleAnalyticsAccounts.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.GoogleAccountSetups.hateoas + json");
                   jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.EventManagement.Campaigns.hateoas + json");
                    jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.tourmanagement.events.hateoas+json");
                }

            }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
             .AddNewtonsoftJson(options =>
             {
                 options.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
                 options.SerializerSettings.ContractResolver =
                     new CamelCasePropertyNamesContractResolver();
             });

            #region CORS POLICY

            // Configure CORS so the API allows requests from JavaScript.  
            // For demo purposes, all origins/headers/methods are allowed.  
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOriginsHeadersAndMethods",
                    builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            #endregion

            #region REGISTER DB-CONTEXT

            var connectionString = Configuration["ConnectionStrings:EventManagementDB"];
            services.AddDbContext<EventManagementContext>(o => o.UseSqlServer(connectionString));

            #endregion

            // register the repository
           services.AddScoped<ICampaignService, CampaignService>();
           services.AddScoped<ICampaignRepository,CampaignRepository>();
           services.AddScoped<IGoogleAccountSetupService, GoogleAccountSetupService>();
           services.AddScoped<IGoogleAccountSetupRepository,GoogleAccountSetupRepository>();
           services.AddScoped<IGoogleAnalyticsAccountService, GoogleAnalyticsAccountService>();
           services.AddScoped<IGoogleAnalyticsAccountRepository,GoogleAnalyticsAccountRepository>();
           services.AddScoped<IAuditsService, AuditsService>();
           services.AddScoped<IAuditsRepository,AuditsRepository>();
           services.AddScoped<ISerpService, SerpService>();
           services.AddScoped<ISerpRepository,SerpRepository>();
           services.AddScoped<ICompanyService, CompanyService>();
           services.AddScoped<ICompanyRepository,CompanyRepository>();
           services.AddScoped<IEmailSettingService, EmailSettingService>();
           services.AddScoped<IEmailSettingRepository,EmailSettingRepository>();
           services.AddScoped<IWhiteLabelService, WhiteLabelService>();
           services.AddScoped<IWhiteLabelRepository,WhiteLabelRepository>();
           services.AddScoped<IStripePaymentService, StripePaymentService>();
           services.AddScoped<IStripePaymentRepository,StripePaymentRepository>();
           services.AddScoped<IProductService, ProductService>();
           services.AddScoped<IProductRepository,ProductRepository>();
           services.AddScoped<IPlanService, PlanService>();
           services.AddScoped<IPlanRepository,PlanRepository>();
           services.AddScoped<IRankingGraphService, RankingGraphService>();
           services.AddScoped<IRankingGraphRepository,RankingGraphRepository>();
           services.AddScoped<ITrafficSummaryService, TrafficSummaryService>();
           services.AddScoped<ITrafficSummaryRepository,TrafficSummaryRepository>();
           services.AddScoped<IGscSummaryService, GscSummaryService>();
           services.AddScoped<IGscSummaryRepository,GscSummaryRepository>();
           services.AddScoped<ISocialMediaSUmmmaryService, SocialMediaSUmmmaryService>();
           services.AddScoped<ISocialMediaSUmmmaryRepository,SocialMediaSUmmmaryRepository>();
           services.AddScoped<IGoogleAdsSummaryService, GoogleAdsSummaryService>();
           services.AddScoped<IGoogleAdsSummaryRepository,GoogleAdsSummaryRepository>();
           services.AddScoped<ICampaignGoogleAnalyticsService, CampaignGoogleAnalyticsService>();
           services.AddScoped<ICampaignGoogleAnalyticsRepository,CampaignGoogleAnalyticsRepository>();
           services.AddScoped<ICampaignGSCService, CampaignGSCService>();
           services.AddScoped<ICampaignGSCRepository,CampaignGSCRepository>();
           services.AddScoped<ICampaignFacebookService, CampaignFacebookService>();
           services.AddScoped<ICampaignFacebookRepository,CampaignFacebookRepository>();
           services.AddScoped<ICampaignGoogleAdsService, CampaignGoogleAdsService>();
           services.AddScoped<ICampaignGoogleAdsRepository,CampaignGoogleAdsRepository>();
           services.AddScoped<ICampaignUserService, CampaignUserService>();
           services.AddScoped<ICampaignUserRepository,CampaignUserRepository>();
           services.AddScoped<ICompanyUserService, CompanyUserService>();
           services.AddScoped<ICompanyUserRepository,CompanyUserRepository>();
           services.AddScoped<ICampaignInstagramService, CampaignInstagramService>();
           services.AddScoped<ICampaignInstagramRepository,CampaignInstagramRepository>();
           services.AddScoped<ICampaignFacebookAdsService, CampaignFacebookAdsService>();
           services.AddScoped<ICampaignFacebookAdsRepository,CampaignFacebookAdsRepository>();
           services.AddScoped<ICampaignLinkedinService, CampaignLinkedinService>();
           services.AddScoped<ICampaignLinkedinRepository,CampaignLinkedinRepository>();
           services.AddScoped<IReportSettingService, ReportSettingService>();
           services.AddScoped<IReportSettingRepository,ReportSettingRepository>();
           services.AddScoped<IReportSchedulingService, ReportSchedulingService>();
           services.AddScoped<IReportSchedulingRepository,ReportSchedulingRepository>();
           services.AddScoped<IDefaultPlanService, DefaultPlanService>();
           services.AddScoped<IDefaultPlanRepository,DefaultPlanRepository>();
           services.AddScoped<IFeatureService, FeatureService>();
           services.AddScoped<IFeatureRepository,FeatureRepository>();
           services.AddScoped<IPlanDetailService, PlanDetailService>();
           services.AddScoped<IPlanDetailRepository,PlanDetailRepository>();
           services.AddScoped<ICompanyPlanService, CompanyPlanService>();
           services.AddScoped<ICompanyPlanRepository,CompanyPlanRepository>();
           services.AddScoped<IEmailWhitelabelService, EmailWhitelabelService>();
           services.AddScoped<IEmailWhitelabelRepository,EmailWhitelabelRepository>();
           services.AddScoped<IDomainWhitelabelService, DomainWhitelabelService>();
           services.AddScoped<IDomainWhitelabelRepository,DomainWhitelabelRepository>();
           services.AddScoped<IClientRedirectUriService, ClientRedirectUriService>();
           services.AddScoped<IClientRedirectUriRepository,ClientRedirectUriRepository>();
           services.AddScoped<IClientPostLogoutRedirectUriService, ClientPostLogoutRedirectUriService>();
           services.AddScoped<IClientPostLogoutRedirectUriRepository,ClientPostLogoutRedirectUriRepository>();
           services.AddScoped<ITemplateSettingService, TemplateSettingService>();
           services.AddScoped<ITemplateSettingRepository,TemplateSettingRepository>();
           services.AddScoped<IAppsumoPlanService, AppsumoPlanService>();
           services.AddScoped<IAppsumoPlanRepository,AppsumoPlanRepository>();
           services.AddScoped<ILinkedinAdService, LinkedinAdService>();
           services.AddScoped<ILinkedinAdRepository,LinkedinAdRepository>();
           services.AddScoped<ICampaignGBPService, CampaignGBPService>();
           services.AddScoped<ICampaignGBPRepository,CampaignGBPRepository>();
           services.AddScoped<ICampaignWooCommerceService, CampaignWooCommerceService>();
           services.AddScoped<ICampaignWooCommerceRepository,CampaignWooCommerceRepository>();
           services.AddScoped<IStripeCouponService, StripeCouponService>();
           services.AddScoped<IStripeCouponRepository,StripeCouponRepository>();
           services.AddScoped<ICancellationReasonService, CancellationReasonService>();
           services.AddScoped<ICancellationReasonRepository,CancellationReasonRepository>();
           services.AddScoped<ICampaignGoogleSheetService, CampaignGoogleSheetService>();
           services.AddScoped<ICampaignGoogleSheetRepository,CampaignGoogleSheetRepository>();
           services.AddScoped<ICampaignCallRailService, CampaignCallRailService>();
           services.AddScoped<ICampaignCallRailRepository,CampaignCallRailRepository>();
           services.AddScoped<ICampaignMailchimpService, CampaignMailchimpService>();
           services.AddScoped<ICampaignMailchimpRepository,CampaignMailchimpRepository>();
           services.AddScoped<ICampaignMicrosoftAdService, CampaignMicrosoftAdService>();
           services.AddScoped<ICampaignMicrosoftAdRepository,CampaignMicrosoftAdRepository>();
           services.AddScoped<IChatGptReportService, ChatGptReportService>();
      
            services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

            // register the repository
            services.AddScoped<IEventRepository,EventRepository>();
            services.AddScoped<IAspUserRepository, AspUserRepository>();

            // register an IHttpContextAccessor so we can access the current
            // HttpContext in services by injecting it
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // register the user info service
            services.AddScoped<IUserInfoService, UserInfoService>();

            // register the user info service
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IAspUserService, AspUserService>();

            // register the aws service
            services.AddScoped<IAwsService, AwsService>();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddScoped<IUrlHelper, UrlHelper>(implementationFactory =>
            {
                var actionContext = implementationFactory.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });
            var identityServerUrl = Configuration["IdentityServerUrl"];

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = identityServerUrl;
                    //  options.RequireHttpsMetadata = false; // only for development
                    options.ApiName = "tourmanagementapi";
                    options.ApiSecret = "secret";
                    options.EnableCaching = true;
                    options.CacheDuration = TimeSpan.FromMinutes(1); // that's the default
                    options.TokenRetriever = new Func<HttpRequest, string>(req =>
                    {
                        var fromHeader = TokenRetrieval.FromAuthorizationHeader();
                        var fromQuery = TokenRetrieval.FromQueryString();
                        return fromHeader(req) ?? fromQuery(req);
                    });

                });

            services.AddSwaggerGen(setupAction =>
            {
                setupAction.SwaggerDoc("EventManagementAPISpecification", new OpenApiInfo
                {
                    Title = "Event Management API",
                    Version = "1",
                    Description = "Through this api you can access Event related entities.",

                });

            
                var xmlCommentFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlCommentDtoFile = "EventManagement.Dto.xml";
                var xmlCommentUtilFile = "EventManagement.Utility.xml";
                var xmlCommentFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentFile);
                var xmlCommentDtoFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentDtoFile);
                var xmlCommentUtilFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentUtilFile);
                setupAction.IncludeXmlComments(xmlCommentFullPath);
                setupAction.IncludeXmlComments(xmlCommentDtoFullPath);
                setupAction.IncludeXmlComments(xmlCommentUtilFullPath);
                setupAction.CustomSchemaIds(i => i.FullName);
                setupAction.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(identityServerUrl + "/connect/authorize", UriKind.Absolute),
                            Scopes = new Dictionary<string, string>
                            {
                                { "tourmanagementapi", "Tour Management API" }
                            },
                            TokenUrl = new Uri(identityServerUrl + "/connect/token", UriKind.Absolute)
                        }
                    }
                });
                setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "oauth2",
                                Type = ReferenceType.SecurityScheme
                            }
                        },new List<string>() { "tourmanagementapi" }
                    }
                });
            });


            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var actionExecutingContext =
                        actionContext as Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext;

                    // if there are modelstate errors & all keys were correctly
                    // found/parsed we're dealing with validation errors
                    if (actionContext.ModelState.ErrorCount > 0
                        && actionExecutingContext?.ActionArguments.Count == actionContext.ActionDescriptor.Parameters.Count)
                    {
                        return new UnprocessableEntityObjectResult(actionContext.ModelState);
                    }

                    // if one of the keys wasn't correctly found / couldn't be parsed
                    // we're dealing with null/unparsable input
                    return new BadRequestObjectResult(actionContext.ModelState);
                };
            });
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);

            services.AddDistributedMemoryCache(); // Use in-memory cache for session data
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20); // Set session timeout
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, EventManagementContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();                
            }
            else
            {
                //TODO::
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async (ctx) =>
                    {
                        var exceptionHandlerFeature = ctx.Features.Get<IExceptionHandlerFeature>();
                        if (exceptionHandlerFeature != null)
                        {
                            var logger = loggerFactory.CreateLogger("Global exception logger");
                            logger.LogError(500,
                                exceptionHandlerFeature.Error,
                                exceptionHandlerFeature.Error.Message);
                        }

                        ctx.Response.StatusCode = 500;
                        await ctx.Response.WriteAsync("An unexpected fault happened. Try again later.");
                    });
                });

                app.UseHsts();
            }

            AutoMapper.Mapper.Initialize(config =>
            {
               config.CreateMap<CampaignMailchimpForUpdate, CampaignMailchimp>();
               config.CreateMap<CampaignMailchimpForCreation, CampaignMailchimp>();
               config.CreateMap<CampaignMailchimp, CampaignMailchimpDto>();
               config.CreateMap<CampaignMicrosoftAdForUpdate, CampaignMicrosoftAd>();
               config.CreateMap<CampaignMicrosoftAdForCreation, CampaignMicrosoftAd>();
               config.CreateMap<CampaignMicrosoftAd, CampaignMicrosoftAdDto>();
               config.CreateMap<CampaignCallRailForUpdate, CampaignCallRail>();
               config.CreateMap<CampaignCallRailForCreation, CampaignCallRail>();
               config.CreateMap<CampaignCallRail, CampaignCallRailDto>();
               config.CreateMap<CampaignGoogleSheetForUpdate, CampaignGoogleSheet>();
               config.CreateMap<CampaignGoogleSheetForCreation, CampaignGoogleSheet>();
               config.CreateMap<CampaignGoogleSheet, CampaignGoogleSheetDto>();
               config.CreateMap<CancellationReasonForUpdate, CancellationReason>();
               config.CreateMap<CancellationReasonForCreation, CancellationReason>();
               config.CreateMap<CancellationReason, CancellationReasonDto>();
               config.CreateMap<StripeCouponForUpdate, StripeCoupon>();
               config.CreateMap<StripeCouponForCreation, StripeCoupon>();
               config.CreateMap<StripeCoupon, StripeCouponDto>();
               config.CreateMap<CampaignWooCommerceForUpdate, CampaignWooCommerce>();
               config.CreateMap<CampaignWooCommerceForCreation, CampaignWooCommerce>();
               config.CreateMap<CampaignWooCommerce, CampaignWooCommerceDto>();
               config.CreateMap<CampaignGBPForUpdate, CampaignGBP>();
               config.CreateMap<CampaignGBPForCreation, CampaignGBP>();
               config.CreateMap<CampaignGBP, CampaignGBPDto>();
               config.CreateMap<LinkedinAdForUpdate, LinkedinAd>();
               config.CreateMap<LinkedinAdForCreation, LinkedinAd>();
               config.CreateMap<LinkedinAd, LinkedinAdDto>();
               config.CreateMap<AppsumoPlanForUpdate, AppsumoPlan>();
               config.CreateMap<AppsumoPlanForCreation, AppsumoPlan>();
               config.CreateMap<AppsumoPlan, AppsumoPlanDto>();
               config.CreateMap<TemplateSettingForUpdate, TemplateSetting>();
               config.CreateMap<TemplateSettingForCreation, TemplateSetting>();
               config.CreateMap<TemplateSetting, TemplateSettingDto>();
               config.CreateMap<DomainWhitelabelForUpdate, DomainWhitelabel>();
               config.CreateMap<DomainWhitelabelForCreation, DomainWhitelabel>();
               config.CreateMap<DomainWhitelabel, DomainWhitelabelDto>();
               config.CreateMap<CompanyPlanForUpdate, CompanyPlan>();
               config.CreateMap<CompanyPlanForCreation, CompanyPlan>();
               config.CreateMap<CompanyPlan, CompanyPlanDto>();
               config.CreateMap<PlanDetailForUpdate, PlanDetail>();
               config.CreateMap<PlanDetailForCreation, PlanDetail>();
               config.CreateMap<PlanDetail, PlanDetailDto>();
               config.CreateMap<FeatureForUpdate, Feature>();
               config.CreateMap<FeatureForCreation, Feature>();
               config.CreateMap<Feature, FeatureDto>();
               config.CreateMap<DefaultPlanForUpdate, DefaultPlan>();
               config.CreateMap<DefaultPlanForCreation, DefaultPlan>();
               config.CreateMap<DefaultPlan, DefaultPlanDto>();
               config.CreateMap<EmailWhitelabelForUpdate, EmailWhitelabel>();
               config.CreateMap<EmailWhitelabelForCreation, EmailWhitelabel>();
               config.CreateMap<EmailWhitelabel, EmailWhitelabelDto>();
               config.CreateMap<ReportSchedulingForUpdate, ReportScheduling>();
               config.CreateMap<ReportSchedulingForCreation, ReportScheduling>();
               config.CreateMap<ReportScheduling, ReportSchedulingDto>();
               config.CreateMap<ReportSettingForUpdate, ReportSetting>();
               config.CreateMap<ReportSettingForCreation, ReportSetting>();
               config.CreateMap<ReportSetting, ReportSettingDto>();
               config.CreateMap<CampaignLinkedinForUpdate, CampaignLinkedin>();
               config.CreateMap<CampaignLinkedinForCreation, CampaignLinkedin>();
               config.CreateMap<CampaignLinkedin, CampaignLinkedinDto>();
               config.CreateMap<CampaignFacebookAdsForUpdate, CampaignFacebookAds>();
               config.CreateMap<CampaignFacebookAdsForCreation, CampaignFacebookAds>();
               config.CreateMap<CampaignFacebookAds, CampaignFacebookAdsDto>();
               config.CreateMap<CampaignInstagramForUpdate, CampaignInstagram>();
               config.CreateMap<CampaignInstagramForCreation, CampaignInstagram>();
               config.CreateMap<CampaignInstagram, CampaignInstagramDto>();
               config.CreateMap<CompanyUserForUpdate, CompanyUser>();
               config.CreateMap<CompanyUserForCreation, CompanyUser>();
               config.CreateMap<CompanyUser, CompanyUserDto>();
               config.CreateMap<CampaignUserForUpdate, CampaignUser>();
               config.CreateMap<CampaignUserForCreation, CampaignUser>();
               config.CreateMap<CampaignUser, CampaignUserDto>();
               config.CreateMap<CampaignGoogleAdsForUpdate, CampaignGoogleAds>();
               config.CreateMap<CampaignGoogleAdsForCreation, CampaignGoogleAds>();
               config.CreateMap<CampaignGoogleAds, CampaignGoogleAdsDto>();
               config.CreateMap<CampaignFacebookForUpdate, CampaignFacebook>();
               config.CreateMap<CampaignFacebookForCreation, CampaignFacebook>();
               config.CreateMap<CampaignFacebook, CampaignFacebookDto>();
               config.CreateMap<CampaignGSCForUpdate, CampaignGSC>();
               config.CreateMap<CampaignGSCForCreation, CampaignGSC>();
               config.CreateMap<CampaignGSC, CampaignGSCDto>();
               config.CreateMap<CampaignGoogleAnalyticsForUpdate, CampaignGoogleAnalytics>();
               config.CreateMap<CampaignGoogleAnalyticsForCreation, CampaignGoogleAnalytics>();
               config.CreateMap<CampaignGoogleAnalytics, CampaignGoogleAnalyticsDto>();
               config.CreateMap<GoogleAdsSummaryForUpdate, GoogleAdsSummary>();
               config.CreateMap<GoogleAdsSummaryForCreation, GoogleAdsSummary>();
               config.CreateMap<GoogleAdsSummary, GoogleAdsSummaryDto>();
               config.CreateMap<SocialMediaSUmmmaryForUpdate, SocialMediaSUmmmary>();
               config.CreateMap<SocialMediaSUmmmaryForCreation, SocialMediaSUmmmary>();
               config.CreateMap<SocialMediaSUmmmary, SocialMediaSUmmmaryDto>();
               config.CreateMap<GscSummaryForUpdate, GscSummary>();
               config.CreateMap<GscSummaryForCreation, GscSummary>();
               config.CreateMap<GscSummary, GscSummaryDto>();
               config.CreateMap<TrafficSummaryForUpdate, TrafficSummary>();
               config.CreateMap<TrafficSummaryForCreation, TrafficSummary>();
               config.CreateMap<TrafficSummary, TrafficSummaryDto>();
               config.CreateMap<RankingGraphForUpdate, RankingGraph>();
               config.CreateMap<RankingGraphForCreation, RankingGraph>();
               config.CreateMap<RankingGraph, RankingGraphDto>();
               config.CreateMap<PlanForUpdate, Plan>();
               config.CreateMap<PlanForCreation, Plan>();
               config.CreateMap<Plan, PlanDto>();
               config.CreateMap<ProductForUpdate, Product>();
               config.CreateMap<ProductForCreation, Product>();
               config.CreateMap<Product, ProductDto>();
               config.CreateMap<StripePaymentForUpdate, StripePayment>();
               config.CreateMap<StripePaymentForCreation, StripePayment>();
               config.CreateMap<StripePayment, StripePaymentDto>();
               config.CreateMap<WhiteLabelForUpdate, WhiteLabel>();
               config.CreateMap<WhiteLabelForCreation, WhiteLabel>();
               config.CreateMap<WhiteLabel, WhiteLabelDto>();
               config.CreateMap<EmailSettingForUpdate, EmailSetting>();
               config.CreateMap<EmailSettingForCreation, EmailSetting>();
               config.CreateMap<EmailSetting, EmailSettingDto>();
               config.CreateMap<CompanyForUpdate, Company>();
               config.CreateMap<CompanyForCreation, Company>();
               config.CreateMap<Company, CompanyDto>();
               config.CreateMap<SerpForUpdate, Serp>();
               config.CreateMap<SerpForCreation, Serp>();
               config.CreateMap<Serp, SerpDto>();
               config.CreateMap<AuditsForUpdate, Audits>();
               config.CreateMap<AuditsForCreation, Audits>();
               config.CreateMap<Audits, AuditsDto>();
               config.CreateMap<GoogleAnalyticsAccountForUpdate, GoogleAnalyticsAccount>();
               config.CreateMap<GoogleAnalyticsAccountForCreation, GoogleAnalyticsAccount>();
               config.CreateMap<GoogleAnalyticsAccount, GoogleAnalyticsAccountDto>();
               config.CreateMap<GoogleAccountSetupForUpdate, GoogleAccountSetup>();
               config.CreateMap<GoogleAccountSetupForCreation, GoogleAccountSetup>();
               config.CreateMap<GoogleAccountSetup, GoogleAccountSetupDto>();
               config.CreateMap<CampaignForUpdate, Campaign>();
               config.CreateMap<CampaignForCreation, Campaign>();
               config.CreateMap<Campaign, CampaignDto>();

                config.CreateMap<AspNetUsers, AspUserDto>();
                config.CreateMap<AspUserForCreation, AspNetUsers>();
                config.CreateMap<AspUserForUpdate, AspNetUsers>();

                config.CreateMap<ClientRedirectUris, ClientRedirectUriDto>();
                config.CreateMap<ClientRedirectUriForCreation, ClientRedirectUris>();
                config.CreateMap<ClientRedirectUriForUpdate, ClientRedirectUris>();


                config.CreateMap<ClientPostLogoutRedirectUris, ClientPostLogoutRedirectUriDto>();
                config.CreateMap<ClientPostLogoutRedirectUriForCreation, ClientPostLogoutRedirectUris>();
                config.CreateMap<ClientPostLogoutRedirectUriForUpdate, ClientPostLogoutRedirectUris>();

                config.CreateMap<Event, EventDto>();
                config.CreateMap<EventForCreation, Event>();
                config.CreateMap<EventForUpdate, Event>();

            });
            app.UseRouting();
            // Enable CORS
            app.UseCors("AllowAllOriginsHeadersAndMethods");

            app.UseHttpsRedirection();

            app.UseSwagger();
            var hostedURL = Configuration["HostedUrl"];
            app.UseSwaggerUI(setupAction =>
            {
                setupAction.SwaggerEndpoint("/swagger/EventManagementAPISpecification/swagger.json", "EventManagement API");
                setupAction.RoutePrefix = "";
                setupAction.OAuthClientId("swaggerui");
                setupAction.OAuth2RedirectUrl(hostedURL + "oauth2-redirect.html");
                setupAction.DocExpansion(DocExpansion.None);
                setupAction.EnableDeepLinking();
                setupAction.DisplayOperationId();
            });

            app.UseAuthorization();
            app.UseEndpoints(routes =>
            {
                routes.MapControllerRoute(
                   "default",
                   "{controller=home}/{action=Index}/{id?}");
            });
            app.UseAuthentication();
            //Add the seed data to the database.
            context.EnsureSeedDataForContext();

            app.UseSession();
            app.UseRouting();


        }
    }
}
