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
using System.Runtime.InteropServices;
using Stripe;
using Stripe.Checkout;
using System.Xml.Linq;

namespace EventManagement.Service
{
    public class CompanyPlanService : ServiceBase<CompanyPlan, Guid>, ICompanyPlanService
    {

        #region PRIVATE MEMBERS

        private readonly ICompanyPlanRepository _companyplanRepository;
        private readonly IConfiguration _configuration;
        public readonly IDefaultPlanService _defaultPlanService;
        public readonly IAppsumoPlanService _appsumoPlanService;
        public readonly IStripePaymentService _stripePaymentService;
        #endregion


        #region CONSTRUCTOR

        public CompanyPlanService(IStripePaymentService stripePaymentService, IAppsumoPlanService appsumoPlanService, ICompanyPlanRepository companyplanRepository, ILogger<CompanyPlanService> logger, IConfiguration configuration
            , IDefaultPlanService defaultPlanService) : base(companyplanRepository, logger)
        {
            _companyplanRepository = companyplanRepository;
            _configuration = configuration;
            _defaultPlanService = defaultPlanService;
            _appsumoPlanService = appsumoPlanService;
            _stripePaymentService = stripePaymentService;
        }

        #endregion


        #region PUBLIC MEMBERS   

        public async Task<List<CompanyTransactionDto>> GetAllTransactionHistoryByCompanyId(Guid id)
        {
            var returnDto = new List<CompanyTransactionDto>();

            // get defult plans
            var defaultPlanList = _defaultPlanService.GetAllEntities().ToList();

            // get company plans
            var companyPlanList = _companyplanRepository.GetAllEntities()
                .AsQueryable()
                .Where(x => x.CompanyId == id)
                .OrderByDescending(x => x.CreatedOn)
                .ToList();

            foreach (var plan in companyPlanList)
            {
                var defaultPlan = defaultPlanList.Where(x => x.Id == plan.DefaultPlanId).FirstOrDefault();
                bool appsumo = defaultPlan.Name.Contains("_tier") ? true : false;
                string planType = appsumo ? "Lifetime" : _stripePaymentService.IsMonthlyPlan(plan.PaymentProfileId) ? "Monthly" : "Yearly";

                var entity = new CompanyTransactionDto();
                entity.Id = plan.Id;
                entity.CompanyId = plan.CompanyId;
                entity.DefaultPlanId = plan.DefaultPlanId;
                entity.CreatedOn = plan.CreatedOn;
                entity.ExpiredOn = plan.ExpiredOn;
                entity.PaymentProfileId = plan.PaymentProfileId;
                entity.Active = plan.Active;
                entity.MaxProjects = plan.MaxProjects;
                entity.MaxTeamUsers = plan.MaxTeamUsers;
                entity.MaxClientUsers = plan.MaxClientUsers;
                entity.MaxKeywordsPerProject = plan.MaxKeywordsPerProject;
                entity.DefaultPlanName = GetPlanNameByAppsumoPlanId(defaultPlan.Name);
                entity.DefaultPlanCost = defaultPlan.Cost;
                entity.Type = planType;
                entity.AppsumoPlan = appsumo;

                returnDto.Add(entity);
            }

            return returnDto;
        }
        public async Task<AppsumoPlanDetailDto> GetAppsumoPlanByCompanyPlanId(Guid id)
        {
            var returnDto = new AppsumoPlanDetailDto();
            returnDto.FeatureList = new List<string>();

            var companyPlanDto = _companyplanRepository.GetEntityById(id);
            var planDetail = _defaultPlanService.GetEntityById(companyPlanDto.DefaultPlanId);
            string[] fetures = new string[10] {
                "SEO word search",
                "Weekly rank tracking",
                "Google analytics integration",
                "Google Search Console integration",
                "Facebook analytics integration",
                "Linkedin integration",
                "Custom branded report creator",
                "Automated reporting",
                "Unlimited team users",
                "Unlimited client users"
            };
            returnDto.FeatureList.Add(companyPlanDto.MaxProjects + " projects");
            var keywords = companyPlanDto.MaxKeywordsPerProject > 0 ? companyPlanDto.MaxKeywordsPerProject.ToString() : "∞";
            returnDto.FeatureList.Add(keywords + " keywords per project");
            returnDto.FeatureList.AddRange(fetures);
            if (planDetail.Name != "agencyeasy_tier1") { returnDto.FeatureList.Add("White labelled client dashboard"); }

            returnDto.Id = companyPlanDto.Id;
            returnDto.DefaultPlanId = planDetail.Id;
            returnDto.AppsumoPlanId = planDetail.Name;
            returnDto.Name = GetPlanNameByAppsumoPlanId(planDetail.Name);
            returnDto.Cost = planDetail.Cost;
            returnDto.MaxProjects = companyPlanDto.MaxProjects;
            returnDto.MaxKeywordsPerProject = companyPlanDto.MaxKeywordsPerProject;
            returnDto.MaxTeamUsers = companyPlanDto.MaxTeamUsers;
            returnDto.WhitelabelSupport = planDetail.Name == "agencyeasy_tier1" ? false : true;

            return returnDto;
        }

