using CoralTime.BL.Interfaces;
using CoralTime.BL.Interfaces.Reports;
using CoralTime.BL.Services;
using CoralTime.BL.Services.Notifications;
using CoralTime.BL.Services.Reports.DropDownsAndGrid;
using CoralTime.BL.Services.Reports.Export;
using CoralTime.Common.Attributes;
using CoralTime.Common.Constants;
using CoralTime.Common.Middlewares;
using CoralTime.Common.Services;
using CoralTime.DAL;
using CoralTime.DAL.Helpers;
using CoralTime.DAL.Models;
using CoralTime.DAL.Models.Jira;
using CoralTime.DAL.Repositories;
using CoralTime.Services.API;
using CoralTime.Services.API.Services;
using CoralTime.ViewModels.Clients;
using CoralTime.ViewModels.Errors;
using CoralTime.ViewModels.Jira;
using CoralTime.ViewModels.JiraSettings;
using CoralTime.ViewModels.Member;
using CoralTime.ViewModels.MemberActions;
using CoralTime.ViewModels.MemberProjectRoles;
using CoralTime.ViewModels.ProjectRole;
using CoralTime.ViewModels.Projects;
using CoralTime.ViewModels.Settings;
using CoralTime.ViewModels.Tasks;
using CoralTime.ViewModels.Vsts;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using NLog.Web;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using ODataRoutes = CoralTime.Common.Constants.Constants.Routes.OData;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("defaultDbData.json", optional: true)
    .AddEnvironmentVariables();

builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Host.UseNLog();

#if DEBUG
builder.Logging.AddDebug();
#endif


bool.TryParse(builder.Configuration["UseMySql"], out var useMySql);
if (useMySql)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnectionMySQL"),
        new MySqlServerVersion(new Version(8, 0, 21)),
        b => b.MigrationsAssembly("CoralTime.MySqlMigrations")));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
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
var authority = builder.Configuration["Authority"];
var additionalIssuers = builder.Configuration.GetSection("ValidIssuers").Get<string[]>() ?? Array.Empty<string>();
var tokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuers = new[] { authority }.Concat(additionalIssuers),
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
    .AddInMemoryApiScopes(Config.ApiScopes)
    .AddInMemoryApiResources(Config.GetApiResources())
    .AddInMemoryClients(Config.GetClients(builder.Configuration))
    .AddAspNetIdentity<ApplicationUser>()
    .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
    .AddProfileService<IdentityWithAdditionalClaimsProfileService>()
    .AddOperationalStore<AppDbContext>(options =>
    {
        options.EnableTokenCleanup = true;
    })
    .AddExtensionGrantValidator<AzureGrant>();

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
        .AddOData(opt => opt.AddRouteComponents(ODataRoutes.BaseODataApiRoute, GetEdmModel()).EnableQueryFeatures(100));


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

app.MapFallbackToFile("index.html");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoralTime V1");
});

Constants.EnvName = env.EnvironmentName;

//CombineFileWkhtmltopdf(env);

var dbContextInitializer = new DbContextInitializer();
dbContextInitializer.InitializeFirstTimeDataBaseAsync(app.Services, builder.Configuration).Wait();

app.Run();


