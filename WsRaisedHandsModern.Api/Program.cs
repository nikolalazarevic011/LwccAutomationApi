using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Scalar.AspNetCore;
using WsRaisedHandsModern.Api.Helpers;
using WsRaisedHandsModern.Api.Interfaces;
using WsRaisedHandsModern.Api.Services;
using WsRaisedHandsModern.Api.Data.CMSReporting;
using WsRaisedHandsModern.Api.Data.CMSReporting.Interfaces;
using WsRaisedHandsModern.Api.Data.CMSReporting.Repositories;
using WsRaisedHandsModern.Api.Data.AppData;
using WsRaisedHandsModern.Api.Data.AppData.Interfaces;
using WsRaisedHandsModern.Api.Data.AppData.Repositories;
using WsRaisedHandsModern.Api.Data.AppData.Entities;
using WsRaisedHandsModern.Api.Extensions;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Use builder.Configuration for logPath
var logPath = builder.Configuration["LogPath"] ?? @"C:\temp\RaisedHandsLog.txt";

// Bootstrap logger captures startup errors before full configuration loads
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

// Replace default logging with Serilog
builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .WriteTo.Console()
    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
);

// Add MVC controllers and views
builder.Services.AddControllersWithViews();

// Add controllers for API
builder.Services.AddControllers();

// Add Email Services and settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Register EmailService for both IEmailService and IEmailSender interfaces
builder.Services.AddTransient<EmailService>();
builder.Services.AddTransient<IEmailService>(provider => provider.GetService<EmailService>()!);

// Add Excel service
builder.Services.AddTransient<IExcelService, ExcelService>();

// Add Certificate Services and settings
builder.Services.Configure<CertificateSettings>(builder.Configuration.GetSection("CertificateSettings"));
builder.Services.AddTransient<ICertificateService, CertificateService>();
builder.Services.AddTransient<ICsvProcessingService, CsvProcessingService>();

// Add OpenAPI services
builder.Services.AddOpenApi();

// Add External CMS Connection String
var cmsConnectionString = builder.Configuration.GetConnectionString("CmsReportingConnection");
builder.Services.AddDbContext<CmsReportingDbContext>(opt =>
{
    opt.UseSqlServer(cmsConnectionString);
    // Intentionally not specifying migrations assembly here, this is an external/legacy database
});

// Add Connection String for ApplicationDbContext (Identity)
var connectionString = "";
if (builder.Environment.IsDevelopment())
    connectionString = builder.Configuration.GetConnectionString("Development") ?? throw new InvalidOperationException("Connection string 'Development' not found.");
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    opt.UseSqlServer(connectionString,
        b => b.MigrationsAssembly("WsRaisedHandsModern.Api")); // Specify the assembly where the migrations are located
});

// Add identity services from Extensions folder
builder.Services.AddIdentityServices(builder.Configuration);

// Configure Razor Pages for Identity
builder.Services.AddRazorPages().AddRazorPagesOptions(options =>
{
    // Protect the entire /Account/Manage folder in the Identity area
    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");

    // Disable public registration - require admin role to access
    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Register", "RequireAdminRole");
});

// Add CMS Reporting Repository
builder.Services.AddScoped<IRaisedHandsCmsReportRepository, RaisedHandsCmsReportRepository>();

// Add AppData repositories
builder.Services.AddScoped<IEmailHistoryRepository, EmailHistoryRepository>();

// Add Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Log application startup
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStarted.Register(() =>
{
    foreach (var address in app.Urls)
    {
        Log.Information("Now listening on: {Address}", address);
    }
});

// Add middleware
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add authentication and authorization middleware
app.UseAuthentication();  // Needs to come before UseAuthorization
app.UseAuthorization();   // Add before any endpoint mapping

// Map endpoints
app.MapRazorPages(); // Needed for Identity UI

// Add conventional routing for MVC views
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Add API controllers with attribute routing
app.MapControllers(); // Add after identity and authentication middleware, to map API Endpoints

Log.Information("Starting up");

// Database initialization and seeding
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();

    // Only seed users in the development environment
    if (app.Environment.IsDevelopment())
    {
        await Seed.SeedUsers(services);
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "An error occurred during migration");
}
finally
{
    Log.Information("Application started successfully");
}

app.Run();

Log.Information("Shutting down");
Log.CloseAndFlush(); // Ensure logs are flushed on shutdown
