using Application.Services;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

public class CompanyServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly CompanyService _service;
    private readonly Mock<ILogger<CompanyService>> _loggerMock = new();

    public CompanyServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new CompanyService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetCompaniesAsync_ShouldReturnCompanies()
    {
        // Arrange
        _context.Companies.Add(new Company { Id = 1, Name = "Tech Corp" });
        _context.SaveChanges();

        // Act
        var result = await _service.GetCompaniesAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Tech Corp");
    }

    [Fact]
    public async Task CreateCompanyAsync_ShouldAddCompany()
    {
        // Arrange
        var company = new Company { Id = 2, Name = "Finance Ltd" };

        // Act
        var result = await _service.CreateCompanyAsync(company);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(2);
        _context.Companies.Should().HaveCount(1);
    }
}