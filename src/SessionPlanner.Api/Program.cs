using System.Text;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SessionPlanner.Core.Auth;
using SessionPlanner.Core.Interfaces;
using SessionPlanner.Infrastructure.Auth;
using SessionPlanner.Infrastructure.Data;
using SessionPlanner.Infrastructure.Services;
using System.Reflection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);
const string FrontendCorsPolicy = "FrontendCors";

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SessionPlanner API",
        Version = "v1"
    });

    options.ExampleFilters();

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(document =>
    {
        return new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
        };
    });
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=SessionPlanner.db"));

builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJWTTokenService, JWTTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ISoftwareService, SoftwareService>();
builder.Services.AddScoped<ISoftwareVersionService, SoftwareVersionService>();
builder.Services.AddScoped<IOperatingSystemService, OperatingSystemService>();
builder.Services.AddScoped<ILaboratoryService, LaboratoryService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IPersonnelService, PersonnelService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IEquipmentModelService, EquipmentModelService>();
builder.Services.AddScoped<IPhysicalServerService, PhysicalServerService>();
builder.Services.AddScoped<IVirtualMachineService, VirtualMachineService>();
builder.Services.AddScoped<ISaaSProductService, SaaSProductService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITeachingNeedService, TeachingNeedService>();
builder.Services.AddScoped<ILaboratorySoftwareService, LaboratorySoftwareService>();

var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("JWT key is missing. Configure Jwt:Key in appsettings or user secrets.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    var permissions = PermissionHelper.GetAllPermissions(typeof(Permissions));

    foreach (var permission in permissions)
    {
        options.AddPolicy(permission, policy =>
            policy.RequireClaim("perm", permission));
    }
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var corsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        if (corsOrigins.Length == 0)
        {
            return;
        }

        policy
            .WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<AppDbContext>();

    if (app.Environment.IsEnvironment("Testing"))
    {
        await db.Database.EnsureCreatedAsync();
    }
    else
    {
        await db.Database.MigrateAsync();
    }

    await InitializeData.InitializeAsync(services);
}

var swaggerEnabled = app.Configuration.GetValue<bool>("Swagger:Enabled");

if (swaggerEnabled)
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SessionPlanner API v1");
        options.RoutePrefix = "swagger";
        options.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();

if (corsOrigins.Length > 0)
{
    app.UseCors(FrontendCorsPolicy);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Needed for WebApplicationFactory in integration tests
public partial class Program { }