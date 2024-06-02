﻿using ElevPortalen.Data;
using ElevPortalen.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace ElevPortalen.Services
{
    /// <summary>
    ///  Lavet af Jozsef
    /// </summary>
    public class SkillService : ISkillService
    {
        private readonly ElevPortalenDataDbContext _context;
        private readonly IDataProtector? _dataProtector;

        #region constructor
        public SkillService(ElevPortalenDataDbContext context, IDataProtectionProvider dataProtectionProvider)
        {
            _context = context;
            _dataProtector = dataProtectionProvider.CreateProtector("ProtectData");
            //i just placed it here if need, we can use it to protect data
        }
        #endregion

        #region Getting the skill Name as string to List
        public async Task<List<string>> GetSkills(StudentModel data)
        {
            if (data == null || data.Skills == null)
            {
                return null!;
            }

            List<string> skills = await Task.Run(() =>
                typeof(SkillModel).GetProperties()
                    .Where(skill => skill.PropertyType == typeof(bool) && (bool)skill.GetValue(data.Skills))
                    .Select(skill => skill.Name)
                    .ToList());

            return skills;
        }
        #endregion

        #region Getting the skill Name by Id as string to List
        public async Task<List<string>> GetSkillsById(int id)
        {
            var skilldata = await _context.StudentSkills.FirstOrDefaultAsync(s => s.StudentId == id);

            if (skilldata == null)
            {
                return null!;
            }

            List<string> skills = await Task.Run(() =>
                typeof(SkillModel).GetProperties()
                    .Where(skill => skill.PropertyType == typeof(bool) && (bool)skill.GetValue(skilldata))
                    .Select(skill => skill.Name)
                    .ToList());

            return skills;
        }
        #endregion

        #region Create Skills
        public async Task<(string?, bool)> CreateSkills(int studentId, SkillModel newSkills)
        {
            try
            {
                // Check if the student already has skills
                var existingSkills = await _context.StudentSkills.FirstOrDefaultAsync(s => s.StudentId == studentId);

                if (existingSkills != null)
                {
                    // Student already has skills, update the existing skills
                    existingSkills.CSharp = newSkills.CSharp;
                    existingSkills.Java = newSkills.Java;
                    existingSkills.DotNet = newSkills.DotNet;
                    existingSkills.Typescript = newSkills.Typescript;
                    existingSkills.Python = newSkills.Python;
                    existingSkills.PHP = newSkills.PHP;
                    existingSkills.CPlusPlus = newSkills.CPlusPlus;
                    existingSkills.C = newSkills.C;
                    existingSkills.Bootstrap = newSkills.Bootstrap;
                    existingSkills.Blazor = newSkills.Blazor;
                    existingSkills.JavaScript = newSkills.JavaScript;
                    existingSkills.HTML = newSkills.HTML;
                    existingSkills.CSS = newSkills.CSS;
                    existingSkills.SQL = newSkills.SQL;
                    existingSkills.OfficePack = newSkills.OfficePack;
                    existingSkills.CloudComputing = newSkills.CloudComputing;
                    existingSkills.VersionControl = newSkills.VersionControl;
                    existingSkills.NetWork = newSkills.NetWork;
                    existingSkills.ProblemSolving = newSkills.ProblemSolving;
                    existingSkills.Communikation = newSkills.Communikation;
                    existingSkills.TeamWorking = newSkills.TeamWorking;
                    existingSkills.WillingToLearn = newSkills.WillingToLearn;
                }

                // Create new skills entry
                newSkills.StudentId = studentId;
                _context.StudentSkills.Add(newSkills);
                await _context.SaveChangesAsync();

                return ("Skills were added successfully.", true);
            }
            catch (Exception ex)
            {
                // Handle the exception and return an error message
                return ($"An error has ocurred: {ex.Message}", false);
            }
        }
        #endregion

        #region Retrieve the skills with StudentId
        public async Task<(string?, SkillModel?)> GetSkillsByStudentId(int studentId)
        {
            try
            {
                var skills = await _context.StudentSkills
                    .Where(s => s.StudentId == studentId).FirstAsync();

                return (null, skills);
            }
            catch (Exception ex)
            {
                return ($"An error occurred while retrieving skills data: {ex.Message}", null);
            }
        }
        #endregion

        #region Update
        public async Task<(string?, bool)> UpdateSkills(int studentId, SkillModel updatedSkills)
        {
            try
            {
                var skill = await _context.StudentSkills.FirstOrDefaultAsync(s => s.StudentId == studentId);

                // If the entry is not null
                if (skill != null)
                {
                    // Update individual skills
                    skill.CSharp = updatedSkills.CSharp;
                    skill.Java = updatedSkills.Java;
                    skill.DotNet = updatedSkills.DotNet;
                    skill.Typescript = updatedSkills.Typescript;
                    skill.Python = updatedSkills.Python;
                    skill.PHP = updatedSkills.PHP;
                    skill.CPlusPlus = updatedSkills.CPlusPlus;
                    skill.C = updatedSkills.C;
                    skill.Bootstrap = updatedSkills.Bootstrap;
                    skill.Blazor = updatedSkills.Blazor;
                    skill.JavaScript = updatedSkills.JavaScript;
                    skill.HTML = updatedSkills.HTML;
                    skill.CSS = updatedSkills.CSS;
                    skill.SQL = updatedSkills.SQL;
                    skill.OfficePack = updatedSkills.OfficePack;
                    skill.CloudComputing = updatedSkills.CloudComputing;
                    skill.VersionControl = updatedSkills.VersionControl;
                    skill.NetWork = updatedSkills.NetWork;
                    skill.ProblemSolving = updatedSkills.ProblemSolving;
                    skill.Communikation = updatedSkills.Communikation;
                    skill.TeamWorking = updatedSkills.TeamWorking;
                    skill.WillingToLearn = updatedSkills.WillingToLearn;

                    _context.Entry(skill).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    return ("Skills updated successfully", true);


                }
                else
                {
                    // Handle the exception and return an error message
                    return ($"An error has ocurred", false);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update skills: {ex.Message}");
            }
        }
        #endregion

        #region Delete skills
        public async Task<(bool, string?)> DeleteSkills(int studentId)
        {
            try
            {
                // Find the entry for the specified student
                var skill = await _context.StudentSkills.FirstOrDefaultAsync(s => s.StudentId == studentId);

                // If the entry is not null, delete it
                if (skill != null)
                {
                    _context.StudentSkills.Remove(skill);
                    await _context.SaveChangesAsync();

                    return (true, "Skills deleted successfully."); //success
                }
                else
                {
                    return (false, "Skills not found for the specified student"); // Return a message when the skills are not found
                }
            }
            catch (Exception ex)
            {
                return (false, $"{ex.Message}");
            }
        }

        #endregion
    }

    public interface ISkillService {

        Task<List<string>> GetSkills(StudentModel data);
        Task<List<string>> GetSkillsById(int id);
        Task<(string?, bool)> CreateSkills(int studentId, SkillModel newSkills);
        Task<(string?, SkillModel?)> GetSkillsByStudentId(int studentId);
        Task<(string?, bool)> UpdateSkills(int studentId, SkillModel updatedSkills);
        Task<(bool, string?)> DeleteSkills(int studentId);




    }
}