IEdmModel GetEdmModel()
{
    var odataBuilder = new ODataConventionModelBuilder();
    odataBuilder.EntitySet<ProjectView>("Projects");
    odataBuilder.EntitySet<MemberView>("Members");
    odataBuilder.EntitySet<MemberView>("MemberViews");
    odataBuilder.EntitySet<ProjectRoleView>("ProjectRoles");
    odataBuilder.EntitySet<ErrorODataView>("Errors");
    odataBuilder.EntitySet<TaskTypeView>("Tasks");
    odataBuilder.EntitySet<SettingsView>("Settings");
    odataBuilder.EntitySet<ManagerProjectsView>("ManagerProjects");
    odataBuilder.EntitySet<ProjectNameView>("ProjectsNames");
    odataBuilder.EntitySet<MemberActionView>("MemberActions");
    odataBuilder.EntitySet<VstsProjectIntegrationView>("VstsProjectIntegration");
    odataBuilder.EntitySet<JiraProject>("JiraProject");
    odataBuilder.EntitySet<JiraProjectView>("JiraProjectViews");
    odataBuilder.EntitySet<JiraProjectLinkedView>("JiraProjectLinkedViews");
    odataBuilder.EntitySet<JiraSetting>("Jira");
    odataBuilder.EntitySet<JiraSettingsView>("JiraSettingsViews");

    RegisterODataFunctions(odataBuilder);

    odataBuilder.EnableLowerCamelCase();
    return odataBuilder.GetEdmModel();
}

