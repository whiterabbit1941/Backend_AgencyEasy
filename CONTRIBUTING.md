# Contributing to AgencyEasy Backend

Thank you for your interest in contributing to AgencyEasy! This document provides guidelines and instructions for contributing to the backend repository.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Submitting Changes](#submitting-changes)
- [Security](#security)

## Code of Conduct

By participating in this project, you agree to maintain a respectful and inclusive environment for all contributors.

## Getting Started

### Prerequisites

Before contributing, ensure you have completed the setup instructions in [SETUP.md](SETUP.md).

### Repository Structure

AgencyEasy consists of **two separate repositories**:
- **Backend** (this repository): `Backend_AgencyEasy`
- **Frontend** (companion repository): `AgencyEasy`

When contributing features that span both frontend and backend, coordinate changes across both repositories.

### Fork and Clone

1. Fork both repositories to your GitHub account
2. Clone your forks locally:
   ```bash
   mkdir agencyeasy-platform
   cd agencyeasy-platform

   git clone https://github.com/YOUR-USERNAME/Backend_AgencyEasy.git
   git clone https://github.com/YOUR-USERNAME/AgencyEasy.git
   ```

3. Add upstream remotes:
   ```bash
   cd Backend_AgencyEasy
   git remote add upstream https://github.com/ORIGINAL-OWNER/Backend_AgencyEasy.git

   cd ../AgencyEasy
   git remote add upstream https://github.com/ORIGINAL-OWNER/AgencyEasy.git
   ```

## Development Workflow

### Creating a Feature Branch

```bash
# Update your local main branch
git checkout main
git pull upstream main

# Create a feature branch
git checkout -b feature/your-feature-name
```

### Branch Naming Conventions

- `feature/` - New features (e.g., `feature/add-tiktok-integration`)
- `fix/` - Bug fixes (e.g., `fix/stripe-webhook-error`)
- `refactor/` - Code refactoring (e.g., `refactor/simplify-auth-service`)
- `docs/` - Documentation updates (e.g., `docs/update-api-documentation`)
- `test/` - Adding or updating tests (e.g., `test/add-campaign-service-tests`)

### Making Changes

1. Make your changes in your feature branch
2. Test your changes thoroughly (see [Testing](#testing))
3. Ensure your code follows the coding standards
4. Commit your changes with clear commit messages

## Coding Standards

### C# Coding Conventions

Follow Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions):

- Use PascalCase for class names and method names
- Use camelCase for local variables and method parameters
- Use meaningful and descriptive names
- Add XML documentation comments for public APIs

### Code Organization

```
EventManagement.*/
├── Controllers/          # API controllers (thin, delegate to services)
├── Services/            # Business logic (service layer)
├── Repository/          # Data access layer
├── Dto/                 # Data transfer objects
└── Domain/              # Domain entities
```

### Best Practices

- **Keep controllers thin**: Business logic belongs in service classes
- **Use dependency injection**: Register services in `Startup.cs`
- **Async/await**: Use async operations for I/O-bound work
- **Error handling**: Use try-catch blocks and return appropriate HTTP status codes
- **Configuration**: Never hardcode API keys or credentials - use configuration
- **Logging**: Add appropriate logging for debugging and monitoring

### Security Guidelines

⚠️ **CRITICAL**: Never commit sensitive data

- **NO hardcoded API keys or credentials**
- **NO connection strings** with actual passwords
- **NO email addresses** (use configuration instead)
- **NO URLs** pointing to actual production/test environments
- Use `_configuration["KeyName"]` for all sensitive values
- Add sensitive files to `.gitignore`

## Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests for a specific project
dotnet test EventManagement.Tests/
```

### Writing Tests

- Write unit tests for new service methods
- Write integration tests for API endpoints
- Aim for meaningful test coverage, not just high percentages
- Use descriptive test method names: `MethodName_Scenario_ExpectedBehavior`

Example:
```csharp
[Fact]
public async Task CreateCampaign_WithValidData_ReturnsCampaignDto()
{
    // Arrange
    var service = new CampaignService(...);
    var campaignDto = new CampaignDto { Name = "Test Campaign" };

    // Act
    var result = await service.CreateCampaign(campaignDto);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Test Campaign", result.Name);
}
```

## Submitting Changes

### Commit Messages

Write clear, concise commit messages:

```
Add Google Search Console integration

- Implement OAuth flow for GSC authentication
- Add service methods for fetching search analytics
- Update configuration to include GSC client credentials
- Add API endpoints for GSC data retrieval

Fixes #123
```

Format:
- **First line**: Summary (50 characters or less)
- **Blank line**
- **Body**: Detailed explanation of what and why (wrap at 72 characters)
- **Footer**: Reference related issues

### Pull Request Process

1. **Update documentation**: If you change APIs, update relevant documentation
2. **Update tests**: Add or update tests for your changes
3. **Verify no secrets**: Double-check that no API keys or credentials are included
4. **Sync with main**: Rebase your branch on the latest main branch
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

5. **Push to your fork**:
   ```bash
   git push origin feature/your-feature-name
   ```

6. **Create Pull Request**:
   - Go to the original repository on GitHub
   - Click "New Pull Request"
   - Select your fork and branch
   - Fill in the PR template with:
     - Description of changes
     - Related issue numbers
     - Testing performed
     - Screenshots (if UI changes in frontend)
     - Breaking changes (if any)

### Pull Request Template

```markdown
## Description
Brief description of what this PR does

## Related Issues
Fixes #123
Related to #456

## Type of Change
- [ ] Bug fix (non-breaking change that fixes an issue)
- [ ] New feature (non-breaking change that adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update

## Testing Performed
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing completed
- [ ] Tested with frontend integration (if applicable)

## Checklist
- [ ] My code follows the coding standards of this project
- [ ] I have performed a self-review of my code
- [ ] I have commented my code, particularly in hard-to-understand areas
- [ ] I have updated the documentation accordingly
- [ ] My changes generate no new warnings
- [ ] I have added tests that prove my fix is effective or that my feature works
- [ ] New and existing unit tests pass locally with my changes
- [ ] I have NOT committed any API keys, credentials, or sensitive data
- [ ] I have checked that configuration uses `_configuration["KeyName"]` instead of hardcoded values

## Frontend Changes
If this PR requires corresponding frontend changes:
- [ ] Frontend PR link: [Link to frontend PR]
- [ ] Frontend changes tested with these backend changes
```

### Code Review Process

- All submissions require review from at least one maintainer
- Address review feedback promptly
- Be respectful and constructive in discussions
- Update your PR based on feedback
- Once approved, a maintainer will merge your PR

## Security

### Reporting Security Vulnerabilities

🔒 **DO NOT** open public issues for security vulnerabilities.

Instead, email security concerns to: [your-security-email@example.com]

Include:
- Description of the vulnerability
- Steps to reproduce
- Potential impact
- Suggested fix (if any)

### Security Checklist for Contributors

Before submitting a PR:
- [ ] No API keys or credentials in code
- [ ] No connection strings with actual passwords
- [ ] No hardcoded URLs pointing to real services
- [ ] Sensitive configuration uses `_configuration["KeyName"]`
- [ ] No email addresses hardcoded in source code
- [ ] SQL queries use parameterized queries (prevent SQL injection)
- [ ] Input validation implemented for user-provided data
- [ ] Authentication/Authorization properly implemented for new endpoints

## Questions?

If you have questions about contributing:
- Check the [SETUP.md](SETUP.md) for setup instructions
- Review existing issues and pull requests
- Open a new issue with the "question" label

## License

By contributing to AgencyEasy, you agree that your contributions will be licensed under the same license as the project.

---

Thank you for contributing to AgencyEasy! 🎉
