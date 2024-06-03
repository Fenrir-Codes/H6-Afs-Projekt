using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElevPortalen.Areas.Identity;
using ElevPortalen.Helpers;
using ElevPortalen;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ElevPortalen.Data;
using Microsoft.EntityFrameworkCore;
using ElevPortalen.Models;
using ElevPortalen.Services;
using Moq;
using Microsoft.AspNetCore.DataProtection;

namespace ElevPortalenTests {
    public class IdentityTests {

        /// <summary>
        /// Identity Authorization Test with HTML check
        /// Purpose of tests is to check if views change based on the Identity user logged into the system
        /// </summary>

        private readonly DbContextOptions<ApplicationDbContext> _appDbContextOptions;
        private readonly ApplicationDbContext _appDbContext;
        private readonly DbContextOptions<ElevPortalenDataDbContext> _dataDbContextOptions;
        private readonly ElevPortalenDataDbContext _dataDbContext;
        private readonly DbContextOptions<DataRecoveryDbContext> _recoveryDbContextOptions;
        private readonly DataRecoveryDbContext _recoveryDbContext;
        private readonly CustomRoleHandler _customRoleHandler;
        private readonly Mock<IStudentService> _mockStudentService;
        private readonly Mock<ISkillService> _mockSkillService;
        private readonly Mock<IDataProtectionProvider> _mockDataProtectionProvider;

        #region Constructor
        public IdentityTests()
        {
            // Setup in-memory database options for ApplicationDbContext
            _appDbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "ApplicationDb")
                .Options;

            // Setup in-memory database options for ElevPortalenDataDbContext
            _dataDbContextOptions = new DbContextOptionsBuilder<ElevPortalenDataDbContext>()
                .UseInMemoryDatabase(databaseName: "ElevPortalenDataDb")
                .Options;

            // Setup in-memory database options for DataRecoveryDbContext
            _recoveryDbContextOptions = new DbContextOptionsBuilder<DataRecoveryDbContext>()
                .UseInMemoryDatabase(databaseName: "DataRecoveryDb")
                .Options;

            // Initialize ApplicationDbContext
            _appDbContext = new ApplicationDbContext(_appDbContextOptions);

            // Initialize ElevPortalenDataDbContext
            _dataDbContext = new ElevPortalenDataDbContext(_dataDbContextOptions);

            // Initialize DataRecoveryDbContext
            _recoveryDbContext = new DataRecoveryDbContext(_recoveryDbContextOptions);

            // Mock StudentService and SkillService
            _mockStudentService = new Mock<IStudentService>();
            _mockSkillService = new Mock<ISkillService>();

            // Mock DataProtectionProvider
            _mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
            var dataProtectorMock = new Mock<IDataProtector>();
            _mockDataProtectionProvider.Setup(x => x.CreateProtector(It.IsAny<string>())).Returns(dataProtectorMock.Object);

            // Initialize CustomRoleHandler
            _customRoleHandler = new CustomRoleHandler();
        }
        #endregion


