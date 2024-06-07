using Microsoft.AspNetCore.Identity;

namespace ElevPortalen.Helpers {
    public class CustomRoleHandler {

        // Custom role handler to handle assignment of roles in test project


        public async Task CreateUserRoles(string user, string role, IServiceProvider serviceProvider) {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var userRoleCheck = await roleManager.RoleExistsAsync(role);
            if (!userRoleCheck) {
                await roleManager.CreateAsync(new IdentityRole(role));
            }

            IdentityUser identityUser = await userManager.FindByEmailAsync(user);
            if (identityUser != null) {
                await userManager.AddToRoleAsync(identityUser, role);
            }
        }



    }
}
