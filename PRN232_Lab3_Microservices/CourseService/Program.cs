using System.Text;
using System.Text.Json.Serialization;
using CourseService.Data;
using CourseService.Entities;
using CourseService.Grpc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddDbContext<CourseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddScoped<StudentGrpcClient>();

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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Course Service API", Version = "v1" });
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

SeedCourseData(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/openapi/v1.json", "Course Service API"));
}

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

app.Run();

static void SeedCourseData(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CourseDbContext>();
    context.Database.EnsureCreated();
    context.Database.ExecuteSqlRaw(@"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Semesters')
BEGIN
    CREATE TABLE [Semesters] (
        [SemesterId] int NOT NULL IDENTITY(1,1),
        [SemesterName] nvarchar(max) NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Semesters] PRIMARY KEY ([SemesterId])
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Courses')
BEGIN
    CREATE TABLE [Courses] (
        [CourseId] int NOT NULL IDENTITY(1,1),
        [CourseName] nvarchar(max) NOT NULL,
        [SemesterId] int NOT NULL,
        CONSTRAINT [PK_Courses] PRIMARY KEY ([CourseId]),
        CONSTRAINT [FK_Courses_Semesters_SemesterId] FOREIGN KEY ([SemesterId]) REFERENCES [Semesters] ([SemesterId]) ON DELETE CASCADE
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Subjects')
BEGIN
    CREATE TABLE [Subjects] (
        [SubjectId] int NOT NULL IDENTITY(1,1),
        [SubjectCode] nvarchar(max) NOT NULL,
        [SubjectName] nvarchar(max) NOT NULL,
        [Credit] int NOT NULL,
        CONSTRAINT [PK_Subjects] PRIMARY KEY ([SubjectId])
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Enrollments')
BEGIN
    CREATE TABLE [Enrollments] (
        [EnrollmentId] int NOT NULL IDENTITY(1,1),
        [StudentId] int NOT NULL,
        [CourseId] int NOT NULL,
        [EnrollDate] datetime2 NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Enrollments] PRIMARY KEY ([EnrollmentId]),
        CONSTRAINT [FK_Enrollments_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([CourseId]) ON DELETE CASCADE
    );
END;
");

    if (!context.Semesters.Any())
    {
        var semester = new Semester { SemesterName = "Summer 2026", StartDate = new DateTime(2026, 6, 1), EndDate = new DateTime(2026, 8, 31) };
        context.Semesters.Add(semester);
        context.SaveChanges();
    }

    if (!context.Courses.Any())
    {
        var semester = context.Semesters.First();
        context.Courses.Add(new Course { CourseName = "PRN232", SemesterId = semester.SemesterId });
        context.SaveChanges();
    }
}