        public async Task<bool> IsInvoiceExists(string id)
        {
            var retVal = false;
            if (!string.IsNullOrEmpty(id))
            {
                retVal = _companyplanRepository.Exist(x => x.PaymentProfileId == id);
            }
            else
            {
                retVal = true;
            }
            
            return retVal;
        }


        public DefaultPlanDto GetDefaultPlanForAppsumo(string planName)
        {
            return _defaultPlanService.GetDefaultPlan(planName);
    }

        private string GetPlanNameByAppsumoPlanId(string AppsumoPlanId)
        {
            string name = "";
            if (AppsumoPlanId == "agencyeasy_tier1")
            {
                name = "License Tier 1";
            }
            else if (AppsumoPlanId == "agencyeasy_tier2")
            {
                name = "License Tier 2";
            }
            else if (AppsumoPlanId == "agencyeasy_tier3")
            {
                name = "License Tier 3";
            }
            else if (AppsumoPlanId == "agencyeasy_tier4")
            {
                name = "License Tier 4";
            }
            else if (AppsumoPlanId == "agencyeasy_tier5")
            {
                name = "License Tier 5";
            }
            else
            {
                name = AppsumoPlanId;
            }

            return name;
        }

        public async Task<bool> createCompanyPlan(string companyId, bool isFree, [Optional] string paymentIdForFortyNineUsdPlan, [Optional] string PlanName, bool Downgrade = false)
        {
            var defaultPlan = new DefaultPlanDto();
            var companyPlanDto = new CompanyPlanForCreation();

            if (isFree)
            {
                defaultPlan = _defaultPlanService.GetDefaultPlan("FREE");
                companyPlanDto.PaymentProfileId = "free";
                companyPlanDto.ExpiredOn = DateTime.UtcNow.Date.AddDays(14);
                companyPlanDto.IsDowngradeAppsumo = false;
            }
            else if (PlanName == "LIFETIMEDEAL" || PlanName == "LifeTimeDeal" || PlanName == "lifetimedeal")
            {
                defaultPlan = _defaultPlanService.GetDefaultPlan("LIFETIMEDEAL");
                companyPlanDto.PaymentProfileId = paymentIdForFortyNineUsdPlan;
                companyPlanDto.ExpiredOn = DateTime.UtcNow.Date.AddYears(1);
                companyPlanDto.IsDowngradeAppsumo = false;
            }
            else if (PlanName == "onetimedeal" || PlanName == "ONETIMEDEAL" || PlanName == "OneTimeDeal")
            {
                defaultPlan = _defaultPlanService.GetDefaultPlan("ONETIMEDEAL");
                companyPlanDto.PaymentProfileId = paymentIdForFortyNineUsdPlan;
                companyPlanDto.ExpiredOn = DateTime.UtcNow.Date.AddYears(1);
                companyPlanDto.IsDowngradeAppsumo = false;
            }
            else if (PlanName.ToLower() == "starter" || PlanName.ToLower() == "growth" || PlanName.ToLower() == "professional")
            {
                defaultPlan = _defaultPlanService.GetDefaultPlan(PlanName);
                companyPlanDto.PaymentProfileId = paymentIdForFortyNineUsdPlan;
                companyPlanDto.ExpiredOn = DateTime.UtcNow.Date.AddYears(1);
                companyPlanDto.IsDowngradeAppsumo = false;
            }
            else if (PlanName.Contains("agencyeasy_tier"))
            {
                defaultPlan = _defaultPlanService.GetDefaultPlan(PlanName);
                companyPlanDto.PaymentProfileId = paymentIdForFortyNineUsdPlan;
                companyPlanDto.ExpiredOn = DateTime.UtcNow.Date.AddYears(100);
                companyPlanDto.IsDowngradeAppsumo = Downgrade;
            }

            var returnValue = false;

            try
            {
                companyPlanDto.Id = new Guid();
                companyPlanDto.CompanyId = new Guid(companyId);
                companyPlanDto.DefaultPlanId = defaultPlan.Id;
                companyPlanDto.Active = true;
                companyPlanDto.MaxProjects = defaultPlan.MaxProjects;
                companyPlanDto.MaxClientUsers = defaultPlan.MaxClientUsers;
                companyPlanDto.MaxTeamUsers = defaultPlan.MaxTeamUsers;
                companyPlanDto.MaxKeywordsPerProject = defaultPlan.MaxKeywordsPerProject;

                await CreateEntityAsync<CompanyPlanDto, CompanyPlanForCreation>(companyPlanDto);
                returnValue = true;
            }

            catch (Exception e)
            {
                returnValue = false;
            }

            return returnValue;
        }

