using ElevPortalen.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ElevPortalen.Services
{
    public class AdminService : IAdminService
    {
        // Dsatabase Contexts
        private readonly ElevPortalenDataDbContext _DataDbcontext;
        private readonly DataRecoveryDbContext _recoveryContext;
        private readonly ApplicationDbContext _LoginDbContext;
        private readonly JobOfferDbContext _jobOfferDbContext;
        private readonly UserManager<IdentityUser> _userManager;

        #region constructor
        public AdminService(ElevPortalenDataDbContext DataDbcontext, DataRecoveryDbContext recoveryContext,
            ApplicationDbContext loginDbContext, JobOfferDbContext jobOfferDbContext, UserManager<IdentityUser> userManager)
        {
            _DataDbcontext = DataDbcontext;
            _recoveryContext = recoveryContext;
            _LoginDbContext = loginDbContext;
            _jobOfferDbContext = jobOfferDbContext;
            _userManager = userManager;
        }
        #endregion

        // Name the functions as you like, and add more if needed.  service is integrated allready in program cs

        //Create
        // Read
        // Update  ( Company and student update are already working, I'll just note that they are created with a completely different method. )
        //Delete
        //Other functions

        #region Get registration date Student
        public async Task<Dictionary<string, double>> GetMonthlyRegistrationCountsStudent()
        {
            var monthlyCounts = await _DataDbcontext.Student
                .GroupBy(s => new { s.RegisteredDate.Year, s.RegisteredDate.Month })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            // Convert month number to month name
            var monthNames = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;
            return monthlyCounts.ToDictionary(x => monthNames[x.Month - 1] + " " + x.Year, x => (double)x.Count);
        }

        #endregion

        #region Get registration date Company
        public async Task<Dictionary<string, double>> GetMonthlyRegistrationCountsCompany()
        {
            var monthlyCounts = await _DataDbcontext.Company
                .GroupBy(s => new { s.RegisteredDate.Year, s.RegisteredDate.Month })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            // Convert month number to month name
            var monthNames = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;
            return monthlyCounts.ToDictionary(x => monthNames[x.Month - 1] + " " + x.Year, x => (double)x.Count);
        }

        #endregion

    }

    public interface IAdminService {

        Task<Dictionary<string, double>> GetMonthlyRegistrationCountsStudent();

        Task<Dictionary<string, double>> GetMonthlyRegistrationCountsCompany();


    }
}
