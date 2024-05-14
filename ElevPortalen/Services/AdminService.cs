using ElevPortalen.Data;
using ElevPortalen.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace ElevPortalen.Services
{
    public class AdminService
    {
        // Dsatabase Contexts
        private readonly ElevPortalenDataDbContext _DataDbcontext;
        private readonly DataRecoveryDbContext _recoveryContext;
        private readonly ApplicationDbContext _LoginDbContext;
        private readonly JobOfferDbContext _jobOfferDbContext;

        private readonly UserManager<IdentityUser> _userManager;

        #region constructor
        public AdminService(ElevPortalenDataDbContext DataDbcontext, DataRecoveryDbContext recoveryContext,
            ApplicationDbContext loginDbContext, JobOfferDbContext jobOfferDbContext,
            UserManager<IdentityUser> userManager)
        {
            _DataDbcontext = DataDbcontext;
            _recoveryContext = recoveryContext;
            _LoginDbContext = loginDbContext;
            _jobOfferDbContext = jobOfferDbContext;

            _userManager = userManager;

        }
        #endregion

        // CreateAdmin function to create a new admin user
        public async Task<(bool success, string message)> CreateAdmin(string email, string password) {
            var adminUser = new IdentityUser { UserName = email, Email = email };

            var result = await _userManager.CreateAsync(adminUser, password);
            if (result.Succeeded) {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                return (true, "Admin user created.");
            } else {
                // Handle creation failure, such as displaying errors
                return (false, "Failed to create admin user.");
            }
        }

        // VerifyStudent method to verify students:
        public async Task<(bool success, string message)> VerifyStudent(StudentModel student) {
            try {
                // Update the IsVerified property of the student
                student.IsVerified = true;

                // Update the entity in the context and save changes
                _DataDbcontext.Update(student);
                await _DataDbcontext.SaveChangesAsync();

                return (true, "Student verified successfully.");
            } catch (Exception ex) {
                return (false, $"An error occurred while verifying the student: {ex.Message}");
            }
        }


        // Name the functions as you like, and add more if needed.  service is integrated allready in program cs

        //Create
        // Read
        // Update  ( Company and student update are already working, I'll just note that they are created with a completely different method. )
        //Delete
        //Other functions


    }
}
