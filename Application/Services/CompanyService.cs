using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class CompanyService(ApplicationDbContext context, ILogger<CompanyService> logger) : ICompanyService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<CompanyService> _logger = logger;

        public async Task<List<Company>> GetCompaniesAsync()
        {
            try
            {
                return await _context.Companies.ToListAsync();
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error fetching all companies.");
                throw;
            }

        }

        public async Task<Company?> GetCompanyByIdAsync(int id)
        {
            try
            {
                return await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);   
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching company with ID {Id}", id);
                throw;
            }
        }
        public async Task<Company> CreateCompanyAsync(Company company)
        {
            try
            {
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Company {CompanyName} created successfully.", company.Name);
                return company;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating company {CompanyName}", company.Name);
                throw;
            }
        }

        public async Task<bool> UpdateCompanyAsync(Company company)
        {
            try
            {
                _context.Companies.Update(company);
                bool updated = await _context.SaveChangesAsync() > 0;
                if (updated) _logger.LogInformation("Company {CompanyName} updated successfully.", company.Name);
                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company {CompanyName}", company.Name);
                throw;
            }
        }

        public async Task<bool> DeleteCompanyAsync(int id)
        {
            try
            {
                var company = await _context.Companies.FindAsync(id);
                if (company == null)
                {
                    _logger.LogWarning("Company with ID {Id} not found.", id);
                    return false;
                }

                _context.Companies.Remove(company);
                bool deleted = await _context.SaveChangesAsync() > 0;
                if (deleted) _logger.LogInformation("Company {CompanyName} deleted successfully.", company.Name);
                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting company with ID {Id}", id);
                throw;
            }
        }
    }
}