        public async Task<bool> CreateStripePlan(CompanyPlanDetailDto companyplan)
        {
            try
            {
                var stripeSecretKey = new StripeClient(_configuration["StripeSecret"]);

                // cancel current subscription plan
                var currentPlan = _companyplanRepository.GetFilteredEntities().Where(x => x.CompanyId == companyplan.CompanyId && x.Active).FirstOrDefault();
                if (currentPlan != null)
                {
                    // if current plan is not free plan then cancel from stripe also
                    if (currentPlan.PaymentProfileId.Contains("sub_"))
                    {
                        var subOption = new SubscriptionCancelOptions
                        {
                            InvoiceNow = true,
                        };

                        var service = new SubscriptionService(stripeSecretKey);

                        var subscriptionRes = await service.GetAsync(currentPlan.PaymentProfileId);

                        //if (subscriptionRes.Status == "active")
                        //{
                        //    var response = service.Cancel(currentPlan.PaymentProfileId, subOption);
                        //}
                    }

                    currentPlan.Active = false;
                    currentPlan.UpdatedOn = DateTime.UtcNow;
                    currentPlan.ExpiredOn = DateTime.UtcNow;

                    _companyplanRepository.UpdateEntity(currentPlan);
                    _companyplanRepository.SaveChanges();
                }

                // subscribed new plan
                var sessionService = new SessionService(stripeSecretKey);
                Session session = sessionService.Get(companyplan.SessionId);
                var selectedNewPlan = session.Metadata["PlanId"];
                var maxProject = session.Metadata["ProjectCount"];

                var newPlanDetails = _defaultPlanService.GetDefaultPlanById(selectedNewPlan);

                var subscriptionService = new SubscriptionService(stripeSecretKey);
                var subscription = subscriptionService.Get(session.SubscriptionId);

                // add new plan entry
                var entity = new CompanyPlan();
                entity.CompanyId = companyplan.CompanyId;
                entity.DefaultPlanId = new Guid(selectedNewPlan);
                entity.Active = true;
                entity.ExpiredOn = subscription.CurrentPeriodEnd;
                entity.PaymentProfileId = subscription.Id;
                entity.MaxKeywordsPerProject = newPlanDetails.MaxKeywordsPerProject;
                entity.MaxProjects = Int32.Parse(maxProject);
                entity.MaxTeamUsers = newPlanDetails.MaxTeamUsers;
                entity.MaxClientUsers = newPlanDetails.MaxClientUsers;
                entity.CreatedBy = "system";
                entity.CreatedOn = DateTime.UtcNow;

                _companyplanRepository.CreateEntity(entity);
                return _companyplanRepository.SaveChanges();
            }
            catch (StripeException e)
            {
                return false;
            }
        }


        #endregion



        #region OVERRIDDEN IMPLEMENTATION

        public override Dictionary<string, PropertyMappingValue> GetPropertyMapping()
        {
            return new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "Id", new PropertyMappingValue(new List<string>() { "Id" } ) },
                        { "CompanyId", new PropertyMappingValue(new List<string>() { "CompanyId" } )},
                        { "UpdatedOn", new PropertyMappingValue(new List<string>() { "UpdatedOn" } )},
                    };
        }

        public override string GetDefaultOrderByColumn()
        {
            return "UpdatedOn";
        }

        public override string GetDefaultFieldsToSelect()
        {
            return "Id,CompanyId,DefaultPlanId,ExpiredOn,PaymentProfileId,Active,MaxProjects,MaxTeamUsers,MaxClientUsers,MaxKeywordsPerProject,CreatedOn,IsDowngradeAppsumo";
        }

        #endregion
    }
}
