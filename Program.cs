using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OBMS.WebAPI.Models;
using OBMS.WebAPI.Repositories.Implementation;
using OBMS.WebAPI.Repositories.Interface;
using OBMS.WebAPI.Services;
using System.Text;
using OBMS.WebAPI.BusinessObjects;
using System.Web.Services.Description;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
// Determine the environment (Development, Staging, Production, etc.)
var environment = builder.Environment;

// Add services to the container.
// Configure JWT Authentication
var key = Encoding.ASCII.GetBytes("obms-authentication");

// Add environment-specific configurations
builder.Configuration
    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

//connection string
builder.Services.AddDbContext<OBMSDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("obms"),
        sqlOptions => {
            sqlOptions.EnableRetryOnFailure();
            sqlOptions.CommandTimeout(300); // 5 minutes timeout for long-running operations
        }));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMvc().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

//to inject the interface and implementations
builder.Services.AddScoped<IRegisterRepository, RegisterRepository>();
builder.Services.AddScoped<IMasterRepository, MasterRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IPayrollRepository, PayrollRepository>();
builder.Services.AddScoped<IAgreementRepository, AgreementRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IFinanceRepository, FinanceRepository>();
builder.Services.AddScoped<ISalaryProcess, SalaryProcess>();
builder.Services.AddScoped<IAccountingRepository, AccountingRepository>();
builder.Services.AddScoped<IQuotationRepository, QuotationRepository>();

// Add Indian Payroll Calculation Services
builder.Services.AddScoped<IPFCalculationService, PFCalculationService>();
builder.Services.AddScoped<IESICalculationService, ESICalculationService>();
builder.Services.AddScoped<ITDSCalculationService, TDSCalculationService>();
builder.Services.AddScoped<IProfessionalTaxService, ProfessionalTaxService>();

// Add GST Calculation Service
builder.Services.AddScoped<IGSTCalculationService, GSTCalculationService>();

// Add Excel Services
builder.Services.AddScoped<IAttendanceExcelService, AttendanceExcelService>();
builder.Services.AddScoped<IAttendancePeriodService, AttendancePeriodService>();

//builder.Services.AddScoped<DataAccess>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "yourapi.com",
        ValidAudience = "yourapi.com",
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero // Token expires exactly at expiration time
    };
});
builder.Services.AddControllers(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }
app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection();

app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
