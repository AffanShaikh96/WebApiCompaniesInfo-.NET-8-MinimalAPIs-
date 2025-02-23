# **Minimal API with Clean Architecture and Authentication**

## **📌 Overview**
This project is a **.NET 8 Minimal API** following **Clean Architecture** and **Domain-Driven Design (DDD)** principles. It includes:
- **Entity Framework Core** with SQL Server
- **FluentValidation** for request validation
- **Authentication & Authorization (JWT-based)**
- **Logging & Error Handling**
- **Unit Testing with xUnit & Moq**

## **📂 Architecture Overview**
The project follows **Clean Architecture** with well-separated layers:

```
YourSolution/
│── API/                    <-- Minimal API layer (Entry Point)
│── Application/            <-- Business Logic & Services
│   ├── Validators/         <-- FluentValidation Validators
│   ├── Services/           <-- Business Service Implementations
│   ├── Interfaces/         <-- Service Interfaces
│── Domain/                 <-- Entities & Domain Logic
│── Infrastructure/         <-- Database & External Dependencies
│── Tests/                  <-- Unit Tests
```

## **📌 Technologies Used**
- **.NET 8** (Minimal API)
- **Entity Framework Core** (SQL Server)
- **FluentValidation** (Data Validation)
- **JWT Authentication** (Security)
- **xUnit & Moq** (Unit Testing)
- **Microsoft Built-In Logging** (Logging)

## **🔹 Database Models & Relationships**
- **Company** (1:M) **Contact** → One company has multiple contacts.
- **Country** (1:M) **Contact** → One country has multiple contacts.

## **📌 API Endpoints**

### **1️⃣ Company Endpoints**
| Method | Endpoint              | Description |
|--------|----------------------|-------------|
| GET    | `/companies`         | Get all companies (🔒 **Protected**) |
| GET    | `/companies/{id}`    | Get a company by ID |
| POST   | `/companies`         | Create a new company |
| PUT    | `/companies/{id}`    | Update a company |
| DELETE | `/companies/{id}`    | Delete a company |

### **2️⃣ Contact Endpoints**
| Method | Endpoint                                           | Description                                                 |
|--------|----------------------------------------------------|-------------------------------------------------------------|
| GET    | `/contacts`                                        | Get all contacts                                            |
| GET    | `/contacts/{id}`                                   | Get a contact by ID                                         |
| POST   | `/contacts`                                        | Create a new contact                                        |
| PUT    | `/contacts/{id}`                                   | Update a contact                                            |
| DELETE | `/contacts/{id}`                                   | Delete a contact                                            |
| GET    | `/contacts/{countryId}{companyId}/filter-contacts` | Get all contacts by fitering with company id and country id |
| GET    | `/contacts/contacts-with-company-and-country`      |  Get all contacts with company and country information      |

### **3️⃣ Country Endpoints**
| Method | Endpoint                             | Description                          |
|--------|--------------------------------------|--------------------------------------|
| GET    | `/countries`                         | Get all countries                    |
| GET    | `/countries/{id}`                    | Get a country by ID                  |
| POST   | `/countries`                         | Create a new country                 |
| PUT    | `/countries/{id}`                    | Update a country                     |
| DELETE | `/countries/{id}`                    | Delete a country                     |
| GET    | `/countries/{id}/company-statistics` | Get company statistics by country ID |

### **4️⃣ Authentication & Security Endpoints**
| Method | Endpoint   | Description |
|--------|-----------|-------------|
| POST   | `/login`  | Authenticate user and return JWT Token |


## **📌 🔒 Authentication & Authorization**
- **JWT Authentication** is implemented to secure endpoints.
- The following endpoint is **protected** and requires a valid **JWT Token**:
  ```csharp
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
  ```
- To access protected endpoints:
  1. **Get JWT Token**: Call `/login` with `{ "username": "admin", "password": "password" }`.
  2. **Use Token**: Send `Authorization: Bearer {TOKEN}` in API requests.

## **📌 Running the Project**
### **1️⃣ Setup & Run Migration**
```bash
# Run Migrations in Visual Studio package manager console by selecting Infrastruce project from Default Project dropdown.
Add-Migration InitialCreate
Update-database
```

## **📌 Unit Testing**
Unit tests are written using **xUnit & Moq** inside the `Tests` project.


✅ **Test Coverage Includes:**
- **Service Layer Tests** (e.g., `CompanyServiceTests.cs`)

---
🚀 **Enjoy building scalable APIs with .NET 8 and Clean Architecture!**