void RegisterODataFunctions(ODataConventionModelBuilder odataBuilder)
{
    // JiraProject Controller
    var jiraProject = odataBuilder.EntitySet<JiraProject>("JiraProject");

    var getJiraProj = jiraProject.EntityType.Collection
        .Function(ODataRoutes.GetAllJiraProjectsBySettingId);
    getJiraProj.Parameter<int>(ODataRoutes.IdParam);
    getJiraProj.ReturnsCollectionFromEntitySet<JiraProject>("JiraProject");

    var getUnAssign = jiraProject.EntityType.Collection
        .Function(ODataRoutes.GetUnAssignJiraProject);
    getUnAssign.Parameter<int>(ODataRoutes.IdParam);
    getUnAssign.ReturnsCollectionFromEntitySet<JiraProjectView>("JiraProjectViews");

    var getAssign = jiraProject.EntityType.Collection
        .Function(ODataRoutes.GetAssignJiraProject);
    getAssign.Parameter<int>(ODataRoutes.IdParam);
    getAssign.ReturnsCollectionFromEntitySet<JiraProjectLinkedView>("JiraProjectLinkedViews");

    // Jira Controller
    var jira = odataBuilder.EntitySet<JiraSetting>("Jira");

    var jiraSettings = jira.EntityType.Collection
        .Function(ODataRoutes.GetSettings);
    jiraSettings.ReturnsCollectionFromEntitySet<JiraSettingsView>("JiraSettingsViews");

    var getAssignUs = jira.EntityType.Collection
        .Function(ODataRoutes.GetAssignedUsers);
    getAssignUs.Parameter<int>(ODataRoutes.IdParam);
    getAssignUs.ReturnsCollectionFromEntitySet<MemberView>("MemberViews");

    var getNotAssignUs = jira.EntityType.Collection
        .Function(ODataRoutes.GetNotAssignedUsers);
    getNotAssignUs.Parameter<int>(ODataRoutes.IdParam);
    getNotAssignUs.ReturnsCollectionFromEntitySet<MemberView>("MemberViews");

    // Clients Controller
    var clients = odataBuilder.EntitySet<ClientView>("Clients");

    var getClients = clients.EntityType.Collection
        .Function(ODataRoutes.GetAllClients);
    getClients.ReturnsCollectionFromEntitySet<ClientView>("Clients");

    // Members Controller
    var members = odataBuilder.EntitySet<MemberView>("Members");

    var getMembers = members.EntityType.Collection
        .Function(ODataRoutes.GetAllMembers);
    getMembers.ReturnsCollectionFromEntitySet<MemberView>("Members");

    var getMemberProjects = members.EntityType.Collection
        .Function(ODataRoutes.GetProjects);
    getMemberProjects.Parameter<int>(ODataRoutes.IdParam);
    getMemberProjects.ReturnsCollectionFromEntitySet<ProjectView>("Projects");

    // Projects Controller
    var projects = odataBuilder.EntitySet<ProjectView>("Projects");

    var getProjectMembers = projects.EntityType.Collection
        .Function(ODataRoutes.GetProjectMembers);
    getProjectMembers.Parameter<int>(ODataRoutes.IdParam);
    getProjectMembers.ReturnsCollectionFromEntitySet<MemberView>("Members");

    var getTrackerAllProj = projects.EntityType.Collection
        .Function(ODataRoutes.GetTimeTrackerAllProjects);
    getTrackerAllProj.ReturnsCollectionFromEntitySet<ProjectView>("Projects");

    // MemberProjectRoles Controller
    var memberProjectRoles = odataBuilder.EntitySet<MemberProjectRoleView>("MemberProjectRoles");

    var getMemberProjectRoles = memberProjectRoles.EntityType.Collection
        .Function(ODataRoutes.GetAllMemberProjectRoles);
    getMemberProjectRoles.ReturnsCollectionFromEntitySet<MemberProjectRoleView>("MemberProjectRoles");

    var getMprMembers = memberProjectRoles.EntityType.Collection
        .Function(ODataRoutes.GetNotAssignedProjectMembers);
    getMprMembers.Parameter<int>(ODataRoutes.IdParam);
    getMprMembers.ReturnsCollectionFromEntitySet<MemberView>("Members");

    var getMprProjects = memberProjectRoles.EntityType.Collection
        .Function(ODataRoutes.GetProjects);
    getMprProjects.Parameter<int>(ODataRoutes.IdParam);
    getMprProjects.ReturnsCollectionFromEntitySet<ProjectView>("Projects");

    // ProjectRoles Controller
    var projectRoles = odataBuilder.EntitySet<ProjectRoleView>("ProjectRoles");

    var getProjectRoles = projectRoles.EntityType.Collection
        .Function(ODataRoutes.GetAllProjectRoles);
    getProjectRoles.ReturnsCollectionFromEntitySet<ProjectRoleView>("ProjectRoles");

    // ProjectNames Controller
    var projectNames = odataBuilder.EntitySet<ProjectNameView>("ProjectsNames");

    var getProjectNames = projectNames.EntityType.Collection
        .Function(ODataRoutes.GetAllProjectNames);
    getProjectNames.ReturnsCollectionFromEntitySet<ProjectNameView>("ProjectsNames");

    // ManagerProjects Controller
    var managerProjects = odataBuilder.EntitySet<ManagerProjectsView>("ManagerProjects");

    var getManagerProjects = managerProjects.EntityType.Collection
        .Function(ODataRoutes.GetManageProjectsOfManager);
    getManagerProjects.ReturnsCollectionFromEntitySet<ManagerProjectsView>("ManagerProjects");

    // MemberActions Controller
    var memberActions = odataBuilder.EntitySet<MemberActionView>("MemberActions");

    var getMemberActions = memberActions.EntityType.Collection
        .Function(ODataRoutes.GetAllMemberActions);
    getMemberActions.ReturnsCollectionFromEntitySet<MemberActionView>("MemberActions");

    // Tasks Controller
    var tasks = odataBuilder.EntitySet<TaskTypeView>("Tasks");

    var getTaskTypes = tasks.EntityType.Collection
        .Function(ODataRoutes.GetAllTasks);
    getTaskTypes.ReturnsCollectionFromEntitySet<TaskTypeView>("Tasks");

    // VstsProjectIntegration Controller
    var vstsProjectIntegration = odataBuilder.EntitySet<VstsProjectIntegrationView>("VstsProjectIntegration");

    var getVstsProjects = vstsProjectIntegration.EntityType.Collection
        .Function(ODataRoutes.GetAllVstsProjects);
    getVstsProjects.ReturnsCollectionFromEntitySet<VstsProjectIntegrationView>("VstsProjectIntegration");

    var getVstsMembers = vstsProjectIntegration.EntityType.Collection
        .Function(ODataRoutes.GetVstsProjectMembers);
    getVstsMembers.Parameter<int>(ODataRoutes.IdParam);
    getVstsMembers.ReturnsCollectionFromEntitySet<MemberView>("Members");
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
