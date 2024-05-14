using ElevPortalen.Data;
using ElevPortalen.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace ElevPortalen.Services
{
    public class AdminService
    {
        // Dsatabase Contexts
        private readonly ElevPortalenDataDbContext _DataDbcontext;
        private readonly DataRecoveryDbContext _recoveryContext;
        private readonly ApplicationDbContext _LoginDbContext;
        private readonly JobOfferDbContext _jobOfferDbContext;

        #region constructor
        public AdminService(ElevPortalenDataDbContext DataDbcontext, DataRecoveryDbContext recoveryContext,
            ApplicationDbContext loginDbContext, JobOfferDbContext jobOfferDbContext)
        {
            _DataDbcontext = DataDbcontext;
            _recoveryContext = recoveryContext;
            _LoginDbContext = loginDbContext;
            _jobOfferDbContext = jobOfferDbContext;
        }
        #endregion


        // Name the functions as you like, and add more if needed.  service is integrated allready in program cs

        //Create
        // Read
        // Update  ( Company and student update are already working, I'll just note that they are created with a completely different method. )
        //Delete
        //Other functions
        #region Get registration date
        public async Task<Dictionary<string, double>> GetMonthlyRegistrationCountsStudent()
        {
            var monthlyCounts = await _DataDbcontext.Student
                .GroupBy(s => new { s.RegisteredDate.Year, s.RegisteredDate.Month })
                .Select(g => new {
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

        #region Get registration date
        public async Task<Dictionary<string, double>> GetMonthlyRegistrationCountsCompany()
        {
            var monthlyCounts = await _DataDbcontext.Company
                .GroupBy(s => new { s.RegisteredDate.Year, s.RegisteredDate.Month })
                .Select(g => new {
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
}
