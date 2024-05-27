﻿using ElevPortalen.Data;
using ElevPortalen.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace ElevPortalen.Services
{
    /// <summary>
    ///  Lavet af Jozsef
    /// </summary>
    public class CompanyService
    {
        private readonly ElevPortalenDataDbContext _context;
        private readonly DataRecoveryDbContext _recoveryContext;
        private readonly IDataProtector? _dataProtector;

        #region constructor
        public CompanyService(ElevPortalenDataDbContext context, DataRecoveryDbContext recoveryContext, IDataProtectionProvider dataProtectionProvider)
        {
            _context = context;
            _recoveryContext = recoveryContext;
            _dataProtector = dataProtectionProvider.CreateProtector("ProtectData");
            //i just placed it here if need, we can use it to protect data
        }
        #endregion

        #region create Company function async
        public async Task<(string?, bool)> CreateCompany(CompanyModel company)
        {
            try
            {
                _context.Company.Add(company); // Add input to context variables
                await _context.SaveChangesAsync(); // Save data

                return ("Company Profile Created.", true);
            }
            catch (Exception ex)
            {
                // Handle the exception and return an error message
                return ($"An error has ocurred: {ex.Message}", false);
            }
        }
        #endregion

        #region Get Company request with the claimprincipal
        public async Task<List<CompanyModel>> ReadData(ClaimsPrincipal _user)
        {
            try
            {
                //Get all the data
                var response = await _context.Company.AsNoTracking().Where(user => user.UserId
                    == Guid.Parse(_user.FindFirstValue(ClaimTypes.NameIdentifier))).ToListAsync();

                return response; // return the decrypted data
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while retrieving Company data." + ex.Message);
            }
        }
        #endregion

        #region Get All Data from Company if they are visible
        public async Task<List<CompanyModel>> ReadAllVisibleCompanyData()
        {
            try
            {
                //Get all the data
                var response = await _context.Company.Where(c => c.IsVisible == true).AsNoTracking().ToListAsync();

                return response; // return the data
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while retrieving Company data." + ex.Message);
            }
        }
        #endregion

        #region Get All Data from Company 
        public async Task<List<CompanyModel>> ReadAllCompanyData()
        {
            try
            {
                //Get all the data
                var response = await _context.Company.AsNoTracking().ToListAsync();

                return response; // return the data
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while retrieving Company data." + ex.Message);
            }
        }
        #endregion

        #region Company Update function
        public async Task<(string, bool)> Update(CompanyModel company)
        {
            try
            {
                _context.Entry(company).State = EntityState.Detached;
                var entry = await _context.Company.FindAsync(company.CompanyId);

                // If the response is not null
                if (entry != null)
                {
                    if (!AreEntitiesEqual(entry, company))
                    {
                        // Update entity properties
                        entry.CompanyName = company.CompanyName;
                        entry.CompanyAddress = company.CompanyAddress;
                        entry.Region = company.Region;
                        entry.Email = company.Email;
                        entry.Link = company.Link;
                        entry.Preferences = company.Preferences;
                        entry.Description = company.Description;
                        entry.profileImage = company.profileImage;
                        entry.PhoneNumber = company.PhoneNumber;
                        entry.IsHiring = company.IsHiring;
                        entry.IsVisible = company.IsVisible;
                        entry.UpdatedDate = DateTime.Now;

                        _context.Entry(entry).State = EntityState.Modified;
                        await _context.SaveChangesAsync();

                        return ($"Updated successfully", true);
                    }
                    else
                    {
                        return ($"No changes were made", true);
                    }
                }
                else
                {
                    return ($"Update failed.", false);  // Return a message when update failed
                }

            }
            catch (Exception ex)
            {
                // Return an error message if an exception occurs
                throw new InvalidOperationException($"Error occurred while updating: {ex.Message}");
            }
        }
        #endregion

        #region Helper method to check if two CompanyModel entities are equal
        private bool AreEntitiesEqual(CompanyModel entry, CompanyModel company)
        {
            return entry.CompanyName == company.CompanyName &&
            entry.CompanyAddress == company.CompanyAddress &&
            entry.Region == company.Region &&
            entry.Email == company.Email &&
            entry.Link == company.Link &&
            entry.Preferences == company.Preferences &&
            entry.Description == company.Description &&
            entry.profileImage == company.profileImage &&
            entry.PhoneNumber == company.PhoneNumber &&
            entry.IsHiring == company.IsHiring &&
            entry.IsVisible == company.IsVisible;
        }
        #endregion

        #region Delete Company function
        public async Task<(string, bool)> Delete(int companytId)
        {
            try
            {
                var company = await _context.Company.FindAsync(companytId);
                if (company != null)
                {
                    await CreateRecoveryData(company); // First create a recovery data

                    var entryToRemove = _context.Company.Local.FirstOrDefault(c => c.CompanyId == company.CompanyId);
                    if (entryToRemove != null)
                    {
                        _context.Entry(entryToRemove).State = EntityState.Detached;
                    }

                    _context.Company.Remove(company);
                    await _context.SaveChangesAsync();

                    return ("The Company Profile was deleted Successfully.", true);
                }
                else
                {
                    return ("Company not found.", false);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error has occurred in delete function: {ex.Message}");
            }
        }
        #endregion

        #region Get Company by Id (Model)
        public async Task<CompanyModel> GetCompanyById(int companyId)
        {
            try
            {
                var company = await _context.Company.AsNoTracking().FirstOrDefaultAsync(c => c.CompanyId == companyId);
                if (company != null)
                {
                    return company;
                }
                else
                {
                    throw new InvalidOperationException("Company not found.");
                }

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while retrieving Company data: {ex.Message}");
            }
        }
        #endregion

        #region Get Company by Id to list
        public async Task<List<CompanyModel>> GetCompanyByIdToList(int Id)
        {
            try
            {
                var company = await _context.Company
                    .Where(c => c.CompanyId == Id).ToListAsync();

                if (company != null)
                {
                    return company;
                }
                else
                {
                    // Throw an exception if no student found
                    throw new InvalidOperationException($"No company found with Id: {Id}");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"{ex.Message}");
            }
        }
        #endregion

        #region Company count
        public async Task<int> GetCompaniesCountAsync()
        {
            try
            {
                // Get all the data
                int response = await _context.Company.CountAsync();

                return response; // return the data
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while retrieving Student data." + ex.Message);
            }
        }
        #endregion

        #region create Recovery data function async for CompanyModel
        public async Task<string> CreateRecoveryData(CompanyModel deletedCompany)
        {
            try
            {
                var recoveryData = new CompanyRecoveryModel
                {
                    UserId = deletedCompany.UserId,
                    CompanyId = deletedCompany.CompanyId,
                    CompanyName = deletedCompany.CompanyName,
                    CompanyAddress = deletedCompany.CompanyAddress,
                    Region = deletedCompany.Region,
                    Email = deletedCompany.Email,
                    Link = deletedCompany.Link,
                    Preferences = deletedCompany.Preferences,
                    Description = deletedCompany.Description,
                    profileImage = deletedCompany.profileImage,
                    PhoneNumber = deletedCompany.PhoneNumber,
                    IsHiring = deletedCompany.IsHiring,
                    IsVisible = deletedCompany.IsVisible,
                    RegisteredDate = deletedCompany.RegisteredDate,
                    RecoveryCreatedDate = DateTime.Now
                };

                _recoveryContext.CompanyDataRecovery.Add(recoveryData); // Add input to context variables
                await _recoveryContext.SaveChangesAsync(); // Save data

                return $"Company Recovery Created";
            }
            catch (Exception ex)
            {
                // Handle the exception and return an error message
                throw new InvalidOperationException($"An error has occurred: {ex.Message}");
            }
        }
        #endregion

        #region Function to check if Company data exist in the recovery database
        public async Task<bool> CheckRecoveryDataExist(Guid id)
        {
            var data = await _recoveryContext.CompanyDataRecovery.Where(s => s.UserId == id).FirstOrDefaultAsync();
            if (data != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Recover the data function for Company
        public async Task<(string, bool)> RecoverCompanyData(Guid id)
        {
            try
            {
                // Retrieve recovery data based on the provided Guid
                var recoveryData = await _recoveryContext.CompanyDataRecovery
                    .FirstOrDefaultAsync(s => s.UserId == id);

                if (recoveryData != null)
                {
                    var recoveredCompany = new CompanyModel
                    {
                        UserId = recoveryData.UserId,
                        CompanyName = recoveryData.CompanyName,
                        CompanyAddress = recoveryData.CompanyAddress,
                        Region = recoveryData.Region,
                        Email = recoveryData.Email,
                        Link = recoveryData.Link,
                        Preferences = recoveryData.Preferences,
                        Description = recoveryData.Description,
                        profileImage = recoveryData.profileImage,
                        PhoneNumber = recoveryData.PhoneNumber,
                        IsHiring = recoveryData.IsHiring,
                        IsVisible = recoveryData.IsVisible,
                        RegisteredDate = recoveryData.RegisteredDate,
                        UpdatedDate = DateTime.Now
                    };
                    // Add the recovered company to the main context
                    _context.Company.Add(recoveredCompany);

                    // Remove the recovery data from the recovery context
                    _recoveryContext.CompanyDataRecovery.Remove(recoveryData);

                    // Save changes to both contexts
                    await _context.SaveChangesAsync();
                    await _recoveryContext.SaveChangesAsync();

                    return ("Data successfully recovered.", true);
                }
                else
                {
                    // Return a message indicating that recovery data does not exist
                    return ("No recovery data found for UserId: {id}.", false);
                }
            }
            catch (Exception ex)
            {
                // Return an error message if an exception occurs
                throw new InvalidOperationException($"Error recovering data: {ex.Message}");
            }
        }
        #endregion

        #region Get a Company by its Guid
        public async Task<CompanyModel?> GetCompanyByGuid(Guid id)
        {
            try
            {
                var company = await _context.Company.FirstOrDefaultAsync(c => c.UserId == id);

                if (company != null)
                {
                    return company;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {

                throw new InvalidOperationException($"An error occurred while retrieving company data: {ex.Message}");

            }
        }
        #endregion

    }
}
