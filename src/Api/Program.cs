using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using RealTimeMongoDashboard.API.Config;
using RealTimeMongoDashboard.Application.Interfaces;
using RealTimeMongoDashboard.Infrastructure.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

// Mongo
var mongoCs = builder.Configuration.GetSection("Mongo:ConnectionString").Value!;
var mongoDbName = builder.Configuration.GetSection("Mongo:Database").Value!;
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoCs));
builder.Services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoDbName));

// Allowed collections (simple example: read from config or hardcode)
builder.Services.AddSingleton<IAllowedCollections>(_ => new AllowedCollections(new[] { "orders", "users", "metrics" }));

// Services
builder.Services.AddScoped<ICollectionService, CollectionService>();

// JWT
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// Controllers + SignalR
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Swagger + JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RealTime MongoDB Dashboard API", Version = "v1" });
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { scheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationsHub>("/hub/notifications"); // you already added this hub

app.Run();

// ---- minimal impl for AllowedCollections (or move to Infrastructure) ----
public sealed class AllowedCollections : IAllowedCollections
{
    private readonly HashSet<string> _set;
    public AllowedCollections(IEnumerable<string> allowed) => _set = new HashSet<string>(allowed, StringComparer.OrdinalIgnoreCase);
    public bool IsAllowed(string collection) => _set.Contains(collection);
}
