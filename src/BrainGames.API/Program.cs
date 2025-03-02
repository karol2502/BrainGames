using System.Configuration;
using System.Security.Claims;
using BrainGames.API.Hubs;
using BrainGames.API.Middlewares;
using BrainGames.API.Persistence;
using BrainGames.API.Services;
using BrainGames.API.Workers;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();

var assembly = typeof(Program).Assembly;
builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));
builder.Services
    .AddValidatorsFromAssembly(assembly)
    .AddFluentValidationAutoValidation();

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services
    .AddDbContext<BrainGamesDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("BrainGamesDB")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth0:Authority"];
        options.Audience = builder.Configuration["Auth0:Audience"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddHangfire(configuration => configuration
    .UseRecommendedSerializerSettings()
    .UseSerializerSettings(new JsonSerializerSettings
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    })
    .UseInMemoryStorage()
);

// Add the processing server as IHostedService
builder.Services.AddHangfireServer();


builder.Services.AddScoped<IUserContext, UserContext>();
builder.Services.AddHostedService<UpdateGameWorker>();


var app = builder.Build();

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(_ => true) // allow any origin
    .AllowCredentials());

app.UseMiddleware<HubsMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard();
app.MapHangfireDashboard();

app.MapControllers();
app.MapHub<GameHub>("/hubs/game", options =>
{
    options.Transports = HttpTransportType.WebSockets;
});

app.Run();
