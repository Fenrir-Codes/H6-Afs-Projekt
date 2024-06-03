﻿using ElevPortalen.Data;
using ElevPortalen.Models;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace ElevPortalen.Services
{
    /// <summary>
    ///  Lavet af Jozsef
    /// </summary>
    public class JobService
    {
        private readonly JobOfferDbContext _context;

        #region constructor
        public JobService(JobOfferDbContext context)
        {
            _context = context;
        }
        #endregion

        #region create Job Offer
        public async Task<(string, bool)> Create(JobOfferModel Job)
        {
            try
            {
                _context.JobOfferDataBase.Add(Job); //Add data
                await _context.SaveChangesAsync(); // Save data

                return ("Job offer has been created.", true);
            }
            catch (DbUpdateException ex)
            {
                // Handle the exception and return an error message
                return ($"An error has ocurred: {ex.Message}", false);
            }
        }
        #endregion

        #region Read all job offers to list
        public async Task<(List<JobOfferModel>?, string, bool)> GetAllOffers()
        {
            try
            {
                var response = await _context.JobOfferDataBase.AsNoTracking().ToListAsync();
                return (response, "Data load success", true);
            }
            catch (DbException ex)
            {
                // Handle the exception, log, and return an error message along with false
                return (null, $"An error occurred: {ex.Message}", false);
            }
        }
        #endregion

        #region Read one offer with the job Id
        public async Task<(string, bool, JobOfferModel?)> GetOfferWithJobId(int JobId)
        {
            try
            {
                var data = await _context.JobOfferDataBase.FirstOrDefaultAsync(offer => offer.JobOfferId == JobId);
                return ("Data read successful.", true, data);
            }
            catch (Exception ex)
            {
                // Handle the exception, log, and return null or throw an error as appropriate
                throw new Exception($"An error occurred : {ex.Message}");
            }
        }
        #endregion

        #region Read one offer with the Company Id
        public async Task<(string, bool, JobOfferModel?)> GetOfferWithCompanyId(int companyId)
        {
            try
            {
                var joboffer = await _context.JobOfferDataBase.FirstOrDefaultAsync(offer => offer.CompanyId == companyId);
                if (joboffer != null)
                {
                    return ("", true, joboffer);
                }
                else
                {
                    return ("No joboffers found!", false, null);
                }
            }
            catch (Exception ex)
            {
                // Handle the exception, log, and return null or throw an error as appropriate
                throw new Exception($"An error occurred : {ex.Message}");
            }
        }
        #endregion

        #region Read all offers with the Company Id
        public async Task<(string, bool, List<JobOfferModel>)> GetAllOffersByCompanyId(int companyId)
        {
            try
            {
                var joboffer = await _context.JobOfferDataBase.Where(offer => offer.CompanyId == companyId).ToListAsync();
                if (joboffer != null)
                {
                    return ("", true, joboffer);
                }
                else
                {
                    return ("No joboffers found!", false, new());
                }
            }
            catch (Exception ex)
            {
                // Handle the exception, log, and return null or throw an error as appropriate
                throw new Exception($"An error occurred : {ex.Message}");
            }
        }
        #endregion

        #region Update Job Offer
        public async Task<(string, bool)> Update(JobOfferModel job)
        {
            try
            {
                _context.Entry(job).State = EntityState.Detached;
                var entry = await _context.JobOfferDataBase.FindAsync(job.JobOfferId);

                // If the response is not null
                if (entry != null)
                {
                    if (!AreEntitiesEqual(entry, job))
                    {
                        entry.Title = job.Title;
                        entry.ContactPerson = job.ContactPerson;
                        entry.PhoneNumber = job.PhoneNumber;
                        entry.JobAddress = job.JobAddress;
                        entry.JobLink = job.JobLink;
                        entry.JobDetails = job.JobDetails;
                        entry.NumberOfPositionsAvailable = job.NumberOfPositionsAvailable;
                        entry.Speciality = job.Speciality;
                        entry.Deadline = job.Deadline;

                        _context.Entry(entry).State = EntityState.Modified;
                        await _context.SaveChangesAsync();

                        return ("Updated successfully", true);
                    }
                    else
                    {
                        return ($"No changes were made", true);
                    }
                }
                else
                {
                    return ("Job offer not found", false);
                    // Return a message when the offer is not found
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while updating data in the database: {ex.Message}");
                // Return an error message if an exception occurs
            }
        }

        #endregion

        #region Helper method to check if two  entities are equal
        private bool AreEntitiesEqual(JobOfferModel entry, JobOfferModel jobtoUpdate)
        { //test note: private methods are not tested
            return entry.Title == jobtoUpdate.Title &&
                entry.ContactPerson == jobtoUpdate.ContactPerson &&
                entry.PhoneNumber == jobtoUpdate.PhoneNumber &&
                entry.JobAddress == jobtoUpdate.JobAddress &&
                entry.JobLink == jobtoUpdate.JobLink &&
                entry.JobDetails == jobtoUpdate.JobDetails &&
                entry.NumberOfPositionsAvailable == jobtoUpdate.NumberOfPositionsAvailable &&
                entry.Speciality == jobtoUpdate.Speciality &&
                entry.Deadline == jobtoUpdate.Deadline;
        }
        #endregion

        #region Delete Joboffer by Id (just one offer)
        public async Task<(string, bool)> DeleteOffer(int jobOfferId)
        {
            try
            {
                var JobToDelete = await _context.JobOfferDataBase.FindAsync(jobOfferId);
                if (JobToDelete != null)
                {
                    _context.JobOfferDataBase.Remove(JobToDelete);
                    await _context.SaveChangesAsync();

                    return ("JobOffer deleted successfully.", true);
                }
                else
                {
                    return ("Error while delete Joboffer.", false);
                }
            }
            catch (DbUpdateException ex)
            {
                // Handle the exception and return an error message
                throw new InvalidOperationException($"An error occurred while deleting data from the database: {ex.Message}");
            }
        }
        #endregion

        #region Delete function for all the job offers a company have in the database (all offers with the company id)
        public async Task<(string, bool)> DeleteAllOffersByCompanyId(int companyId)
        {
            try
            {
                var offersToDelete = _context.JobOfferDataBase.Where(offer => offer.CompanyId == companyId);

                if (!offersToDelete.Any())
                {
                    return ($"No offers found for this company", false);
                }

                _context.JobOfferDataBase.RemoveRange(offersToDelete);
                await _context.SaveChangesAsync();

                return ($"All offers has been deleted successfully", true);
            }
            catch (DbUpdateException ex)
            {
                // Handle the exception and return an error message
                throw new InvalidOperationException($"An error occurred while deleting data from the database: {ex.Message}");
            }
        }
        #endregion


    }
}
