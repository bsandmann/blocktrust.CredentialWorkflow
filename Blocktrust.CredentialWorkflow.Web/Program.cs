using Blocktrust.CredentialWorkflow.Core;
using Blocktrust.CredentialWorkflow.Core.Entities.Identity;
using Blocktrust.CredentialWorkflow.Core.Services;
using Blocktrust.CredentialWorkflow.Core.Services.Interfaces;
using Blocktrust.CredentialWorkflow.Core.Settings;
using Blocktrust.CredentialWorkflow.Web.Common;
using Blocktrust.CredentialWorkflow.Web.Components.Account;
using Blocktrust.CredentialWorkflow.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add controller support
builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AppStateService>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<ClipboardService>();
builder.Services.AddScoped<WorkflowChangeTrackerService>();
builder.Services.AddScoped<ISchemaValidationService, SchemaValidationService>();

// Add Core Services
builder.Services.AddScoped<ICredentialService, CredentialService>();
builder.Services.AddScoped<IDidResolutionService, DidResolutionService>();
builder.Services.AddScoped<IDeliveryService, DeliveryService>();

// Configure strongly typed settings
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<CredentialSettings>(
    builder.Configuration.GetSection("CredentialSettings"));

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContextFactory<DataContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddSignInManager()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddTransient<IEmailSender, SendGridEmailSender>();
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, ApplicationUserEmailSender>();

// Add MediatR with all handlers
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly); // Web project handlers
    cfg.RegisterServicesFromAssembly(typeof(DataContext).Assembly); // Core project handlers
});

builder.Services.AddAntiforgery();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

app.Run();