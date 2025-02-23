using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configure Database with SQL Server (Change connection string as needed)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IContactService, ContactService>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Global Exception Handling Middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionHandlerPathFeature?.Error != null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(exceptionHandlerPathFeature.Error, "Unhandled exception occurred.");

            await context.Response.WriteAsJsonAsync(new
            {
                ErrorMessage = "An unexpected error occurred. Please try again later."
            });
        }
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


// Company Endpoints
app.MapGet("/companies", async (ICompanyService service) => Results.Ok(await service.GetCompaniesAsync()));

app.MapGet("/companies/{id:int}", async (int id, ICompanyService service) =>
{
    var company = await service.GetCompanyByIdAsync(id);
    return company is not null ? Results.Ok(company) : Results.NotFound();
});

app.MapPost("/companies", async (Company company, ICompanyService service) =>
{
    var newCompany = await service.CreateCompanyAsync(company);
    return Results.Created($"/companies/{newCompany.Id}", newCompany);
});

app.MapPut("/companies/{id:int}", async (int id, Company company, ICompanyService service) =>
{
    if (id != company.Id) return Results.BadRequest();
    return await service.UpdateCompanyAsync(company) ? Results.NoContent() : Results.NotFound();
});

app.MapDelete("/companies/{id:int}", async (int id, ICompanyService service) =>
{
    return await service.DeleteCompanyAsync(id) ? Results.NoContent() : Results.NotFound();
});


//Countries End points
app.MapGet("/countries", async (ICountryService service) =>
{
    try
    {
        return Results.Ok(await service.GetCountriesAsync());
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error in getting countries list");
        return Results.Problem("An error occurred while processing your request.");
    }
});

app.MapGet("/countries/{id:int}", async (int id, ICountryService service) =>
{
    try
    {
        var country = await service.GetCountryByIdAsync(id);
        return country is not null ? Results.Ok(country) : Results.NotFound();
    }
    catch(Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error in getting country with ID: {CountryId}", id);
        return Results.Problem("An error occurred while processing your request.");
    }

});

app.MapPost("/countries", async (Country country, ICountryService service) =>
{
    try
    {
        var newCountry = await service.CreateCountryAsync(country);
        return Results.Created($"/countries/{newCountry.Id}", newCountry);
    }
    catch(Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error in creating country.");
        return Results.Problem("An error occurred while processing your request.");
    }

});

app.MapPut("/countries/{id:int}", async (int id, Country country, ICountryService service) =>
{
    try
    {
        if (id != country.Id) return Results.BadRequest();
        return await service.UpdateCountryAsync(country) ? Results.NoContent() : Results.NotFound();
    }
    catch(Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error in updating country with ID: {CountryId}", id);
        return Results.Problem("An error occurred while processing your request.");
    }

});

app.MapDelete("/countries/{id:int}", async (int id, ICountryService service) =>
{
    app.MapDelete("/countries/{id:int}", async (int id, ICountryService service) =>
    {
        try
        {
            return await service.DeleteCountryAsync(id) ? Results.NoContent() : Results.NotFound();
        }
        catch (Exception ex)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Error in deleting country with ID: {CountryId}", id);
            return Results.Problem("An error occurred while processing your request.");
        }
    });
});

app.MapGet("/countries/{countryId:int}/company-statistics", async (int countryId, ICountryService service) =>
{
    try
    {
        var stats = await service.GetCompanyStatisticsByCountryId(countryId);
        return stats.Count > 0 ? Results.Ok(stats) : Results.NotFound("No companies found for this country.");
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error retrieving company statistics for country ID {CountryId}", countryId);
        return Results.Problem("An error occurred while processing your request.");
    }
});

//Contacts End points

app.MapGet("/contacts", async (IContactService service) => Results.Ok(await service.GetContactsAsync()));

app.MapGet("/contacts/{id:int}", async (int id, IContactService service) =>
{
    var contact = await service.GetContactByIdAsync(id);
    return contact is not null ? Results.Ok(contact) : Results.NotFound();
});

app.MapPost("/contacts", async (Contact contact, IContactService service) =>
{
    var newContact = await service.CreateContactAsync(contact);
    return Results.Created($"/contacts/{newContact.Id}", newContact);
});

app.MapPut("/contacts/{id:int}", async (int id, Contact contact, IContactService service) =>
{
    if (id != contact.Id) return Results.BadRequest();
    return await service.UpdateContactAsync(contact) ? Results.NoContent() : Results.NotFound();
});

app.MapDelete("/contacts/{id:int}", async (int id, IContactService service) =>
{
    return await service.DeleteContactAsync(id) ? Results.NoContent() : Results.NotFound();
});

app.MapGet("/contacts/contacts-with-company-and-country", async (IContactService service) => 
Results.Ok(await service.GetContactsWithCompanyAndCountry()));

app.MapGet("/contacts/{countryId:int}/{companyId:int}/filter-contacts", async (int countryId, int companyId, IContactService service) =>
{
    var contacts = await service.FilterContacts(countryId, companyId);
    return contacts.Any() ? Results.Ok(contacts) : Results.NotFound("No contacts found for this country and company");
});

// Run Migrations Automatically (For Development Only)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();

