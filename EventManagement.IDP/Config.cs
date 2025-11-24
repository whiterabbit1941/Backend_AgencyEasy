using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Security.Claims;

namespace Marvin.IDP
{
    // ⚠️ SECURITY WARNING ⚠️
    // This configuration file contains test users with WEAK PASSWORDS for development/testing
    // NEVER use these test users in production environments
    // Consider using a proper user management system with secure password requirements in production
    public static class Config
    {
        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "fec0a4d6-5830-4eb8-8024-272bd5d6d2bb",
                    Username = "Kevin",
                    Password = "password",
                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Kevin"),
                        new Claim("family_name", "Dockx"),
                        new Claim("role", "Administrator"),
                    }
                },
                new TestUser
                {
                    SubjectId = "c3b7f625-c07f-4d7d-9be1-ddff8ff93b4d",
                    Username = "Sven",
                    Password = "password",
                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Sven"),
                        new Claim("family_name", "Vercauteren"),
                        new Claim("role", "Tour Manager"),
                    }
                }
            };
        }

        public static List<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
               new IdentityResources.OpenId(),
               new IdentityResources.Profile(),
               new IdentityResource("roles", "Your role(s)", new []{"role"}),
            };
        }

        internal static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("tourmanagementapi", "Tour Management API", new[] { "role" })

                {
                    ApiSecrets = { new Secret ("secret".Sha256()) }
                }
            };
        }

        public static List<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientName = "Tour Management",
                    ClientId="tourmanagementclient",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequireConsent = false,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris =new List<string>
                    {
                        "https://localhost:3000/signin-oidc",
                        "https://localhost:3000/redirect-silentrenew"
                    },
                    AccessTokenLifetime = 180,
                    PostLogoutRedirectUris = new[]{
                        "https://localhost:3000/" },
                    AllowedScopes = new []
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "roles",
                        "tourmanagementapi",
                    }
                },
                 new Client
                {
                    ClientName = "Swagger UI",
                    ClientId="swaggerui",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    RequireConsent = false,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris =new List<string>
                    {
                        "https://localhost:44357/oauth2-redirect.html"
                    },
                    AllowedScopes = new []
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "roles",
                        "tourmanagementapi",
                    }
                }
            };
        }
    }
}
