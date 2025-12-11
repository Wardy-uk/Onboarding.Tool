using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Onboarding.Tool.Data;
using Onboarding.Tool.Server.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.ApplicationInsights(builder.Configuration["ApplicationInsights:ConnectionString"], TelemetryConverter.Traces)
    .CreateLogger();

builder.Host.UseSerilog(Log.Logger);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHostHeaderName = ForwardedHeadersDefaults.XOriginalHostHeaderName;
    options.ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedPrefix;
    options.AllowedHosts = ["*.nurtur.services"];
});

builder.Services.AddHttpClient();
builder.Services.AddServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddAllowedEmailPolicy();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

OpenIdSettings? openIdOptions = builder.Configuration.GetSection("OpenId").Get<OpenIdSettings>() ?? throw new InvalidOperationException();
ApplicationGatewaySettings? applicationGatewaySettings = builder.Configuration.GetSection("ApplicationGateway").Get<ApplicationGatewaySettings>() ?? throw new InvalidOperationException();
builder.AddAuthentication(openIdOptions);

string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? throw new InvalidOperationException();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseForwardedHeaders();

app.UseDefaultFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.Use((context, next) =>
{
    string basePath = applicationGatewaySettings.BasePath;
    
    if (!string.IsNullOrEmpty(basePath))
    {
        context.Request.PathBase = basePath;
    }

    context.Request.Scheme = "https";
    return next();
});

app.UseCors("AllowReactApp");

app.UseSerilogRequestLogging();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapApiEndpoints();

app.MapFallbackToFile("/index.html");

app.Run();
