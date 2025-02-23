using Domain.Entities;

namespace Application.Interfaces
{
    public interface ICountryService
    {
        Task<List<Country>> GetCountriesAsync();
        Task<Country?> GetCountryByIdAsync(int id);
        Task<Country> CreateCountryAsync(Country country);
        Task<bool> UpdateCountryAsync(Country country);
        Task<bool> DeleteCountryAsync(int id);
        Task<Dictionary<string, int>> GetCompanyStatisticsByCountryId(int countryId);
    }
}
