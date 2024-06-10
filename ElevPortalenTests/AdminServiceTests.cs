using ElevPortalen.Data;
using ElevPortalen.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevPortalenTests {
    public class AdminServiceTests {

        //mocked depencies for unit testing:
        private readonly Mock<UserManager<IdentityUser>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly AdminService _adminUnitService;

        //implemented services for integration testing:
        private readonly AdminService _adminService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _loginDbContext;


        public AdminServiceTests() {
            #region setup for unit testing
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            _userManagerMock = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object, null, null, null, null);

            _adminService = new AdminService(null, null, null, null, _userManagerMock.Object);
            #endregion

            #region setup integration testing
            var services = new ServiceCollection();

            services.AddDbContext<ApplicationDbContext>
                (options =>options
                .UseInMemoryDatabase("ApplicationDb"));

            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            var serviceProvider = services.BuildServiceProvider();

            _loginDbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            _userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            _roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            _adminService = new AdminService(null, null, _loginDbContext, null, _userManager);

            #endregion
        }

        #region Mocked Create Admin should return success when valid
        [Fact]
        public async Task CreateAdmin_ShouldReturnSuccess_WhenUserIsCreatedMock() {
            // Arrange
            var email = "test@admin.com";
            var password = "Test@1234";
            var user = new IdentityUser { UserName = email, Email = email };
            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), password)).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(um => um.AddToRoleAsync(It.IsAny<IdentityUser>(), "Admin")).ReturnsAsync(IdentityResult.Success);
            // Act
            var (success, message) = await _adminUnitService.CreateAdmin(email, password);
            // Assert
            Assert.True(success);
            Assert.Equal("Admin user created.", message);
        }
        #endregion

        #region Mocked Create admin should when when set to fail
        [Fact]
        public async Task CreateAdmin_ShouldReturnFailure_WhenUserCreationFailsMock() {
            // Arrange
            var email = "test@admin.com";
            var password = "Test@1234";
            var user = new IdentityUser { UserName = email, Email = email };

            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<IdentityUser>(), password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Failed to create user" }));

            // Act
            var (success, message) = await _adminUnitService.CreateAdmin(email, password);

            // Assert
            Assert.False(success);
            Assert.Equal("Failed to create admin user.", message);
        }
        #endregion

        #region Create Admin should return success when valid and admin should be in DB
        [Fact]
        public async Task CreateAdmin_ShouldReturnSuccess_WhenUserIsCreated() {
            // Arrange
            await _loginDbContext.Database.EnsureDeletedAsync(); //Ensure InMemory db is clear
            var email = "test@admin.com";
            var password = "Test@1234";
            // Act
            var (success, message) = await _adminService.CreateAdmin(email, password);
            // Assert
            Assert.True(success);
            Assert.Equal("Admin user created.", message);

            var user = await _userManager.FindByEmailAsync(email); // Verify user creation
            Assert.NotNull(user);    
            var roles = await _userManager.GetRolesAsync(user); // Verify role assignment
            Assert.Contains("Admin", roles);
        }
        #endregion

        #region Create Admin should return failure when admin is not found in DB
        [Fact]
        public async Task CreateAdmin_ShouldReturnFailure_WhenUserCreationFails() {
            // Arrange
            await _loginDbContext.Database.EnsureDeletedAsync(); //Ensure InMemory db is clear
            var email = "invalid-email";
            var password = "Test@1234";

            // Act
            var (success, message) = await _adminService.CreateAdmin(email, password);

            // Assert
            Assert.False(success);
            Assert.Equal("Failed to create admin user.", message);
        }
        #endregion
    }


}




