using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SemsApi.Configuration;
using SemsApi.Data;
using SemsApi.Interfaces;
using SemsApi.Models;
using SemsApi.Seeders;
using SemsApi.Services;
using SemsApi.Utilities;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection("Auth"));

builder.Services.AddIdentity<ApplicationUser, Role>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>()
    .AddClasses(classes => classes
        .Where(type => type.Name.EndsWith("Service")))
        .AsImplementedInterfaces()
        .WithScopedLifetime()
 );

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Google:ClientSecret"]!;
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.SaveTokens = false;
    options.ClaimActions.MapJsonKey("email_verified", "email_verified");
    options.SignInScheme = IdentityConstants.ExternalScheme;

    options.Events.OnRedirectToAuthorizationEndpoint = ctx =>
    {
        var url = QueryHelpers.AddQueryString(ctx.RedirectUri, new Dictionary<string, string?>
        {
            ["hd"] = "dmc.edu.ph",
            ["prompt"] = "select_account"
        });
        ctx.Response.Redirect(url);
        return Task.CompletedTask;
    };
})
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["ACCESS_TOKEN"];
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(options =>
{
    // Vite binds the IPv6 loopback on some machines, so both `localhost` and
    // `[::1]` (http and https) occur in practice. `Frontend:AllowedOrigins`
    // adds extra origins without code changes; anything not listed is rejected.
    var origins = new List<string>();

    var configured = builder.Configuration["Frontend:AllowedOrigins"];
    if (!string.IsNullOrWhiteSpace(configured))
    {
        origins.AddRange(configured
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(o => o.Trim().TrimEnd('/')));
    }

    var primary = builder.Configuration["Frontend:BaseUrl"];
    if (!string.IsNullOrWhiteSpace(primary))
        origins.Add(primary.Trim().TrimEnd('/'));

    if (builder.Environment.IsDevelopment())
    {
        origins.AddRange(new[]
        {
            "http://localhost:5173",
            "https://localhost:5173",
            "http://[::1]:5173",
            "https://[::1]:5173",
        });
    }

    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(origins.Distinct(StringComparer.OrdinalIgnoreCase).ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddAuthorization();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SEMS API", Version = "v1" });

    options.AddSecurityDefinition("AccessTokenCookie", new OpenApiSecurityScheme
    {
        Name = "ACCESS_TOKEN",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Description = "JWT issued at sign-in and stored in the ACCESS_TOKEN cookie."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("AccessTokenCookie", document)] = []
    });
});
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    await RoleSeeder.SeedAsync(roleManager);

    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await FacilitySeeder.SeedAsync(context);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SEMS API v1");
    });
}
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();