        #region Check Login Name
        [Fact]
        public async Task CheckNavigationalLoginNameMatch() {

            // Arrange
            //await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear
            using var context = new TestContext(); // Make a disposable sandbox of the project that can be discarded afterwards
            context.Services.AddSingleton(new CustomRoleHandler()); //Call on our custom role assignment class to handle Authorization and roles

            context.Services.AddScoped<AuthenticationStateProvider,
                RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>(); // Add identity service to validation authorization

            var authorizationContext = context.AddTestAuthorization(); // Create a new context using the sandbox and the built in test authorization method
            authorizationContext.SetAuthorized("test@ElevPortalen.dk");    // Create a new authorized user

            // Act
            var index = context.RenderComponent<ElevPortalen.Shared.LoginDisplay>(); //Render the login display

            //Assert
            //Check that user was logged in and has been shown the default HTML message
            index.MarkupMatches(@"    <a href=""Identity/Account/Manage"">Hello, test@ElevPortalen.dk!</a>
                                <form method=""post"" action=""Identity/Account/Logout"">
                                  <button type=""submit"" class=""btn btn-outline-danger m-3"">Log out</button>
                                </form>");

        }
        #endregion

        #region Check Admin HTML 
        // Purpose of method:
        // Show that a logged in Admin is shown the correct HTML
        [Fact]
        public async Task CheckAdminIdentityAndHTML() {
            // Arrange
            using var context = new TestContext(); // Make a disposable sandbox of the project that can be discarded afterwards

            // Configure identity services
            context.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("InMemoryDb"));

            context.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();


            context.Services.AddScoped<CustomRoleHandler>(); // Call on our custom role assignment class to handle Authorization and roles
            context.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>(); // Add identity service to validation authorization

            var serviceProvider = context.Services.BuildServiceProvider();
            var customRoleHandler = serviceProvider.GetRequiredService<CustomRoleHandler>();


            // Create the necessary roles and assign them to the user
            var userEmail = "testAdmin@ElevPortalen.dk";
            var userRole = "Admin";

            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var user = new IdentityUser { UserName = userEmail, Email = userEmail };
            await userManager.CreateAsync(user, "Admin.Passw0rd");

            await customRoleHandler.CreateUserRoles(userEmail, userRole, serviceProvider);

            var authorizationContext = context.AddTestAuthorization(); // Create a new context using the sandbox and the built in test authorization method
            authorizationContext.SetAuthorized(userEmail);    // Create a new authorized user

            // Set the role for the authorized user
            authorizationContext.SetRoles(userRole);


            // Act
            var index = context.RenderComponent<ElevPortalen.Pages.Index>(); //Render the index display - this display leads to a variety of home pages based on role
            var roles = await userManager.GetRolesAsync(user); // Get role of user

            // Assert
            Assert.Contains(userRole, roles); // Assert that assigned role is current role

            // Check what HTML is shown here, should say "Velkommen Admin"
            var expectedHtml = @"    <div class=""container-fluid text-center"" >
                                      <div class=""container-fluid d-flex flex-column align-items-center justify-content-center position-relative"">
                                        <div class=""bg-image-container text-center position-absolute top-100 start-50 translate-middle-x"" style=""z-index: 1;"">
                                          <img src=""/images/Logo_ElevPortalen.png"" class=""bg-image"" style=""max-width: 75%; height: auto; opacity: 0.1;"">
                                        </div>
                                        <div class=""text-container mb-5"" style=""text-align: center; z-index: 2;"">
                                          <h3 class=""w-100"">
                                            <span class=""badge bg-secondary m-5"">Velkommen Admin</span>
                                          </h3>
                                        </div>
                                      </div>
                                    </div>";

            index.MarkupMatches(expectedHtml);

            //Assert.True(user.);
        }
        #endregion

        #region Check company HTML 
        // Purpose of method:
        // Show that a logged in Company is shown the correct HTML

        [Fact]
        public async Task CheckCompanyIdentityAndHTML() {
            // Arrange
            using var context = new TestContext(); // Create a disposable sandbox of the project that can be discarded afterward

            // Configure identity services
            context.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("ApplicationDb"));

            // Configure ElevPortalenDataDbContext
            context.Services.AddDbContext<ElevPortalenDataDbContext>(options =>
                options.UseInMemoryDatabase("ElevPortalenDataDb"));

            // Configure DataRecoveryDbContext
            context.Services.AddDbContext<DataRecoveryDbContext>(options =>
                options.UseInMemoryDatabase("DataRecoveryDb"));

            context.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add the required services and mocked dependencies
            context.Services.AddScoped<CustomRoleHandler>(_ => _customRoleHandler);
            context.Services.AddScoped<IStudentService>(_ => _mockStudentService.Object);
            context.Services.AddScoped<ISkillService>(_ => _mockSkillService.Object);
            context.Services.AddScoped<IDataProtectionProvider>(_ => _mockDataProtectionProvider.Object);
            context.Services.AddScoped<StudentService>(provider =>
                new StudentService(
                    provider.GetRequiredService<ElevPortalenDataDbContext>(),
                    provider.GetRequiredService<DataRecoveryDbContext>(),
                    provider.GetRequiredService<IDataProtectionProvider>()
                ));
            context.Services.AddScoped<SkillService>();
            context.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();

            var serviceProvider = context.Services.BuildServiceProvider();
            var customRoleHandler = serviceProvider.GetRequiredService<CustomRoleHandler>();

            // Create the necessary roles and assign them to the user
            var userEmail = "TestCompany@TestCompany.dk";
            var userRole = "Company";

            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var user = new IdentityUser { UserName = userEmail, Email = userEmail };
            await userManager.CreateAsync(user, "123.Passw0rd");

            await customRoleHandler.CreateUserRoles(userEmail, userRole, serviceProvider);

            var authorizationContext = context.AddTestAuthorization(); // Create a new context using the sandbox and the built-in test authorization method
            authorizationContext.SetAuthorized(userEmail);    // Create a new authorized user

            // Set the role for the authorized user
            authorizationContext.SetRoles(userRole);

            // Act
            var index = context.RenderComponent<ElevPortalen.Pages.Index>(); // Render the index display - this display leads to a variety of home pages based on role

