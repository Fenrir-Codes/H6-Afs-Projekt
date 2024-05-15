using ElevPortalen.Areas.Identity;
using ElevPortalen.Areas.Identity.Pages.Account;
using ElevPortalen.Data;
using ElevPortalen.DatabaseErrorHandler;
using ElevPortalen.Pages.AlertBox;
using ElevPortalen.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Code Modificated by Jozsef

// Add services to the container.
//LoginDb
var LoginDatabase = builder.Configuration.GetConnectionString("LoginDatabase") ??
    throw new InvalidOperationException("Connection string 'LoginDatabase' not found.");
//The ElevPortalenDb.
var PortalDatabase = builder.Configuration.GetConnectionString("PortalDatabase") ??
    throw new InvalidOperationException("Connection string 'PortalDatabase' not found.");
//RecoveryDb
var DataRecoveryString = builder.Configuration.GetConnectionString("RecoveryDatabase") ??
    throw new InvalidOperationException("Connection string 'RecoveryDatabase' not found.");
//Job offer database
var JobOfferDataBase = builder.Configuration.GetConnectionString("JobOfferDataBase") ??
    throw new InvalidOperationException("Connection string 'LoginDatabase' not found.");

//DbContexts
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(LoginDatabase));
builder.Services.AddDbContext<ElevPortalenDataDbContext>(options => options.UseSqlServer(PortalDatabase));
builder.Services.AddDbContext<DataRecoveryDbContext>(options => options.UseSqlServer(DataRecoveryString));
builder.Services.AddDbContext<JobOfferDbContext>(options => options.UseSqlServer(JobOfferDataBase));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//Added IdentityRole by Jozsef
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
options.SignIn.RequireConfirmedAccount = false)
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings to min 6 char and require Uppercase, unique char, and digit.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
});
//Identity END

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
builder.Services.AddBlazorBootstrap();

//Services added by Jozsef
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<CompanyService>();
builder.Services.AddScoped<ElevPortalenDataDbContext>();
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<JobService>();
builder.Services.AddScoped<DawaService>();
builder.Services.AddScoped<SkillService>();
builder.Services.AddScoped<AlertBox>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<RegisterModel>();
builder.Services.AddScoped<ImageUploadService>();
builder.Services.AddHttpClient();
//Dataprotection service by Jozsef
builder.Services.AddDataProtection();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(options =>
    {
        // Grab the secret value from the secret store.
        string? secretValue = builder.Configuration.GetValue<string>("CertificatePassword");
        options.ServerCertificate = new X509Certificate2("elevportalen.pfx", secretValue);
    });
});

// End ----



var app = builder.Build();

#region Check for Roles Existing
async Task CheckRolesExisting(WebApplication app)
{
    // Create a new scope within the dependency injection container by Jozsef
    using (var scope = app.Services.CreateScope())
    {
        // Obtain an instance of the RoleManager service for managing roles
        var roleManager =
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Define an array of role names to be created and seeded
        var roles = new[] { "Admin", "Student", "Company" };

        // Iterate over each role name in the array
        foreach (var role in roles)
        {
            // Check if the role already exists in the system
            if (!await roleManager.RoleExistsAsync(role))
            {
                // If the role does not exist, create a new role using RoleManager
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
#endregion

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Added database connection error handler
app.UseMiddleware<DatabaseErrorHandler>();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

//Calling Check Task here
await CheckRolesExisting(app);

app.Run();

