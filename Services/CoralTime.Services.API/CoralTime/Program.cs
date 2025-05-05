using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OData.Edm;
using NLog.Web;
using CoralTime.BL.Interfaces;
using CoralTime.BL.Services;
using CoralTime.BL.Services.Notifications;
using CoralTime.BL.Services.Reports.DropDownsAndGrid;
using CoralTime.BL.Services.Reports.Export;
using CoralTime.Common.Constants;
using CoralTime.Common.Middlewares;
using CoralTime.DAL;
using CoralTime.DAL.Helpers;
using CoralTime.DAL.Models;
using CoralTime.Services.API.Services;
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
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;
using CoralTime.BL.Interfaces.Reports;
using CoralTime.Common.Attributes;
using CoralTime.DAL.Repositories;
using Microsoft.OData.ModelBuilder;
using CoralTime.Services.API;
using CoralTime.ViewModels.JiraSettings;
using Duende.IdentityServer.Configuration;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("defaultDbData.json", optional: true)
    .AddEnvironmentVariables();

builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Host.UseNLog();


bool.TryParse(builder.Configuration["UseMySql"], out var useMySql);
if (useMySql)
{
    builder.Services.AddDbContextPool<AppDbContext>(options =>
        options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnectionMySQL"),
        new MySqlServerVersion(new Version(8, 0, 21)),
        b => b.MigrationsAssembly("CoralTime.MySqlMigrations")));
}
else
{
    builder.Services.AddDbContextPool<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

IdentityModelEventSource.ShowPII = true;
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var isDemo = bool.Parse(builder.Configuration["DemoSiteMode"]);
var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuer = builder.Configuration["Authority"],
    ValidateAudience = true,
    ValidAudience = "WebAPI",
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero
};

if (isDemo)
{
    builder.Services.AddIdentityServer()
        .AddDeveloperSigningCredential()
        .AddInMemoryApiScopes(Config.ApiScopes)
        .AddInMemoryIdentityResources(Config.GetIdentityResources())
        .AddInMemoryApiResources(Config.GetApiResources())
        .AddInMemoryClients(Config.GetClients(builder.Configuration))
        .AddAspNetIdentity<ApplicationUser>()
        .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
        .AddProfileService<IdentityWithAdditionalClaimsProfileService>();
}
else
{
    builder.Services.AddDataProtection()
      .PersistKeysToDbContext<AppDbContext>()
      .SetApplicationName("coraltime");

    builder.Services.AddIdentityServer(options =>
    {
        options.KeyManagement.Enabled = true;
        options.KeyManagement.SigningAlgorithms = new[]
        {
            new SigningAlgorithmOptions
            {
                Name = "RS256"
            }
        };
        options.KeyManagement.PropagationTime = TimeSpan.FromMinutes(1);
        options.KeyManagement.RotationInterval = TimeSpan.FromDays(30);
    })
    .AddKeyManagement()
    .AddInMemoryIdentityResources(Config.GetIdentityResources())
    .AddInMemoryApiResources(Config.GetApiResources())
    .AddInMemoryClients(Config.GetClients(builder.Configuration))
    .AddAspNetIdentity<ApplicationUser>()
    .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
    .AddProfileService<IdentityWithAdditionalClaimsProfileService>()
    .AddOperationalStore<AppDbContext>(options =>
    {
        options.EnableTokenCleanup = true;
    });

    tokenValidationParameters.ValidateIssuerSigningKey = true;
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultForbidScheme = "Identity.Application";
}).AddJwtBearer(options =>
{
    options.Audience = "WebAPI";
    options.Authority = builder.Configuration["Authority"];
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = tokenValidationParameters;
});

builder.Services.AddAuthorization(options =>
{
    foreach (var policyName in Constants.ApplicationsPolicies)
    {
        var policyRoles = Constants.RolePolicies
            .Where(x => x.Value.Contains(policyName))
            .Select(x => x.Key)
            .ToArray();
        options.AddPolicy(policyName, policy =>
        {
            policy.RequireClaim(ClaimTypes.Role, policyRoles);
        });
    }
});

builder.Services.AddSingleton<IConfiguration>(sp => builder.Configuration);
builder.Services.AddScoped<BaseService>();
builder.Services.AddScoped<UnitOfWork>();
builder.Services.AddScoped<AppDbContext>();

builder.Services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>();
builder.Services.AddTransient<Duende.IdentityServer.Services.IProfileService, IdentityWithAdditionalClaimsProfileService>();
builder.Services.AddTransient<IExtensionGrantValidator, AzureGrant>();
builder.Services.AddTransient<IPersistedGrantStore, PersistedGrantStore>();

builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IMemberProjectRoleService, MemberProjectRoleService>();
builder.Services.AddScoped<INotificationService, NotificationsService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITasksService, TasksService>();
builder.Services.AddScoped<ITimeEntryService, TimeEntryService>();
builder.Services.AddScoped<IReportsService, ReportsService>();
builder.Services.AddScoped<IReportExportService, ReportsExportService>();
builder.Services.AddScoped<IReportsSettingsService, ReportsSettingsService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<CheckSecureHeaderServiceFilter>();
builder.Services.AddScoped<CheckSecureHeaderNotificationFilter>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IMemberActionService, MemberActionService>();
builder.Services.AddScoped<IVstsService, VstsService>();
builder.Services.AddScoped<IVstsAdminService, VstsService>();
builder.Services.AddScoped<IJiraServices, JiraServices>();
builder.Services.AddScoped<IJiraProjectService, JiraProjectService>();
builder.Services.AddScoped<IJiraWorklogSerivce, JiraWokrlogService>();

builder.Services.AddMemoryCache();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddControllers()
    .AddOData(opt => opt.AddRouteComponents("/odata", GetEdmModel()).EnableQueryFeatures(100));

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "CoralTime", Version = "v1" });
});

builder.Services.Configure<IdentityOptions>(options =>
{
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseHttpsRedirection();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (builder.Configuration.GetValue<string>("ApplicationInsights:InstrumentationKey") is null)
{
    var telemetryConfig = app.Services.GetService<Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration>();
    if (telemetryConfig is not null)
    {
        telemetryConfig.DisableTelemetry = true;
    }
}

app.Use(async (context, next) =>
{
    await next();
});

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "StaticFiles")),
    RequestPath = "/StaticFiles"
});

app.UseIdentityServer();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseCors("AllowAllOrigins");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    // endpoints.MapODataRoute("ODataRouteComponent", "odata", GetEdmModel());
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoralTime V1");
});

Constants.EnvName = env.EnvironmentName;

//CombineFileWkhtmltopdf(env);

using (var scope = app.Services.CreateScope())
{
    AppDbContext.InitializeFirstTimeDataBaseAsync(scope.ServiceProvider, builder.Configuration).Wait();
}

app.Run();


IEdmModel GetEdmModel()
{
    var odataBuilder = new ODataConventionModelBuilder();
    odataBuilder.EntitySet<ClientView>("Clients");
    odataBuilder.EntitySet<ProjectView>("Projects");
    odataBuilder.EntitySet<MemberView>("Members");
    odataBuilder.EntitySet<MemberProjectRoleView>("MemberProjectRoles");
    odataBuilder.EntitySet<ProjectRoleView>("ProjectRoles");
    odataBuilder.EntitySet<TaskTypeView>("Tasks");
    odataBuilder.EntitySet<ErrorODataView>("Errors");
    odataBuilder.EntitySet<SettingsView>("Settings");
    odataBuilder.EntitySet<ManagerProjectsView>("ManagerProjects");
    odataBuilder.EntitySet<ProjectNameView>("ProjectsNames");
    odataBuilder.EntitySet<MemberActionView>("MemberActions");
    odataBuilder.EntitySet<VstsProjectIntegrationView>("VstsProjectIntegration");
    odataBuilder.EntitySet<JiraSettingsView>("GetAssignedUsers");
    odataBuilder.EnableLowerCamelCase();
    return odataBuilder.GetEdmModel();
}

void CombineFileWkhtmltopdf(IWebHostEnvironment environment)
{
    var fileNameWkhtmltopdf = "wkhtmltopdf.exe";
    var contentRoot = environment.ContentRootPath;
    var pathContentPDF = Path.Combine(contentRoot, "Content", "PDF");
    var pathContentPDFSplitFile = Path.Combine(pathContentPDF, "SplitFileWkhtmltopdf");

    if (!File.Exists(Path.Combine(pathContentPDF, fileNameWkhtmltopdf)))
    {
        var filePattern = "*.0**";
        var destFile = $"../{fileNameWkhtmltopdf}";
        var cmd = new ProcessStartInfo("cmd.exe", $@"/c copy /y /b {filePattern} {destFile}")
        {
            WorkingDirectory = pathContentPDFSplitFile,
            UseShellExecute = false
        };
        Process.Start(cmd);
    }
}
