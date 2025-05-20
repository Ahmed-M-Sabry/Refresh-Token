using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NuGet.Protocol.Core.Types;
using RepoPatternAndJwt.Core.Helper;
using RepoPatternAndJwt.Core.Models;
using RepoPatternAndJwt.Core.RepositoriesInterFace;
using RepoPatternAndJwt.EF.Data;
using RepoPatternAndJwt.EF.Reopsitories;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    // Define the basic info for the Swagger UI like title and version of the API
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ahmed Sabry", Version = "v1" });

    // Define the security scheme for JWT authorization (Bearer token)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        // Description of how JWT is used for authorization
        Description = "JWT Authorization header using the Bearer scheme.",

        // Specify that the security scheme is HTTP-based
        Type = SecuritySchemeType.Http,

        // Define the scheme type as "bearer" for JWT tokens
        Scheme = "bearer",

        // Define the format as JWT (JSON Web Token)
        BearerFormat = "JWT"
    });

    // Add a requirement for security: any request must include the Bearer token for authorization
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                // Reference to the previously defined Bearer scheme
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer" // Referring to the "Bearer" security definition created above
                }
            },
            // An empty array means no specific scopes are required
            Array.Empty<string>()
        }
    });
});



// ConnectionString and DataBase
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddDbContext<ApplicationDbContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
         b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

// Map JWT Section in app setting for class which i create
builder.Services.Configure<JwtSetting>(builder.Configuration.GetSection("JWT"));

// Map Classes and interface

builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthServices, AuthServices>();


// Bearer
// دي الطريقة اللي المصادقة اللي هيستخدمها الطلب

// Bearer token
// دا التوكين اللي هيتبعت

//JWT
// الشكل اللي هيكون عليه التوكين


// Bearer is a type or method of authentication used in HTTP requests

// A Bearer token is an access token sent with a request,
// and when you include it in your HTTP request, you’re saying
// “I possess this token, and it grants me access to the requested resource.”

// JWT (JSON Web Token) is the actual token that contains information about the user and their permissions.
// JWT is a specific type of token that is often used as a Bearer token

//This token could be a JWT or any other form of access token.

// JWT is commonly used as a Bearer token,
// but a Bearer token could be any form of access token, not just JWT.

builder.Services.AddAuthentication(
    option => {
        option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    // AddJwtBearer
    // method is part of the ASP.NET Core authentication system,
    // used to configure and enable JWT (JSON Web Token) authentication for your application.
    // It's specifically used when you want your application to accept JWT tokens as a way of authenticating API
    // requests, typically for scenarios like API authentication, mobile app authentication,
    // or single-page applications (SPA) where the client sends JWT tokens with each request.
    .AddJwtBearer(
        op =>
        {
            // This determines whether the JWT metadata must be transmitted over HTTPS. In production,
            // you should set this to true to ensure secure transmission.
            op.RequireHttpsMetadata = false;
            // This controls whether the token will be stored after authentication,
            // typically in AuthenticationProperties. In stateless applications
            // (which don’t store server-side sessions), it's common to set this to false
            op.SaveToken = false;

            // This is the heart of JWT validation.
            // You specify rules here to validate incoming JWT tokens to ensure they’re legitimate and valid
            op.TokenValidationParameters = new TokenValidationParameters
            {
                // Ensures that the signature key used to sign the JWT is valid
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = builder.Configuration["JWT:Issuer"],
                ValidAudience = builder.Configuration["JWT:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),

                // If ValidateLifetime is missing, expired tokens could be used.
                // If ValidateIssuer or ValidateAudience is missing, a token from an untrusted source or for an unauthorized system might be accepted
                // If ValidateIssuerSigningKey or IssuerSigningKey is missing, a forged token could be accepted.

                ClockSkew = TimeSpan.Zero
            };
        }

    );





var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
