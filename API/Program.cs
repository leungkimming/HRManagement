using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Autofac.Extensions.DependencyInjection;
using Autofac;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.Extensions.Logging.EventLog;
using API;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.HttpSys;

const string AllowCors = "AllowCors";
const string CORS_ORIGINS = "CorsOrigins";

var builder = WebApplication.CreateBuilder(args);
// Windows Event logging
builder.Logging.ClearProviders();
builder.Logging.AddEventLog(eventLogSettings =>
{
    eventLogSettings.SourceName = ".NET Runtime";
});

// allow CORS
builder.Services.AddCors(option => option.AddPolicy(
    AllowCors,
    policy =>
        policy.WithOrigins(builder.Configuration.GetSection(CORS_ORIGINS).Get<string[]>()).AllowAnyHeader().AllowCredentials().AllowAnyMethod()
));
// Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(cbuilder 
    => cbuilder.RegisterModule(new API.RegisterModule(builder.Configuration.GetConnectionString("DDDConnectionString"))));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Service.MapperProfiles.UserProfile).Assembly);

// Add other features
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
});

//builder.Services.AddAuthentication();
builder.Services.AddAuthentication(HttpSysDefaults.AuthenticationScheme);
if (builder.Environment.IsEnvironment("SpecFlow"))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
    builder.Services.AddAuthorization(options =>
    {
        options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .Build();
    });
} else {
    builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
       .AddNegotiate();
    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = options.DefaultPolicy;
    });
}    

// for Blazor wasm hosting
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || builder.Environment.IsEnvironment("SpecFlow"))
{
    //app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
    // for Blazor wasm hosting
    app.UseWebAssemblyDebugging();
} else {
    builder.WebHost.UseHttpSys(options =>
    {
        options.Authentication.Schemes = AuthenticationSchemes.Negotiate | AuthenticationSchemes.NTLM;
        options.Authentication.AllowAnonymous = false;
    });
    //app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseExceptionHandler("/Error");
app.UseMiddleware<ErrorHandler>();

app.UseCors(AllowCors);

app.UseHttpsRedirection();

app.MapControllers();

// for Blazor wasm hosting
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");
app.Run();

// Define class name 'Program' for Specflow to work
public partial class Program { }