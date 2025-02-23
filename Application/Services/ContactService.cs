using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ContactService(ApplicationDbContext context) : IContactService
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<List<Contact>> GetContactsAsync() =>
             await (from contact in _context.Contacts
                    select new Contact
                    {
                        Id = contact.Id,
                        Name = contact.Name
                    })
            .ToListAsync();


        public async Task<Contact?> GetContactByIdAsync(int id) =>
             await (from contact in _context.Contacts
                    select new Contact
                    {
                        Id = contact.Id,
                        Name = contact.Name
                    })
            .FirstOrDefaultAsync(c => c.Id == id);

        

        public async Task<Contact> CreateContactAsync(Contact contact)
        {
            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();
            return contact;
        }

        public async Task<bool> UpdateContactAsync(Contact contact)
        {
            _context.Contacts.Update(contact);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteContactAsync(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null) return false;
            _context.Contacts.Remove(contact);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Contact>> GetContactsWithCompanyAndCountry() =>
            await (from contact in _context.Contacts
                   select new Contact
                   {
                       Id = contact.Id,
                       Name = contact.Name,
                       CountryId = contact.CountryId,
                       Country = contact.Country,
                       CompanyId = contact.CompanyId,
                       Company = contact.Company
                   })
          .ToListAsync();


        public async Task<List<Contact>> FilterContacts(int countryId, int companyId) =>
            await (from contact in _context.Contacts
                   where contact.CountryId == countryId && contact.CompanyId == companyId
                   select new Contact
                   {
                       Id = contact.Id,
                       Name = contact.Name,
                       CountryId = contact.CountryId,
                       Country = contact.Country,
                       CompanyId = contact.CompanyId,
                       Company = contact.Company
                   })
            .ToListAsync();

    }
}
