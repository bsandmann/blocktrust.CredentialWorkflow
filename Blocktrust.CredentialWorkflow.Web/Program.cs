using Blocktrust.Common.Resolver;
using Blocktrust.CredentialWorkflow.Core;
using Blocktrust.CredentialWorkflow.Core.Commands.Workflow.ExecuteWorkflow.ActionProcessors;
using Blocktrust.CredentialWorkflow.Core.Crypto;
using Blocktrust.CredentialWorkflow.Core.Entities.Identity;
using Blocktrust.CredentialWorkflow.Core.Services;
using Blocktrust.CredentialWorkflow.Core.Services.DIDComm;
using Blocktrust.CredentialWorkflow.Core.Services.DIDPrism;
using Blocktrust.CredentialWorkflow.Core.Services.Interfaces;
using Blocktrust.CredentialWorkflow.Core.Settings;
using Blocktrust.CredentialWorkflow.Web.Common;
using Blocktrust.CredentialWorkflow.Web.Components.Account;
using Blocktrust.CredentialWorkflow.Web.Services;
using Blocktrust.Mediator.Client.Commands.TrustPing;
using Blocktrust.Mediator.Common;
using Blocktrust.Mediator.Common.Commands.CreatePeerDid;
using DidPrismResolverClient;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

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


var MyAllowSpecificOrigins = "MyCorsPolicy";

// Register the CORS policy in the Services container:
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins, policy =>
    {
        // For local debugging, you can allow everything (less secure):
        // policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();

        // Or allow just your extension’s origin and local dev calls:
        policy
            .WithOrigins("chrome-extension://maknmcndlbopnoalcpponedjadclakng",
                "https://localhost:7209") // or "http://localhost:7209" if you are using HTTP
            .AllowAnyHeader()
            .AllowAnyMethod()
            // If you use cookies or Auth headers, you also need:
            .AllowCredentials();
    });
});

// Add Core Services

// builder.Services.AddScoped<IDidResolutionService, DidResolutionService>();

builder.Services.AddScoped<CredentialParser>();
builder.Services.AddScoped<ExtractPrismPubKeyFromLongFormDid>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IEcService, EcServiceBouncyCastle>();

builder.Services.AddSingleton<IWorkflowQueue, WorkflowQueue>();
builder.Services.AddHostedService<WorkflowProcessingService>();

builder.Services.AddHostedService<RecurringWorkflowBackgroundService>();

builder.Services.AddSingleton<ITriggerValidationService, HttpTriggerValidationService>();

builder.Services.AddSingleton<TemplateService>();

builder.Services.AddScoped<ISchemaValidationService, SchemaValidationService>();
builder.Services.AddScoped<IFormService, FormService>();

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddTransient<CustomValidationProcessor>();
builder.Services.AddTransient<CreateDIDActionProcessor>();
builder.Services.AddTransient<UpdateDIDActionProcessor>();
builder.Services.AddTransient<DeactivateDIDActionProcessor>();
builder.Services.AddTransient<DIDCommActionProcessor>();
builder.Services.AddTransient<EmailActionProcessor>();
builder.Services.AddTransient<HttpActionProcessor>();
builder.Services.AddTransient<IssueW3CCredentialProcessor>();
builder.Services.AddTransient<VerifyW3CCredentialProcessor>();
builder.Services.AddTransient<W3cValidationProcessor>();
builder.Services.AddTransient<JwtTokenGeneratorActionProcessor>();

// Add HTTP client for HTTP Action processor
builder.Services.AddHttpClient("HttpActionProcessor");


// Configure strongly typed settings
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<CredentialSettings>(
    builder.Configuration.GetSection("CredentialSettings"));
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// Configure Authentication and Authorization
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContextFactory<DataContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddSignInManager()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Configure primary PrismDidClient
builder.Services.AddHttpClient<PrismDidClient>("PrimaryPrismClient")
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            // Insecure: bypass SSL errors
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    });

// Configure fallback PrismDidClient
builder.Services.AddHttpClient<PrismDidClient>("FallbackPrismClient")
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            // Insecure: bypass SSL errors
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    });


// Register the primary client options with a name for better identification
builder.Services.AddSingleton<PrimaryPrismDidClientOptions>(sp => 
{
    var appSettings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AppSettings>>().Value;
    return new PrimaryPrismDidClientOptions
    {
        BaseUrl = appSettings.PrismBaseUrl,
        DefaultLedger = appSettings.PrismDefaultLedger
    };
});

// Register primary options as PrismDidClientOptions for backward compatibility
builder.Services.AddSingleton<PrismDidClientOptions>(sp => 
    sp.GetRequiredService<PrimaryPrismDidClientOptions>());

// Register the fallback client options with a name for better identification
builder.Services.AddSingleton<FallbackPrismDidClientOptions>(sp => 
{
    var appSettings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AppSettings>>().Value;
    return new FallbackPrismDidClientOptions
    {
        BaseUrl = appSettings.PrismBaseUrlFallback,
        DefaultLedger = appSettings.PrismDefaultLedgerFallback
    };
});

// Register the primary client as the default implementation
builder.Services.AddScoped(sp => 
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var options = sp.GetRequiredService<PrimaryPrismDidClientOptions>();
    var httpClient = factory.CreateClient("PrimaryPrismClient");
    return new PrismDidClient(httpClient, options);
});

// Register the fallback client as a named service
builder.Services.AddScoped<FallbackPrismDidClient>(sp => 
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var options = sp.GetRequiredService<FallbackPrismDidClientOptions>();
    var httpClient = factory.CreateClient("FallbackPrismClient");
    return new FallbackPrismDidClient(new PrismDidClient(httpClient, options));
});

builder.Services.AddTransient<IEmailSender, SendGridEmailSender>();
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, ApplicationUserEmailSender>();

builder.Services.AddScoped<ISecretResolver, PeerDIDSecretResolver>();
builder.Services.AddSingleton<IDidDocResolver, SimpleDidDocResolver>();

// Add MediatR with all handlers
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(DataContext).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CreatePeerDidHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(TrustPingHandler).Assembly);
});

builder.Services.AddAntiforgery();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}


// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

// Apply database migrations at startup
using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    dataContext.Database.Migrate();
}

app.Run();