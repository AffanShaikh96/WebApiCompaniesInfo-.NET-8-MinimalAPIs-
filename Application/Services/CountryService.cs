using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CountryService(ApplicationDbContext context, ILogger<CountryService> logger) : ICountryService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<CountryService> _logger = logger;


        public async Task<List<Country>> GetCountriesAsync()
        {
            try
            {
                return await _context.Countries.ToListAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all countries.");
                throw;
            }

        }

        public async Task<Country?> GetCountryByIdAsync(int id)
        {
            try
            {
                return await _context.Countries.FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching country with ID {Id}", id);
                throw;
            }
        }

        public async Task<Country> CreateCountryAsync(Country country)
        {
            try
            {
                _context.Countries.Add(country);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Country {CountryName} created successfully.", country.Name);
                return country;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating country {CountryName}", country.Name);
                throw;
            }
        }

        public async Task<bool> UpdateCountryAsync(Country country)
        {
            try
            {
                _context.Countries.Update(country);
                bool updated = await _context.SaveChangesAsync() > 0;
                if (updated) _logger.LogInformation("Country {CountryName} updated successfully.", country.Name);
                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating country {CountryName}", country.Name);
                throw;
            }
        }

        public async Task<bool> DeleteCountryAsync(int id)
        {
            try
            {
                var country = await _context.Countries.FindAsync(id);
                if (country == null)
                {
                    _logger.LogWarning("Country with ID {Id} not found.", id);
                    return false;
                }

                _context.Countries.Remove(country);
                bool deleted = await _context.SaveChangesAsync() > 0;
                if (deleted) _logger.LogInformation("Country {CountryName} deleted successfully.", country.Name);
                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting country with ID {Id}", id);
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetCompanyStatisticsByCountryId(int countryId)
        {
            try
            {
                var companyStats = await _context.Contacts
                    .Where(c => c.CountryId == countryId)
                    .GroupBy(c => c.Company.Name)
                    .Select(group => new { CompanyName = group.Key, ContactCount = group.Count() })
                    .ToDictionaryAsync(x => x.CompanyName, x => x.ContactCount);

                if (companyStats.Count == 0)
                {
                    _logger.LogWarning("No companies found for country with ID {CountryId}", countryId);
                }
                else
                {
                    _logger.LogInformation("Fetched company statistics for country ID {CountryId}", countryId);
                }

                return companyStats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching company statistics for country with ID {CountryId}", countryId);
                throw;
            }
        }
    }
}
