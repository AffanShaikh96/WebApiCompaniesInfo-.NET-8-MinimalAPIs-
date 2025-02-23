using Application.Services;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Services
{
    public class CountryServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly CountryService _service;
        private readonly Mock<ILogger<CountryService>> _loggerMock = new();

        public CountryServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _service = new CountryService(_context, _loggerMock.Object);
        }

        [Fact]
        public async Task GetCompanyStatisticsByCountryId_ShouldReturnCorrectCounts()
        {
            // Arrange
            var country = new Country { Id = 1, Name = "USA" };
            var company1 = new Company { Id = 1, Name = "Tech Corp" };
            var company2 = new Company { Id = 2, Name = "Finance Ltd" };

            var contacts = new List<Contact>
            {
                new Contact { Id = 1, Name = "Alice", CompanyId = 1, CountryId = 1 },
                new Contact { Id = 2, Name = "Bob", CompanyId = 1, CountryId = 1 },
                new Contact { Id = 3, Name = "Charlie", CompanyId = 2, CountryId = 1 }
            };

            _context.Countries.Add(country);
            _context.Companies.AddRange(company1, company2);
            _context.Contacts.AddRange(contacts);
            _context.SaveChanges();

            // Act
            var result = await _service.GetCompanyStatisticsByCountryId(1);

            // Assert
            result.Should().ContainKey("Tech Corp").WhoseValue.Should().Be(2);
            result.Should().ContainKey("Finance Ltd").WhoseValue.Should().Be(1);
        }
    }
}
