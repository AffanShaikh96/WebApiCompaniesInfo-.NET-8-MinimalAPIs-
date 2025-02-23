using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Company> Companies { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Country> Countries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>()
            .HasMany(c => c.Contacts)
            .WithOne(c => c.Company)
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Country>()
            .HasMany(c => c.Contacts)
            .WithOne(c => c.Country)
            .HasForeignKey(c => c.CountryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed Countries
        modelBuilder.Entity<Country>().HasData(
            new Country { Id = 1, Name = "USA" },
            new Country { Id = 2, Name = "Canada" },
            new Country { Id = 3, Name = "Germany" }
        );

        // Seed Companies
        modelBuilder.Entity<Company>().HasData(
            new Company { Id = 1, Name = "Apple" },
            new Company { Id = 2, Name = "Google" }
        );

        // Seed Contacts
        modelBuilder.Entity<Contact>().HasData(
            new Contact { Id = 1, Name = "John Doe", CompanyId = 1, CountryId = 1 },
            new Contact { Id = 2, Name = "Alice Smith", CompanyId = 2, CountryId = 2 },
            new Contact { Id = 3, Name = "Bob Johnson", CompanyId = 1, CountryId = 1 }
        );
    }
}