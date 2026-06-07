using Microsoft.EntityFrameworkCore;
using MyProject.Domain.Entities;
using MyProject.Domain.IRepositories;
using MyProject.Infrastructure.Repositories;
using MyProject.Application.Services;
using Microsoft.AspNetCore.DataProtection;
using MyProject.WebMvc.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<HospitalManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Data Protection to share authentication cookies with WebApi
builder.Services.AddDataProtection()
    .SetApplicationName("HospitalManagementSharedApp");

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CookieForwardingHandler>();

// Register Repositories
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IMedicineRepository, MedicineRepository>();
builder.Services.AddScoped<ILabTestRepository, LabTestRepository>();
builder.Services.AddScoped<IDoctorScheduleRepository, DoctorScheduleRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

// Register Services
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<StaffService>();
builder.Services.AddScoped<PatientService>();
builder.Services.AddScoped<MedicineService>();
builder.Services.AddScoped<LabTestService>();
builder.Services.AddScoped<DoctorScheduleService>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<AuthService>();

// Register API Services (for calling WebApi via HttpClient)
builder.Services.AddScoped<AuthApiService>();
builder.Services.AddScoped<RoleApiService>();
builder.Services.AddScoped<StaffApiService>();
builder.Services.AddScoped<PatientApiService>();
builder.Services.AddScoped<MedicineApiService>();
builder.Services.AddScoped<LabTestApiService>();
builder.Services.AddScoped<DoctorScheduleApiService>();
builder.Services.AddScoped<AppointmentApiService>();

// Register Cookie Authentication
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = System.TimeSpan.FromMinutes(60);
    });

// Add HttpClient for calling WebApi
builder.Services.AddHttpClient("WebApiClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7281/api/");
    client.Timeout = TimeSpan.FromSeconds(30);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseCookies = false
}).AddHttpMessageHandler<CookieForwardingHandler>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
