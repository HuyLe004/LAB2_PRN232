using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StudentService.Data;
using StudentService.Entities;
using StudentService.Grpc;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddDbContext<StudentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
    options.ConfigureEndpointDefaults(endpointOptions =>
    {
        endpointOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddControllers();
builder.Services.AddGrpc();
builder.Services.AddTransient<StudentGrpcService>();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwtSection["Secret"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLongForLab3";
var jwtIssuer = jwtSection["Issuer"] ?? "PRN232_Lab3";
var jwtAudience = jwtSection["Audience"] ?? "PRN232_Lab3_Users";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Student Service API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

SeedStudentData(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/openapi/v1.json", "Student Service API"));
}

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();
app.MapGrpcService<StudentGrpcService>();

app.Run();

static void SeedStudentData(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
    context.Database.EnsureCreated();
    context.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Students')
BEGIN
    CREATE TABLE [Students] (
        [StudentId] int NOT NULL IDENTITY(1,1),
        [FullName] nvarchar(100) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [DateOfBirth] datetime2 NOT NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [StudentCode] nvarchar(max) NULL,
        CONSTRAINT [PK_Students] PRIMARY KEY ([StudentId])
    );
END;
");

    if (!context.Students.Any())
    {
        context.Students.AddRange(
            new Student { FullName = "Nguyen Van A", Email = "a@example.com", DateOfBirth = new DateTime(2001, 1, 1), PhoneNumber = "0900000001", StudentCode = "SV001" },
            new Student { FullName = "Tran Thi B", Email = "b@example.com", DateOfBirth = new DateTime(2002, 2, 2), PhoneNumber = "0900000002", StudentCode = "SV002" });
        context.SaveChanges();
    }
}

