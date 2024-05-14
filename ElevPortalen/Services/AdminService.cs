using ElevPortalen.Data;

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


    }
}
