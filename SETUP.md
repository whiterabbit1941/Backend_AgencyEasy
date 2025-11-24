# AgencyEasy Backend - Setup Guide

This guide will help you set up the AgencyEasy backend application for development, testing, or production environments.

## 🔄 Backend + Frontend Synchronization

**IMPORTANT:** AgencyEasy consists of **TWO separate repositories** that must be configured to work together:

1. **Backend Repository** (this one): `Backend_AgencyEasy` - RESTful API (.NET Core 3.1)
2. **Frontend Repository**: `AgencyEasy` - User interface (React/Next.js)

### Repository Layout

For proper synchronization, your directory structure should look like this:

```
parent-folder/
├── Backend_AgencyEasy/    ← This backend repository
│   ├── EventManagement.API/
│   ├── EventManagement.IDP/
│   └── ...
└── AgencyEasy/             ← Frontend repository (separate clone)
    ├── src/
    ├── public/
    └── ...
```

### Why This Matters

- Both repositories must be running simultaneously during development
- The frontend connects to the backend API endpoints
- Configuration must be synchronized between both projects
- This guide covers ONLY the backend setup
- Refer to the frontend repository's README for frontend-specific setup

> 💡 **Tip**: Open two terminal windows - one for the backend, one for the frontend - to run both simultaneously.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Initial Setup](#initial-setup)
- [Configuration](#configuration)
- [Required API Keys and Services](#required-api-keys-and-services)
- [Database Setup](#database-setup)
- [Running the Application](#running-the-application)
- [Frontend Setup](#frontend-setup)
- [Deployment](#deployment)

## Prerequisites

Before you begin, ensure you have the following installed:

- **.NET Core 3.1 SDK** or later
- **SQL Server** (Local or Remote)
- **Visual Studio 2019+** or **Visual Studio Code**
- **Git** (for version control)

## Initial Setup

### 1. Clone Both Repositories

```bash
# Create a parent directory for the full platform
mkdir agencyeasy-platform
cd agencyeasy-platform

# Clone the backend repository
git clone <backend-repository-url> Backend_AgencyEasy

# Clone the frontend repository
git clone <frontend-repository-url> AgencyEasy

# Verify your directory structure
ls -la
# You should see both Backend_AgencyEasy/ and AgencyEasy/ directories

# Navigate to the backend directory for the rest of this setup
cd Backend_AgencyEasy
```

### 2. Restore NuGet Packages

```bash
dotnet restore
```

## Configuration

### 1. Environment-Specific Configuration Files

The application uses different configuration files for different environments:

- `appsettings.json` - Base configuration (should not contain sensitive data)
- `appsettings.Development.json` - Development environment configuration
- `appsettings.Test.json` - Test environment configuration
- `appsettings.Live.json` - Production environment configuration

**IMPORTANT:**
- Never commit files containing actual API keys or credentials to version control
- Use the `.example` template files as a reference
- Create your own local configuration files with your actual credentials

### 2. Setting Up Configuration Files

#### For Development Environment:

1. Copy the example file (if available) or use the existing template:
   ```bash
   # The appsettings.Development.json is already in the project but emptied
   # Fill it with your actual values
   ```

2. Update `appsettings.Development.json` with your configuration values

#### For Test/Live Environments:

1. Copy the example files:
   ```bash
   cp EventManagement.API/appsettings.Test.json.example EventManagement.API/appsettings.Test.json
   cp EventManagement.API/appsettings.Live.json.example EventManagement.API/appsettings.Live.json
   ```

2. Fill in your actual credentials and configuration values

## Required API Keys and Services

You will need to obtain API keys and credentials from the following services:

### 1. Database Configuration

**Required:** SQL Server connection string

- **Local Development:** Use Windows Authentication or SQL Server Authentication
- **Remote/Production:** Use SQL Server Authentication with secure credentials

**Configuration Key:** `ConnectionStrings:EventManagementDB`

**Example:**
```json
{
  "ConnectionStrings": {
    "EventManagementDB": "Server=YOUR_SERVER;Database=eintelligence;User Id=YOUR_USER;Password=YOUR_PASSWORD;integrated security=False;MultipleActiveResultSets=False;App=EntityFramework"
  }
}
```

### 2. AWS (Amazon Web Services)

**Required for:** S3 storage, CloudFront distribution, Amplify deployment

- Sign up at: https://aws.amazon.com/
- Create an IAM user with appropriate permissions
- Generate access keys

**Configuration Keys:**
- `AWS:AccessKeyID` - Your AWS Access Key ID
- `AWS:SecretAccessKeyID` - Your AWS Secret Access Key
- `AWS:DistributionId` - CloudFront Distribution ID (if using)
- `AWS:AmplifyAppId` - Amplify App ID (if using)
- `AWS:BranchName` - Amplify branch name (e.g., "Staging_CI_CD")
- `BlobUrl` - S3 bucket URL for file storage

### 3. SendGrid (Email Service)

**Required for:** Sending transactional emails

- Sign up at: https://sendgrid.com/
- Create an API key with Mail Send permissions

**Configuration Keys:**
- `Client` - Your SendGrid API Key
- `MailFrom` - Your verified sender email address

### 4. Google Ads & Analytics

**Required for:** Google Ads integration, Google Analytics reporting

- Create a project in Google Cloud Console: https://console.cloud.google.com/
- Enable Google Ads API and Google Analytics API
- Create OAuth 2.0 credentials
- Apply for Google Ads Developer Token

**Configuration Keys:**
- `ClientIdForGoogleAds` - Google OAuth Client ID
- `ClientSecretForGoogleAds` - Google OAuth Client Secret
- `DeveloperTokenForGoogleAds` - Google Ads Developer Token
- `GoogleApiKey` - Google API Key

**Documentation:**
- Google Ads API: https://developers.google.com/google-ads/api/docs/start
- Google Analytics: https://developers.google.com/analytics

### 5. Facebook Ads

**Required for:** Facebook Ads integration and reporting

- Create an app at: https://developers.facebook.com/
- Add Marketing API product
- Get App ID and App Secret

**Configuration Keys:**
- `FacebookAppId` - Your Facebook App ID
- `FacebookAppSecret` - Your Facebook App Secret

**Documentation:** https://developers.facebook.com/docs/marketing-apis

### 6. LinkedIn Ads

**Required for:** LinkedIn Ads integration

- Create an app at: https://www.linkedin.com/developers/
- Request access to Marketing Developer Platform
- Get Client ID and Client Secret

**Configuration Keys:**
- `LinkedinClientID` - LinkedIn Client ID
- `LinkedinSeceretId` - LinkedIn Client Secret

**Documentation:** https://docs.microsoft.com/en-us/linkedin/marketing/

### 7. Microsoft Advertising

**Required for:** Microsoft Ads (Bing Ads) integration

- Sign up at: https://ads.microsoft.com/
- Create an app in Azure AD: https://portal.azure.com/
- Get Developer Token from Microsoft Advertising

**Configuration Keys:**
- `MicrosoftClientId` - Azure AD Application (client) ID
- `MicrosoftClientSeceret` - Azure AD Client Secret
- `DeveloperToken` - Microsoft Advertising Developer Token

**Documentation:** https://docs.microsoft.com/en-us/advertising/guides/

### 8. Stripe (Payment Processing)

**Required for:** Subscription payments, billing

- Sign up at: https://stripe.com/
- Get API keys from Dashboard
- Create products and price IDs
- Create tax rates (if needed)

**Configuration Keys:**
- `StripeKey` - Publishable key (pk_test_... for test, pk_live_... for production)
- `StripeSecret` - Secret key (sk_test_... for test, sk_live_... for production)
- `StripeTaxId` - Tax rate ID (if using)
- `MarketPlaceProductId` - Product ID for marketplace items

**Stripe Product Price IDs:**
- `StripeSubscriptionPriceId:CustomMonthly`
- `StripeSubscriptionPriceId:CustomYearly`
- `StripeSubscriptionPriceId:AgencyMonthly`
- `StripeSubscriptionPriceId:AgencyYearly`
- `StripeSubscriptionPriceId:StartupMonthly`
- `StripeSubscriptionPriceId:StartupYearly`
- `StripeSubscriptionPriceId:ProfessionalMonthly`
- `StripeSubscriptionPriceId:ProfessionalYearly`

**Stripe Coupon Codes:**
- `StripeCouponCode:20Percent` - Coupon code for 20% discount
- `StripeCouponCode:50Percent` - Coupon code for 50% discount

**Documentation:** https://stripe.com/docs/api

### 9. Mailchimp

**Required for:** Email marketing integration

- Sign up at: https://mailchimp.com/
- Create an API key
- Get List ID from your audience
- Create OAuth app for OAuth integration

**Configuration Keys:**
- `Mailchimp:ApiKey` - Mailchimp API Key
- `Mailchimp:ListId` - Default audience/list ID
- `MailchimpClientId` - OAuth Client ID
- `MailchimpSecretId` - OAuth Client Secret
- `MailchimpRedirectUrl` - OAuth redirect URL (relative path)

**Documentation:** https://mailchimp.com/developer/

### 10. DataForSEO

**Required for:** SEO keyword ranking and SERP data

- Sign up at: https://dataforseo.com/
- Get your login credentials

**Configuration Keys:**
- `DataForSeoLoginV3` - Your DataForSEO login email
- `DataForSeoPasswordV3` - Your DataForSEO API password

**Documentation:** https://docs.dataforseo.com/

### 11. Api2Pdf

**Required for:** PDF generation from HTML

- Sign up at: https://www.api2pdf.com/
- Get your API key

**Configuration Keys:**
- `Api2Pdf` - Your Api2Pdf API Key

**Documentation:** https://www.api2pdf.com/documentation/

### 12. GoHighLevel

**Required for:** GoHighLevel CRM integration

- Sign up at: https://www.gohighlevel.com/
- Get API key from settings

**Configuration Keys:**
- `GoHighLevelApiKey` - Your GoHighLevel API Key

**Documentation:** https://highlevel.stoplight.io/

### 13. OpenAI ChatGPT

**Required for:** AI-powered features using ChatGPT

- Sign up at: https://platform.openai.com/
- Create an API key

**Configuration Keys:**
- `ChatGptApiKey` - Your OpenAI API Key

**Documentation:** https://platform.openai.com/docs/

### 14. Azure Application Insights (Optional)

**Required for:** Application monitoring and telemetry

- Create Application Insights resource in Azure Portal
- Get instrumentation key

**Configuration Keys:**
- `ApplicationInsights:InstrumentationKey` - Application Insights Instrumentation Key

**Documentation:** https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview

### 15. Azure WebJobs (If using WebJobs)

**Required for:** Background job processing

- Create Azure Storage account
- Get connection strings

**Configuration Keys (in app.config):**
- `AzureWebJobsDashboard` - Connection string for WebJobs Dashboard
- `AzureWebJobsStorage` - Connection string for WebJobs Storage

## Database Setup

### 1. Create Database

Create a SQL Server database named `eintelligence` or use the name specified in your connection string.

### 2. Run Migrations

The application uses Entity Framework Core for database management.

```bash
# Navigate to the API project directory
cd EventManagement.API

# Apply migrations
dotnet ef database update
```

**Note:** If migrations are not set up, you may need to create them first:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 3. Identity Server Database

The Identity Server (EventManagement.IDP) also requires database setup:

```bash
cd EventManagement.IDP
dotnet ef database update
```

## Running the Application

### Development Mode

1. **Using Visual Studio:**
   - Open `EventManagement.sln`
   - Set startup projects (right-click solution > Set Startup Projects)
   - Select Multiple startup projects:
     - EventManagement.API
     - EventManagement.IDP
   - Press F5 to run

2. **Using Command Line:**

   ```bash
   # Terminal 1 - Run API
   cd EventManagement.API
   dotnet run

   # Terminal 2 - Run Identity Server
   cd EventManagement.IDP
   dotnet run
   ```

### Default URLs

- **Backend API:** https://localhost:44357/
- **Identity Server:** https://localhost:44349/
- **Frontend:** Refer to the frontend repository's documentation for its URL (typically https://localhost:3000/ or similar)

## Frontend Setup

After setting up the backend, you'll need to set up the frontend application:

1. Navigate to the frontend directory:
   ```bash
   cd ../AgencyEasy
   ```

2. Follow the setup instructions in the frontend repository's README.md

3. Configure the frontend to point to your backend API URL (https://localhost:44357/)

4. Ensure both the backend API and Identity Server are running before starting the frontend application

**Important:** The frontend and backend must be configured to communicate with each other. Make sure:
- The frontend knows the backend API URL
- The backend allows requests from the frontend origin (CORS configuration)
- The Identity Server is properly configured with the frontend's redirect URIs

## Deployment

### Preparation

1. **Update configuration** for your target environment (Test/Live)
2. **Build the application:**
   ```bash
   dotnet build --configuration Release
   ```

3. **Publish the application:**
   ```bash
   dotnet publish --configuration Release --output ./publish
   ```

### Environment-Specific Deployment

Set the `ASPNETCORE_ENVIRONMENT` environment variable:

- `Development` - Uses appsettings.Development.json
- `Test` - Uses appsettings.Test.json
- `Production` or `Live` - Uses appsettings.Production.json or appsettings.Live.json

### Security Checklist

Before deploying to production:

- [ ] All sensitive data removed from configuration files in source control
- [ ] Environment-specific config files properly configured
- [ ] HTTPS enabled for all endpoints
- [ ] Database connection uses encrypted connection
- [ ] All API keys are valid production keys (not test keys)
- [ ] CORS settings properly configured
- [ ] Strong passwords for database and service accounts
- [ ] Regular backups configured

## Troubleshooting

### Common Issues

1. **Database Connection Failed:**
   - Verify connection string is correct
   - Check SQL Server is running and accessible
   - Verify firewall rules allow connection

2. **API Key Errors:**
   - Ensure all required API keys are configured
   - Verify keys are valid and have necessary permissions
   - Check API quotas and limits haven't been exceeded

3. **Migration Errors:**
   - Ensure Entity Framework tools are installed: `dotnet tool install --global dotnet-ef`
   - Check database permissions
   - Review migration files for conflicts

## Support and Documentation

For additional help:

- Check the official .NET documentation: https://docs.microsoft.com/dotnet/
- Review API provider documentation for each integrated service
- Contact your development team

## Environment Variables Reference

See `.env.example` file for a complete list of all environment variables that can be configured.

## License

[Add your license information here]

## Contributing

[Add contribution guidelines if open source]
