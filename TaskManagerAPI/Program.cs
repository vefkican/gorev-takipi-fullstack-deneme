using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskManagerAPI.Data;
using TaskManagerAPI.Repositories;
using TaskManagerAPI.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Serilog;

// ==================== SERILOG YAPILANDIRMASI ====================
// Konsola ve günlük dosyasına loglama (logs/log-{tarih}.txt)
// Microsoft ve EF Core logları Warning seviyesinde tutulur (gürültüyü azaltmak için)

var logConfig = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
{
    logConfig.WriteTo.Seq("http://localhost:5341");
}

Log.Logger = logConfig.CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(); // Serilog'u varsayılan logger olarak ayarla

// ==================== SERVİS KAYITLARI ====================

builder.Services.AddControllers();

// Rate Limiting - API isteklerini sınırlandırma
builder.Services.AddRateLimiter(options =>
{
    // Genel istekler: 10 saniyede en fazla 10 istek
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromSeconds(10);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    // Auth istekleri: 1 dakikada en fazla 5 istek (brute force koruması)
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    options.RejectionStatusCode = 429;
});

// CORS - React frontend erişim izni
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://gorev-takipi-fullstack-deneme.vercel.app")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ==================== VERİTABANI BAĞLANTISI ====================

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

string connectionString;

if (databaseUrl != null)
{
    // Production: DATABASE_URL ortam değişkeninden bağlantı bilgisi al
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]}";
}
else
{
    // Development: appsettings.json'dan bağlantı bilgisi al
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ==================== DEPENDENCY INJECTION ====================

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

// ==================== JWT KİMLİK DOĞRULAMA ====================

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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
    });

// ==================== MIDDLEWARE PIPELINE ====================

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseSwagger();                    // Swagger JSON endpoint
app.UseSwaggerUI();                  // Swagger arayüzü

app.UseCors("AllowReact");           // CORS politikası uygula
app.UseMiddleware<TaskManagerAPI.Middlewares.ErrorHandlingMiddleware>(); // Global hata yakalama
app.UseHttpsRedirection();           // HTTP → HTTPS yönlendirme
app.UseAuthentication();             // JWT token doğrulama
app.UseRateLimiter();                // İstek sınırlandırma
app.UseAuthorization();              // Yetkilendirme kontrolü
app.MapControllers();                // Controller endpoint'lerini eşle

// ==================== VERİTABANI MİGRASYONU ====================
// Uygulama başlarken bekleyen migration'ları otomatik uygula

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();