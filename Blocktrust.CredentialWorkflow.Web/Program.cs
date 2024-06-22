using Blocktrust.CredentialWorkflow.Core;
using Blocktrust.CredentialWorkflow.Core.Entities.Identity;
using Blocktrust.CredentialWorkflow.Web.Common;
using Blocktrust.CredentialWorkflow.Web.Components.Account;
using Blocktrust.CredentialWorkflow.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to athe container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AppStateService>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<ClipboardService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContextFactory<DataContext>(options =>
    options.UseNpgsql(connectionString));


builder.Services.AddDatabaseDeveloperPageExceptionFilter();


builder.Services.AddIdentityCore<ApplicationUser>(options =>
        {
            // Sign in
            options.SignIn.RequireConfirmedAccount = false;
            // options.SignIn.RequireConfirmedEmail = false;
            // options.SignIn.RequireConfirmedPhoneNumber = false;

            // Password settings.
            // options.Password.RequireDigit = true;
            // options.Password.RequireLowercase = true;
            // options.Password.RequireNonAlphanumeric = true;
            // options.Password.RequireUppercase = true;
            // options.Password.RequiredLength = 6;
            // options.Password.RequiredUniqueChars = 1;

            // Lockout settings.
            // options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            // options.Lockout.MaxFailedAccessAttempts = 5;
            // options.Lockout.AllowedForNewUsers = true;

            // User settings.
            // options.User.AllowedUserNameCharacters =
            //     "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            // options.User.RequireUniqueEmail = false;
        }
    )
    .AddSignInManager()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

var appSettingsSection = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingsSection);
builder.Services.AddTransient<IEmailSender, SendGridEmailSender>();
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, ApplicationUserEmailSender>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();


app.Run();