using CoralTime.BL.Interfaces;
using CoralTime.BL.Interfaces.Reports;
using CoralTime.BL.Services;
using CoralTime.BL.Services.Notifications;
using CoralTime.BL.Services.Reports.DropDownsAndGrid;
using CoralTime.BL.Services.Reports.Export;
using CoralTime.Common.Attributes;
using CoralTime.Common.Constants;
using CoralTime.Common.Middlewares;
using CoralTime.DAL;
using CoralTime.DAL.Helpers;
using CoralTime.DAL.Models;
using CoralTime.DAL.Repositories;
using CoralTime.Services;
using CoralTime.ViewModels.Clients;
using CoralTime.ViewModels.Errors;
using CoralTime.ViewModels.Member;
using CoralTime.ViewModels.MemberActions;
using CoralTime.ViewModels.MemberProjectRoles;
using CoralTime.ViewModels.ProjectRole;
using CoralTime.ViewModels.Projects;
using CoralTime.ViewModels.Settings;
using CoralTime.ViewModels.Tasks;
using CoralTime.ViewModels.Vsts;
using Duende.IdentityServer;
using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.EntityFramework.Interfaces;
using Duende.AccessTokenManagement;
using Duende.IdentityServer.Validation;
using Duende.IdentityServer.Stores;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using static CoralTime.Common.Constants.Constants.Routes.OData;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using Duende.IdentityModel;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CoralTime
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            bool.TryParse(Configuration["UseMySql"], out var useMySql);
            if (useMySql)
            {
                // Add MySQL support (At first create DB on MySQL server.)
                services.AddDbContextPool<AppDbContext>(options =>
                    options.UseMySql(Configuration.GetConnectionString("DefaultConnectionMySQL"),
                    new MySqlServerVersion(new Version(8, 0, 21)),
                    b => b.MigrationsAssembly("CoralTime.MySqlMigrations")));
            }
            else
            {
                // Sql Server
                services.AddDbContextPool<AppDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            }

            IdentityModelEventSource.ShowPII = true;
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    });
            });

            SetupIdentity(services);

            AddApplicationServices(services);
            services.AddMemoryCache();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddControllers().AddOData(opt => opt.AddRouteComponents("/odata", GetEdmModel()).EnableQueryFeatures(100));

            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "CoralTime", Version = "v1" });
            });
        }
        //dell endpointDataSource, Logger
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, EndpointDataSource endpointDataSource, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Disable ApplicationInsights messages if it isn't configured
            var isApplicationInsights = Configuration.GetValue<string>("ApplicationInsights:InstrumentationKey") != null;
            if (!isApplicationInsights)
            {
                var configuration = app.ApplicationServices.GetService<Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration>();
                configuration.DisableTelemetry = true;
            }

            SetupAngularRouting(app);

            app.UseDefaultFiles();

            // Uses static file for the current path.
            app.UseStaticFiles();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Environment.CurrentDirectory, "StaticFiles")),
                RequestPath = "/StaticFiles"
            });

            app.UseIdentityServer();

            // Add middleware exceptions
            app.UseMiddleware<ErrorHandlingMiddleware>();

            //Make sure you add app.UseCors before app.UseRouting otherwise the request will be finished before the CORS middleware is applied
            app.UseCors("AllowAngularApp");

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(static endpoints =>
            {
                endpoints.MapControllers();

                // TODO: review it
                //endpoints.MapODataRouteComponent("ODataRouteComponent", "odata", GetEdmModel());
            });

            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoralTime V1");
            });

            Constants.EnvName = env.EnvironmentName;

            CombineFileWkhtmltopdf(env);

            AppDbContext.InitializeFirstTimeDataBaseAsync(app.ApplicationServices, Configuration).Wait();
        }

        private void AddApplicationServices(IServiceCollection services)
        {
            // Add application services.
            services.AddSingleton<IConfiguration>(sp => Configuration);

            services.AddScoped<BaseService>();

            services.AddScoped<UnitOfWork>();
            services.AddScoped<AppDbContext>();

            services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>();
            services.AddTransient<Duende.IdentityServer.Services.IProfileService, IdentityWithAdditionalClaimsProfileService>();
            services.AddTransient<IExtensionGrantValidator, AzureGrant>();
            services.AddTransient<IPersistedGrantStore, PersistedGrantStore>();

            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IMemberProjectRoleService, MemberProjectRoleService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<INotificationService, NotificationsService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<ITasksService, TasksService>();
            services.AddScoped<ITimeEntryService, TimeEntryService>();
            services.AddScoped<IReportsService, ReportsService>();
            services.AddScoped<IReportExportService, ReportsExportService>();
            services.AddScoped<IReportsSettingsService, ReportsSettingsService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<CheckSecureHeaderServiceFilter>();
            services.AddScoped<CheckSecureHeaderNotificationFilter>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IMemberActionService, MemberActionService>();
            services.AddScoped<IVstsService, VstsService>();
            services.AddScoped<IVstsAdminService, VstsService>();
        }

        private static void SetupAngularRouting(IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.HasValue && null != Constants.AngularRoutes.FirstOrDefault(ar => context.Request.Path.Value.StartsWith(ar, StringComparison.OrdinalIgnoreCase)))
                {
                    context.Request.Path = new PathString("/");

                    context.Response.Headers.Append("Cache-Control", "no-cache, no-store");
                    context.Response.Headers.Append("Expires", "-1");
                }

                await next();
            });
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<ClientView>("Clients");
            builder.EntitySet<ProjectView>("Projects");
            builder.EntitySet<MemberView>("Members");
            builder.EntitySet<MemberProjectRoleView>("MemberProjectRoles");
            builder.EntitySet<ProjectRoleView>("ProjectRoles");
            builder.EntitySet<TaskTypeView>("Tasks");
            builder.EntitySet<ErrorODataView>("Errors");
            builder.EntitySet<SettingsView>("Settings");
            builder.EntitySet<ManagerProjectsView>("ManagerProjects");
            builder.EntitySet<ProjectNameView>("ProjectsNames");
            builder.EntitySet<MemberActionView>("MemberActions");
            builder.EntitySet<VstsProjectIntegrationView>("VstsProjectIntegration");
            builder.EnableLowerCamelCase();
            return builder.GetEdmModel();
        }

        private void SetupIdentity(IServiceCollection services)
        {
            var isDemo = bool.Parse(Configuration["DemoSiteMode"]);

            // Identity options.
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                if (isDemo)
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                }
                else
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                }

                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            });

            var accessTokenLifetime = int.Parse(Configuration["AccessTokenLifetime"]);
            var refreshTokenLifetime = int.Parse(Configuration["RefreshTokenLifetime"]);
            var slidingRefreshTokenLifetime = int.Parse(Configuration["SlidingRefreshTokenLifetime"]);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Configuration["Authority"],
                ValidateAudience = true,
                ValidAudience = "WebAPI",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            if (isDemo)
            {
                services.AddIdentityServer()
                    .AddDeveloperSigningCredential()
                    .AddInMemoryApiScopes(Config.ApiScopes)
                    .AddInMemoryIdentityResources(Config.GetIdentityResources())
                    .AddInMemoryApiResources(Config.GetApiResources())
                    .AddInMemoryClients((Config.GetClients(Configuration)))
                    .AddAspNetIdentity<ApplicationUser>()
                    .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
                    .AddProfileService<IdentityWithAdditionalClaimsProfileService>();
            }
            else
            {
                var cert = X509CertificateLoader.LoadPkcs12FromFile("coraltime.pfx", "", X509KeyStorageFlags.MachineKeySet);

                services.AddIdentityServer()
                    .AddInMemoryIdentityResources(Config.GetIdentityResources())
                    .AddInMemoryApiResources(Config.GetApiResources())
                    .AddInMemoryClients(Config.GetClients(Configuration))
                    .AddAspNetIdentity<ApplicationUser>()
                    .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
                    .AddSigningCredential(cert)
                    .AddProfileService<IdentityWithAdditionalClaimsProfileService>()
                    .AddOperationalStore<AppDbContext>(options =>
                    {
                        options.EnableTokenCleanup = true;
                    }
                    );
                var key = new X509SecurityKey(cert);
                tokenValidationParameters.IssuerSigningKey = key;
                tokenValidationParameters.ValidateIssuerSigningKey = true;
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = "Bearer";
                options.DefaultForbidScheme = "Identity.Application";
            }).AddJwtBearer(options =>
            {
                // name of the API resource
                options.Audience = "WebAPI";
                options.Authority = Configuration["Authority"];
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = tokenValidationParameters;
            });

            services.AddAuthorization(options =>
            {
                foreach (var policyName in Constants.ApplicationsPolicies)
                {
                    var policyRoles = Constants.RolePolicies.Where(x => x.Value.Contains(policyName)).Select(x => x.Key).ToArray();
                    foreach(var role in policyRoles)
                    {
                        options.AddPolicy(policyName, policy =>
                        {
                            policy.RequireClaim(ClaimTypes.Role, role);
                        });
                    }
                }
            });
        }

        private void CombineFileWkhtmltopdf(IWebHostEnvironment environment)
        {
            var fileNameWkhtmltopdf = "wkhtmltopdf.exe";
            var patchContentRoot = environment.ContentRootPath;

            var pathContentPDF = $"{patchContentRoot}\\Content\\PDF";
            var pathContentPDFSplitFile = $"{pathContentPDF}\\SplitFileWkhtmltopdf";

            var fileNotExist = !File.Exists(pathContentPDF + "\\" + fileNameWkhtmltopdf);
            if (fileNotExist)
            {
                var filePattern = "*.0**";
                var destFile = $"\"../{fileNameWkhtmltopdf}\"";

                var cmd = new ProcessStartInfo("cmd.exe", $@"/c copy /y /b {filePattern} {destFile}")
                {
                    WorkingDirectory = pathContentPDFSplitFile,
                    UseShellExecute = false
                };
                Process.Start(cmd);
            }
        }
    }
}