            var roles = await userManager.GetRolesAsync(user); // Get role of user

            // Assert
            Assert.Contains(userRole, roles); // Assert that assigned role is current role

            // Check what HTML is shown here, should contain information about the students
            var expectedHtml = @"
                                    <div class=""container-fluid text-center"" >
                                      <div class=""row"" >
                                        <div class=""col-lg-12"" >
                                          <div class=""d-flex justify-content-center w-100 mb-4"" >
                                            <div class=""input-group mb-3"" style=""max-width:600px;"">
                                              <input type=""text"" class=""form-control"" placeholder=""Search...""  >
                                            </div>
                                          </div>
                                        </div>
                                      </div>
                                      <div class=""container fixed-bottom"" style=""max-width:600px;"">
                                        <div class=""alert alert-warning d-flex align-items-center justify-content-center shadow"" role=""alert"">
                                          <i class=""fa-solid fa-circle-info fa-xl me-3""></i>
                                          <span class=""fs-5"">Ingen studenter er registreret endnu.</span>
                                        </div>
                                      </div>
                                    </div>";

            index.MarkupMatches(expectedHtml);
        }
        #endregion

        #region Check student HTML
        // Purpose of method:
        // Show that a logged in Student is shown the correct HTML

        [Fact]
        public async Task CheckStudentIdentityAndHTML() {
            // Arrange
            using var context = new TestContext(); // Create a disposable sandbox of the project that can be discarded afterward

            // Configure identity services
            context.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("ApplicationDb"));

            // Configure ElevPortalenDataDbContext
            context.Services.AddDbContext<ElevPortalenDataDbContext>(options =>
                options.UseInMemoryDatabase("ElevPortalenDataDb"));

            // Configure DataRecoveryDbContext
            context.Services.AddDbContext<DataRecoveryDbContext>(options =>
                options.UseInMemoryDatabase("DataRecoveryDb"));

            context.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add the required services and mocked dependencies
            context.Services.AddScoped<CustomRoleHandler>(_ => _customRoleHandler);
            context.Services.AddScoped<ISkillService>(_ => _mockSkillService.Object);
            context.Services.AddScoped<IDataProtectionProvider>(_ => _mockDataProtectionProvider.Object);
            context.Services.AddScoped<CompanyService>(provider =>
                new CompanyService(
                    provider.GetRequiredService<ElevPortalenDataDbContext>(),
                    provider.GetRequiredService<DataRecoveryDbContext>(),
                    provider.GetRequiredService<IDataProtectionProvider>()
                ));
            context.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();

            var serviceProvider = context.Services.BuildServiceProvider();
            var customRoleHandler = serviceProvider.GetRequiredService<CustomRoleHandler>();

            // Create the necessary roles and assign them to the user
            var userEmail = "TestStudent@TestStudent.dk";
            var userRole = "Student";

            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var user = new IdentityUser { UserName = userEmail, Email = userEmail };
            await userManager.CreateAsync(user, "123.Passw0rd");

            await customRoleHandler.CreateUserRoles(userEmail, userRole, serviceProvider);

            var authorizationContext = context.AddTestAuthorization(); // Create a new context using the sandbox and the built-in test authorization method
            authorizationContext.SetAuthorized(userEmail);    // Create a new authorized user

            // Set the role for the authorized user
            authorizationContext.SetRoles(userRole);

            // Act
            var index = context.RenderComponent<ElevPortalen.Pages.Index>(); // Render the index display - this display leads to a variety of home pages based on role

            var roles = await userManager.GetRolesAsync(user); // Get role of user

            // Assert
            Assert.Contains(userRole, roles); // Assert that assigned role is current role

            // Check what HTML is shown here, should say "Velkommen Student"
            var expectedHtml = @"<div class=""container-fluid text-center"" >
                                  <div class=""row"" >
                                    <div class=""col-lg-12"" >
                                      <div class=""d-flex justify-content-center w-100 mb-4"" >
                                        <div class=""input-group mb-3"" style=""max-width:600px;"">
                                          <input type=""text"" class=""form-control"" placeholder=""Search...""  >
                                        </div>
                                      </div>
                                    </div>
                                  </div>
                                  <div class=""container fixed-bottom"" style=""max-width:600px;"">
                                    <div class=""alert alert-warning d-flex align-items-center justify-content-center shadow"" role=""alert"">
                                      <i class=""fa-solid fa-circle-info fa-xl me-3""></i>
                                      <span class=""fs-5"">Ingen virksomhed er registreret endnu.</span>
                                    </div>
                                  </div>
                                </div>";

            index.MarkupMatches(expectedHtml);
        }

        #endregion



    }
}
