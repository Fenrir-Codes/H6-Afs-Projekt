using ElevPortalen.Data;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using ElevPortalen.Models;
using Microsoft.AspNetCore.DataProtection;

namespace ElevPortalen.Services {
    public class AdminService {
        // Dsatabase Contexts
        private readonly ElevPortalenDataDbContext _DataDbcontext;
        private readonly DataRecoveryDbContext _recoveryContext;
        private readonly ApplicationDbContext _loginDbContext;
        private readonly JobOfferDbContext _jobOfferDbContext;

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        #region constructor
        public AdminService(
            ElevPortalenDataDbContext DataDbcontext, DataRecoveryDbContext recoveryContext,
            ApplicationDbContext loginDbContext, JobOfferDbContext jobOfferDbContext,
            UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager) {
            _DataDbcontext = DataDbcontext;
            _recoveryContext = recoveryContext;
            _loginDbContext = loginDbContext;
            _jobOfferDbContext = jobOfferDbContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        #endregion


        // Name the functions as you like, and add more if needed.  service is integrated allready in program cs

        //Create
        // Read
        // Update  ( Company and student update are already working, I'll just note that they are created with a completely different method. )
        //Delete
        //Other functions




    }
}
