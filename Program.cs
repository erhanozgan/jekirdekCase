using System.Text;
using jekirdekCase.Components;
using jekirdekCase.Data;
using jekirdekCase.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using jekirdekCase.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ✅ PostgreSQL Bağlantısı
builder.Services.AddDbContext<CRMDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ JWT Authentication Konfigürasyonu
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];
if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT Secret is not configured in appsettings.json.");
}

var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// ✅ Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// ✅ CORS Yapılandırması
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Authorization"); // Token için gerekli olabilir
    });
});

// ✅ Yetkilendirme
builder.Services.AddAuthorization();

// ✅ Fluent Validation Aktif Edildi
builder.Services.AddControllers().AddFluentValidation(fv =>
    fv.RegisterValidatorsFromAssemblyContaining<CustomerValidator>());

// ✅ Controller'lar ve Razor Pages Aktif Edildi
builder.Services.AddControllers();
builder.Services.AddRazorPages();

// ✅ Scoped Servisler
builder.Services.AddScoped<TokenService>();

// ✅ Swagger Konfigürasyonu
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT token kullanımı için: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// ✅ Hata Yakalama Middleware'i
app.UseMiddleware<ErrorHandlingMiddleware>();

// ✅ Swagger Ayarları (Sadece Development Ortamında)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = "swagger"; // Swagger'ı sadece /swagger altında aç
    });
}

// ✅ HTTPS Yönlendirme
app.UseHttpsRedirection();

// ✅ CORS Middleware'i - **Önemli**
app.UseCors("AllowAll");

app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 200;
        return;
    }

    await next();
});

// ✅ Authentication ve Authorization Middleware'leri
app.UseAuthentication();
app.UseAuthorization();

// ✅ Statik Dosya Servisini Devre Dışı Bırak (Eğer wwwroot/index.html varsa onu açmasını engellemek için)
app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = false // Statik dosya servisini tamamen kapatır
});

// ✅ İlk Açılışta Otomatik Yönlendirme Middleware
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/" || context.Request.Path == "/Index")
    {
        var token = context.Request.Cookies["AuthToken"];

        if (string.IsNullOrEmpty(token) && !context.User.Identity.IsAuthenticated)
        {
            context.Response.Redirect("/Account/Login");
        }
        else
        {
            context.Response.Redirect("/customers/customers");
        }
        return;
    }

    await next();
});

app.MapControllers();
app.MapRazorPages();

app.Run();
