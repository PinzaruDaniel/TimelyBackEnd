using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Quartz;
using TimelyBackEnd.Data;
using TimelyBackEnd.Services;
using TimelyBackEnd.Services.Jobs;
using TimelyBackEnd.Services.Interfaces;
using TimelyBackEnd.Services.Implementations;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuration & DB
builder.Services.AddDbContext<TimelyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IHomeworkService, HomeworkService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<NotificationJob>(); // Add this
builder.Services.AddScoped<HomeworkCleanupJob>(); // Register HomeworkCleanupJob
builder.Services.AddHttpClient();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Quartz for notifications and homework cleanup
builder.Services.AddQuartz(q =>
{
    // Notification job
    var notificationJobKey = new JobKey("NotificationJob");
    q.AddJob<NotificationJob>(opts => opts.WithIdentity(notificationJobKey));
    q.AddTrigger(opts => opts
        .ForJob(notificationJobKey)
        .WithIdentity("NotificationJob-trigger")
        .WithSimpleSchedule(x => x.WithIntervalInMinutes(5).RepeatForever()));
    
    // Homework cleanup job
    var cleanupJobKey = new JobKey("HomeworkCleanupJob");
    q.AddJob<HomeworkCleanupJob>(opts => opts.WithIdentity(cleanupJobKey));
    q.AddTrigger(opts => opts
        .ForJob(cleanupJobKey)
        .WithIdentity("HomeworkCleanupJob-trigger")
        .WithSimpleSchedule(x => x.WithIntervalInMinutes(10).RepeatForever()));
});
builder.Services.AddQuartzHostedService(opt => opt.WaitForJobsToComplete = true);

// JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key is not configured");
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
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TimelyBackEnd API",
        Version = "v1"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer {token}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new string[] { } }
    });
});

var app = builder.Build();

// Seed default user if it doesn't exist
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TimelyDbContext>();
    try
    {
        await context.Database.EnsureCreatedAsync();
        
        var defaultUserEmail = "alice.smith@example.com";
        if (!await context.Users.AnyAsync(u => u.Email == defaultUserEmail))
        {
            var defaultUser = new TimelyBackEnd.Models.User
            {
                Id = Guid.NewGuid(),
                FullName = "Alice Smith",
                Email = defaultUserEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                FirstName = "Alice",
                LastName = "Smith",
                Age = 25,
                Street = "123 Maple Street",
                City = "Springfield",
                State = "IL",
                Zip = "62701",
                Country = "USA",
                ImageUrl = "https://optimistdrinks.com/cdn/shop/articles/oip21_day_5_1.jpg?v=1621112229",
                Role = "Student"
            };
            
            context.Users.Add(defaultUser);
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Default user 'alice.smith@example.com' created successfully!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Error seeding default user: {ex.Message}");
    }
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend"); 

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();