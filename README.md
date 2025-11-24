# AgencyEasy Backend

A comprehensive backend solution for managing multi-platform advertising campaigns, analytics, and reporting.

## Overview

AgencyEasy is a powerful platform that integrates with multiple advertising and analytics platforms to provide unified campaign management, reporting, and analytics capabilities.

### 📦 Repository Structure

This project consists of **two separate repositories** that must work together:

- **Backend (This Repository)**: `Backend_AgencyEasy` - RESTful API built with .NET Core 3.1
- **Frontend (Companion Repository)**: `AgencyEasy` - React/Next.js frontend application

> ⚠️ **Important**: Both repositories are required for full functionality. Ensure you clone and set up both the backend and frontend projects. The frontend should be located at `../AgencyEasy` relative to this backend directory.

### Supported Integrations

- **Advertising Platforms:**
  - Google Ads
  - Facebook Ads
  - LinkedIn Ads
  - Microsoft Advertising (Bing Ads)

- **Analytics:**
  - Google Analytics (Universal Analytics & GA4)
  - Google Search Console
  - Instagram Insights

- **Other Integrations:**
  - Stripe (Payment Processing)
  - Mailchimp (Email Marketing)
  - GoHighLevel (CRM)
  - WooCommerce
  - CallRail

## Features

- Multi-platform campaign management
- Automated reporting and scheduling
- PDF report generation
- Email notifications via SendGrid
- SEO keyword ranking tracking
- Payment processing and subscription management
- OAuth integration for platform connections
- Identity Server authentication

## Technology Stack

- **.NET Core 3.1**
- **Entity Framework Core**
- **IdentityServer4** (Authentication & Authorization)
- **SQL Server**
- **RESTful API Architecture**

## Getting Started

### Prerequisites

- .NET Core 3.1 SDK or later
- SQL Server (Local or Remote)
- Visual Studio 2019+ or Visual Studio Code

### Quick Start

1. **Clone both repositories**
   ```bash
   # Create a parent directory for both projects
   mkdir agencyeasy-platform
   cd agencyeasy-platform

   # Clone the backend
   git clone <backend-repository-url> Backend_AgencyEasy

   # Clone the frontend
   git clone <frontend-repository-url> AgencyEasy

   # Your directory structure should look like:
   # agencyeasy-platform/
   # ├── Backend_AgencyEasy/  (this repository)
   # └── AgencyEasy/          (frontend repository)
   ```

2. **Set up backend configuration**

   Copy the environment example file and configure your settings:
   ```bash
   cd Backend_AgencyEasy
   cp .env.example .env
   ```

   See [SETUP.md](SETUP.md) for detailed configuration instructions.

3. **Configure database connection**

   Update your `appsettings.Development.json` with your database connection string.

4. **Run database migrations**
   ```bash
   cd EventManagement.API
   dotnet ef database update
   ```

5. **Run the backend application**
   ```bash
   dotnet run
   ```

6. **Set up and run the frontend**

   Refer to the frontend repository's README for setup instructions.

## Documentation

- **[SETUP.md](SETUP.md)** - Complete setup and configuration guide
- **[.env.example](.env.example)** - Environment variables template
- **[SECURITY_CLEANUP_SUMMARY.md](SECURITY_CLEANUP_SUMMARY.md)** - Security cleanup documentation

## Project Structure

### Backend Structure
```
Backend_AgencyEasy/
├── EventManagement.API/          # Main API project
├── EventManagement.IDP/          # Identity Server project
├── EventManagement.Service/      # Business logic layer
├── EventManagement.Repository/   # Data access layer
├── EventManagement.Domain/       # Domain models
├── EventManagement.Dto/          # Data transfer objects
├── EventManagement.Utility/      # Utility classes
└── EventManagement.WebJob/       # Background jobs (Azure WebJobs)
```

### Full Application Architecture

The AgencyEasy application consists of two synchronized components:

#### Backend (This Repository) - `Backend_AgencyEasy/`
- **Framework**: .NET Core 3.1 with C#
- **Purpose**: RESTful API, authentication, business logic, data persistence
- **Default Port**: `https://localhost:44357` (API), `https://localhost:44349` (Identity Server)
- **API Documentation**: Available at `/swagger` endpoint

#### Frontend (Companion Repository) - `AgencyEasy/`
- **Location**: Should be cloned to `../AgencyEasy` relative to backend
- **Purpose**: User interface, dashboard, campaign management UI
- **Default Port**: `https://localhost:3000` (typically)
- **API Connection**: Configured to connect to backend at `https://localhost:44357`

#### Communication Flow
```
User Browser (localhost:3000)
    ↓
Frontend (React/Next.js)
    ↓ [HTTP/REST]
Backend API (localhost:44357)
    ↓
Identity Server (localhost:44349) ← Authentication
    ↓
SQL Server Database
    ↓
External APIs (Google, Facebook, etc.)
```

Both repositories must be running simultaneously for the application to function properly.

## Configuration

This application requires API keys and credentials from various third-party services. See [SETUP.md](SETUP.md) for:

- Complete list of required API keys
- Sign-up instructions for each service
- Configuration guide
- Security best practices

### Required Services

The application integrates with 15+ external services. You'll need to obtain API credentials from:

- AWS (S3, CloudFront, Amplify)
- Google (Ads, Analytics, APIs)
- Facebook (Ads API)
- LinkedIn (Ads API)
- Microsoft (Advertising API)
- Stripe (Payment processing)
- SendGrid (Email delivery)
- Mailchimp (Email marketing)
- And more...

See [SETUP.md](SETUP.md) for complete details.

## API Documentation

Once the application is running, access the Swagger UI documentation at:

```
https://localhost:44357/swagger
```

## Security

⚠️ **Important Security Notes:**

- Never commit API keys or sensitive credentials to version control
- Use environment variables or secure configuration management
- Rotate API keys regularly
- Follow the security checklist in [SETUP.md](SETUP.md)

## Contributing

We welcome contributions! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Setup

See [SETUP.md](SETUP.md) for detailed development environment setup instructions.

## License

[Specify your license here]

## Support

For setup assistance and documentation:
- Review [SETUP.md](SETUP.md)
- Check [SECURITY_CLEANUP_SUMMARY.md](SECURITY_CLEANUP_SUMMARY.md)
- Open an issue on GitHub

## Acknowledgments

This project integrates with many third-party services. Please review their respective terms of service and API usage policies:

- Google APIs: https://developers.google.com/terms
- Facebook Marketing API: https://developers.facebook.com/terms
- LinkedIn Marketing API: https://www.linkedin.com/legal/api-terms
- Microsoft Advertising API: https://docs.microsoft.com/en-us/advertising/guides/
- Stripe API: https://stripe.com/legal/ssa
- And others as listed in SETUP.md

---

**Note:** This is an open-source version of the AgencyEasy backend. All sensitive credentials and API keys have been removed. You must provide your own credentials to run this application.
