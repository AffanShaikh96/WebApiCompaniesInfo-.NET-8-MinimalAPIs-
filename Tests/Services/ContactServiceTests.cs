using Application.Services;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Services
{
    public class ContactServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ContactService _service;
        private readonly Mock<ILogger<ContactService>> _loggerMock = new();

        public ContactServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _context = new ApplicationDbContext(options);
            _service = new ContactService(_context, _loggerMock.Object);
        }

        [Fact]
        public async Task GetContactsAsync_ShouldReturnContacts()
        {
            // Arrange
            _context.Contacts.Add(new Contact { Id = 1, Name = "John Doe" });
            _context.SaveChanges();

            // Act
            var result = await _service.GetContactsAsync();

            // Assert
            result.Should().HaveCount(1);
            result[0].Name.Should().Be("John Doe");
        }

        [Fact]
        public async Task CreateContactAsync_ShouldAddContact()
        {
            // Arrange
            var contact = new Contact { Id = 2, Name = "Jane Doe" };

            // Act
            var result = await _service.CreateContactAsync(contact);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(2);
            _context.Contacts.Should().HaveCount(1);
        }

        [Fact]
        public async Task DeleteContactAsync_ShouldRemoveContact()
        {
            // Arrange
            var contact = new Contact { Id = 3, Name = "Mark Smith" };
            _context.Contacts.Add(contact);
            _context.SaveChanges();

            // Act
            var result = await _service.DeleteContactAsync(3);

            // Assert
            result.Should().BeTrue();
            _context.Contacts.Should().BeEmpty();
        }
    }
}
