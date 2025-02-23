using Application.Interfaces;
using Application.Services;
using Application.Validators;
using Domain.Entities;
using FluentValidation;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

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

// Register FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CompanyValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ContactValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CountryValidator>();


// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Add Authorization
builder.Services.AddAuthorization();

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


//Login End Point
app.MapPost("/login", (UserLoginModel model) =>
{
    if (model.Username == "admin" && model.Password == "password") // Mock authentication
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("3621c50d75ad522a466d39ac13d2a90d555ad7779f2ef432f4bf588a4a2eedcb"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, model.Username),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var token = new JwtSecurityToken(
            issuer: "localhost",
            audience: "localhost",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Results.Ok(new { token = tokenString });
    }

    return Results.Unauthorized();
});


// Company Endpoints
app.MapGet("/companies", [Authorize] async (ICompanyService service) =>
{
    try
    {
        return Results.Ok(await service.GetCompaniesAsync());

    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error in getting companies list");
        return Results.Problem("An error occurred while processing your request.");
    }

}).WithTags("Company End Points");

app.MapGet("/companies/{id:int}", async (int id, ICompanyService service) =>
{
    try
    {
        var company = await service.GetCompanyByIdAsync(id);
        return company is not null ? Results.Ok(company) : Results.NotFound();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error in getting company with ID: {CompanyId}", id);
        return Results.Problem("An error occurred while processing your request.");
    }

}).WithTags("Company End Points");

app.MapPost("/companies", async (Company company, IValidator<Company> validator, ICompanyService service) =>
{
    try
    {
        var validationResult = await validator.ValidateAsync(company);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }
        var newCompany = await service.CreateCompanyAsync(company);
        return Results.Created($"/companies/{newCompany.Id}", newCompany);
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error in creating company.");
        return Results.Problem("An error occurred while processing your request.");
    }
}).WithTags("Company End Points");

app.MapPut("/companies/{id:int}", async (int id, Company company, IValidator<Company> validator, ICompanyService service) =>
{

    try
    {
        var validationResult = await validator.ValidateAsync(company);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }
        else if (id != company.Id)
        {
            return Results.BadRequest();
        }

        return await service.UpdateCompanyAsync(company) ? Results.NoContent() : Results.NotFound();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error in updating company with ID: {CompanyId}", id);
        return Results.Problem("An error occurred while processing your request.");
    }
}).WithTags("Company End Points"); ;

app.MapDelete("/companies/{id:int}", async (int id, ICompanyService service) =>
{
    try
    {
        return await service.DeleteCompanyAsync(id) ? Results.NoContent() : Results.NotFound();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error in deleting company with ID: {CompanyId}", id);
        return Results.Problem("An error occurred while processing your request.");
    }
}).WithTags("Company End Points"); ;


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
}).WithTags("Country End Points"); ;

app.MapGet("/countries/{id:int}", async (int id, ICountryService service) =>
{
    try
    {
        var country = await service.GetCountryByIdAsync(id);
        return country is not null ? Results.Ok(country) : Results.NotFound();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error in getting country with ID: {CountryId}", id);
        return Results.Problem("An error occurred while processing your request.");
    }

}).WithTags("Country End Points"); ;

app.MapPost("/countries", async (Country country, IValidator<Country> validator, ICountryService service) =>
{
    try
    {
        var validationResult = await validator.ValidateAsync(country);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }
        var newCountry = await service.CreateCountryAsync(country);
        return Results.Created($"/countries/{newCountry.Id}", newCountry);
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error in creating country.");
        return Results.Problem("An error occurred while processing your request.");
    }

}).WithTags("Country End Points");

app.MapPut("/countries/{id:int}", async (int id, Country country, IValidator<Country> validator, ICountryService service) =>
{
    try
    {
        var validationResult = await validator.ValidateAsync(country);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }
        else if (id != country.Id)
        {
            return Results.BadRequest();
        }
        return await service.UpdateCountryAsync(country) ? Results.NoContent() : Results.NotFound();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error in updating country with ID: {CountryId}", id);
        return Results.Problem("An error occurred while processing your request.");
    }

}).WithTags("Country End Points"); ;

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
}).WithTags("Country End Points"); ;

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
}).WithTags("Country End Points"); ;

//Contacts End points

app.MapGet("/contacts", async (IContactService service) =>
{
    try
    {
        return Results.Ok(await service.GetContactsAsync());
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error in getting contacts list");
        return Results.Problem("An error occurred while processing your request.");
    }

}).WithTags("Contact End Points");

app.MapGet("/contacts/{id:int}", async (int id, IContactService service) =>
{
    try
    {
        var contact = await service.GetContactByIdAsync(id);
        return contact is not null ? Results.Ok(contact) : Results.NotFound();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error in getting contact with ID: {ContactId}", id);
        return Results.Problem("An error occurred while processing your request.");
    }
}).WithTags("Contact End Points"); ;

app.MapPost("/contacts", async (Contact contact, IValidator<Contact> validator, IContactService service) =>
{
    try
    {
        var validationResult = await validator.ValidateAsync(contact);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }
        var newContact = await service.CreateContactAsync(contact);
        return Results.Created($"/contacts/{newContact.Id}", newContact);
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error in creating contact.");
        return Results.Problem("An error occurred while processing your request.");
    }
}).WithTags("Contact End Points"); ;

app.MapPut("/contacts/{id:int}", async (int id, Contact contact, IValidator<Contact> validator, IContactService service) =>
{
    try
    {
        var validationResult = await validator.ValidateAsync(contact);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }
        else if (id != contact.Id) { 
            return Results.BadRequest(); 
        }
        return await service.UpdateContactAsync(contact) ? Results.NoContent() : Results.NotFound();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error in updating contact with ID: {ContactId}", id);
        return Results.Problem("An error occurred while processing your request.");
    }
}).WithTags("Contact End Points"); ;

app.MapDelete("/contacts/{id:int}", async (int id, IContactService service) =>
{
    try
    {
        return await service.DeleteContactAsync(id) ? Results.NoContent() : Results.NotFound();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error in deleting contact with ID: {ContactId}", id);
        return Results.Problem("An error occurred while processing your request.");
    }
}).WithTags("Contact End Points"); ;

app.MapGet("/contacts/contacts-with-company-and-country", async (IContactService service) =>
{
    try
    {
        return Results.Ok(await service.GetContactsWithCompanyAndCountry());
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error in getting contacts with country and company.");
        return Results.Problem("An error occurred while processing your request.");
    }
}).WithTags("Contact End Points"); ;

app.MapGet("/contacts/{countryId:int}/{companyId:int}/filter-contacts", async (int countryId, int companyId, IContactService service) =>
{

    try
    {
        var contacts = await service.FilterContacts(countryId, companyId);
        return contacts.Count > 0 ? Results.Ok(contacts) : Results.NotFound("No contacts found for this country id and company id");
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error in getting contacts with country id: {countryId} and company id: {companyId}.", countryId, companyId);
        return Results.Problem("An error occurred while processing your request.");
    }
}).WithTags("Contact End Points"); ;

// Run Migrations Automatically (For Development Only)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();
public record UserLoginModel(string Username, string Password);
