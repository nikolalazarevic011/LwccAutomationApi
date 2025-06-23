
using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Scalar.AspNetCore;
using WsRaisedHandsModern.Api.Helpers;
using WsRaisedHandsModern.Api.Interfaces;
using WsRaisedHandsModern.Api.Services;
using Microsoft.EntityFrameworkCore;
using WsRaisedHandsModern.Api.Data.CMSReporting;
using WsRaisedHandsModern.Api.Data.CMSReporting.Interfaces;
using WsRaisedHandsModern.Api.Data.CMSReporting.Repositories;
// using WsRaisedHandsModern.Api.Data.CMSReporting.Entities; // Uncomment if you need to use entities directly


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

builder.Services.AddControllers();

//add Email Services and settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

// Add Excel service
builder.Services.AddTransient<IExcelService, ExcelService>();

// Add Certificate Services and settings
builder.Services.Configure<CertificateSettings>(builder.Configuration.GetSection("CertificateSettings"));
builder.Services.AddTransient<ICertificateService, CertificateService>();
builder.Services.AddTransient<ICsvProcessingService, CsvProcessingService>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//Add Connection Strings
var cmsConnectionString = builder.Configuration.GetConnectionString("CmsReportingConnection");
builder.Services.AddDbContext<CmsReportingDbContext>(opt =>
{
    opt.UseSqlServer(cmsConnectionString);
});

/*var connectionString = "";
if (builder.Environment.IsDevelopment())
    connectionString = builder.Configuration.GetConnectionString("Development") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");
else
{
    // Use connection string provided at runtime by FlyIO.
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");
}
builder.Services.AddDbContext<CmsReportingDbContext>(opt =>
{
    opt.UseSqlServer(connectionString,
        b => b.MigrationsAssembly("WsRaisedHandsModern.Api")); //specify the assembly where the migrations are located
});*/

//add CMS Reporting Repository
builder.Services.AddScoped<IRaisedHandsCmsReportRepository, RaisedHandsCmsReportRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStarted.Register(() =>
{
    foreach (var address in app.Urls)
    {
        Log.Information("Now listening on: {Address}", address);
    }
});

app.MapControllers();

Log.Information("Starting up");

app.Run();

Log.Information("Shutting down");

Log.CloseAndFlush(); // Ensure logs are flushed on shutdown

