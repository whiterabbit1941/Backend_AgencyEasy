using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.Services;
using Joonasw.AspNetCore.SecurityHeaders;
using Marvin.IDP.Entities;
using Marvin.IDP.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Marvin.IDP
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {         
            services.AddMvc();

            var connectionString = Configuration["connectionStrings:DefaultConnection"];
            services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(connectionString));
            var connectionStringAccount = Configuration["connectionStrings:DefaultConnection"];
            services.AddDbContext<ApplicationAccountDbContext>(o => o.UseLazyLoadingProxies()
                                                                   .UseSqlServer(connectionString));
            
            services.ConfigureApplicationCookie(options =>
            {
                options.CookieManager = new BackPortedChunkingCookieManager();
            });

            services.AddScoped<IMarvinUserRepository, MarvinUserRepository>();
            services.AddScoped<IApplicationAccountRepository, ApplicationAccountRepository>();

            services.AddHttpsRedirection(options => options.HttpsPort = 443);

            //var identityServerDataDBConnectionString =
            // Configuration["connectionStrings:identityServerDataDBConnectionString"];

            var migrationsAssembly = typeof(Startup)
                .GetTypeInfo().Assembly.GetName().Name;

            services.AddDbContext<ApplicationDbContext>(builder =>
       builder.UseSqlServer(connectionString, sqlOptions => sqlOptions.MigrationsAssembly(migrationsAssembly)));

            services.AddIdentity<User, IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

            services.Configure<DataProtectionTokenProviderOptions>(options =>

                options.TokenLifespan = TimeSpan.FromHours(3)
            );

            services.AddIdentityServer(
                options =>
            {
                options.IssuerUri = Configuration["IdentityServerUrl"];
                options.PublicOrigin = options.IssuerUri;
            }
            )
             .AddDeveloperSigningCredential()
             //.AddTestUsers(Config.GetUsers())
             //.AddMarvinUserStore()

             .AddAspNetIdentity<User>()
             .AddProfileService<MarvinUserProfileService>()
            //  .AddProfileService<MarvinUserProfileService>()

            // this adds the config data from DB (clients, resources)
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = b =>
                    b.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
            })

            // this adds the operational data from DB (codes, tokens, consents)
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = b =>
                    b.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly(migrationsAssembly));

                // this enables automatic token cleanup. this is optional.
                options.EnableTokenCleanup = true;
                options.TokenCleanupInterval = 30;
            });
            services.Configure<SecurityStampValidatorOptions>(options =>
                    options.ValidationInterval = TimeSpan.FromSeconds(10));

            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            ConfigurationDbContext configurationDbContext,
            PersistedGrantDbContext persistedGrantDbContext,
            ApplicationDbContext applicationDbContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            configurationDbContext.Database.Migrate();
            configurationDbContext.EnsureSeedDataForContext();

            persistedGrantDbContext.Database.Migrate();

            //marvinUserContext.Database.Migrate();
            //marvinUserContext.EnsureSeedDataForContext();

            applicationDbContext.EnsureSeedDataForContext();
            app.UseCors(c => c.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            app.UseIdentityServer();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthorization();
            app.UseEndpoints(routes =>
            {
               routes.MapControllerRoute(
                "default",
                "{controller=home}/{action=Index}/{id?}");
                            });
            app.UseAuthentication();
        }
    }
}
