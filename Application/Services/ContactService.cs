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
    public class ContactService(ApplicationDbContext context, ILogger<ContactService> logger) : IContactService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<ContactService> _logger = logger;

        public async Task<List<Contact>> GetContactsAsync()
        {
            try
            {
                return await _context.Contacts.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all contacts.");
                throw;
            }
        }


        public async Task<Contact?> GetContactByIdAsync(int id)
        {
            try
            {
                return await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching contact with ID {Id}", id);
                throw;
            }
        }



        public async Task<Contact> CreateContactAsync(Contact contact)
        {
            try
            {
                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Contact {ContactName} created successfully.", contact.Name);
                return contact;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contact {ContactName}", contact.Name);
                throw;
            }
        }

        public async Task<bool> UpdateContactAsync(Contact contact)
        {
            try
            {
                _context.Contacts.Update(contact);
                bool updated = await _context.SaveChangesAsync() > 0;
                if (updated) _logger.LogInformation("Contact {ContactName} updated successfully.", contact.Name);
                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contact {ContactName}", contact.Name);
                throw;
            }
        }

        public async Task<bool> DeleteContactAsync(int id)
        {
            try
            {
                var contact = await _context.Contacts.FindAsync(id);
                if (contact == null)
                {
                    _logger.LogWarning("Contact with ID {Id} not found.", id);
                    return false;
                }

                _context.Contacts.Remove(contact);
                bool deleted = await _context.SaveChangesAsync() > 0;
                if (deleted) _logger.LogInformation("Contact {ContactName} deleted successfully.", contact.Name);
                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contact with ID {Id}", id);
                throw;
            }
        }

        public async Task<List<Contact>> GetContactsWithCompanyAndCountry()
        {
            try
            {
                return await (from contact in _context.Contacts
                              select new Contact
                              {
                                  Id = contact.Id,
                                  Name = contact.Name,
                                  CountryId = contact.CountryId,
                                  Country = contact.Country,
                                  CompanyId = contact.CompanyId,
                                  Company = contact.Company
                              }).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all contacts with company and country.");
                throw;
            }

        }



        public async Task<List<Contact>> FilterContacts(int countryId, int companyId)
        {
            try
            {
                return await (from contact in _context.Contacts
                              where contact.CountryId == countryId && contact.CompanyId == companyId
                              select new Contact
                              {
                                  Id = contact.Id,
                                  Name = contact.Name,
                                  CountryId = contact.CountryId,
                                  Country = contact.Country,
                                  CompanyId = contact.CompanyId,
                                  Company = contact.Company
                              }).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering contacts for country ID {CountryId} and company ID {CompanyId}", countryId, companyId);
                throw;
            }
        }
    }
}
