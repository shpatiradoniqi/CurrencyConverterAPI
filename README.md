# Currency Converter API

A simple ASP.NET Core Web API for retrieving and converting currency exchange rates using the [Frankfurter API](https://www.frankfurter.app/) as the data source.

---

## Features

- Retrieve latest exchange rates for a given base currency.
- Convert amounts between currencies.
- Retrieve historical exchange rates for a date range.
- JWT Bearer authentication with role-based authorization (`User`, `Admin`).
- Caching and resiliency using Polly (retry and circuit breaker policies).
- API documentation with Swagger/OpenAPI.

---

## Configuration

### appsettings.json example

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "Issuer": "MyIssuer",
    "Audience": "MyAudience",
    "Key": "mysupersecretkey123!"
  },
  "Frankfurter": {
    "BaseUrl": "https://api.frankfurter.app/"
  }
}
Running the Project
Clone the repository:
git clone https://github.com/yourusername/your-repo.git

Update the JWT and Frankfurter API settings in appsettings.json or your user secrets.

Run the project via Visual Studio or using CLI:

dotnet run

Authentication

JWT Bearer tokens protect the API.

Roles are read from the role claim in the JWT token.

Example token payload:
{
  "sub": "testuser",
  "role": "User",
  "iss": "MyIssuer",
  "aud": "MyAudience",
  "exp": 1893456000
}

Testing
Unit and integration tests are included.
To view and run tests, see the Main folder containing: