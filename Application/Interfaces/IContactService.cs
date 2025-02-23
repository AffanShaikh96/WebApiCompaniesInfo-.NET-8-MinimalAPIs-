using Domain.Entities;

namespace Application.Interfaces
{
    public interface IContactService
    {
        Task<List<Contact>> GetContactsAsync();
        Task<Contact?> GetContactByIdAsync(int id);
        Task<Contact> CreateContactAsync(Contact contact);
        Task<bool> UpdateContactAsync(Contact contact);
        Task<bool> DeleteContactAsync(int id);
        Task<List<Contact>> GetContactsWithCompanyAndCountry();
        Task<List<Contact>> FilterContacts(int countryId, int companyId);
    }
}
