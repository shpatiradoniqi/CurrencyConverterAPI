Currency Converter API

A robust and simple ASP.NET Core Web API for retrieving and converting currency exchange rates. This API leverages the Frankfurter API as its primary data source and includes enterprise-grade features such as authentication, caching, and resiliency.
Features

    Real-time Exchange Rates: Retrieve the latest exchange rates for any given base currency.

    Currency Conversion: Convert amounts between different currencies easily.

    Historical Data: Access historical exchange rates for a specified date range.

    Secure Authentication: Implements JWT Bearer authentication with role-based authorization (User, Admin).

    Performance & Reliability: Caching and resiliency policies using Polly to handle transient faults (retry) and improve response times (circuit breaker).

    API Documentation: Comprehensive and interactive API documentation with Swagger/OpenAPI.

Technologies Used

    ASP.NET Core: A cross-platform framework for building modern, cloud-based, internet-connected applications.

    Frankfurter API: A free and open-source API for currency exchange rates.

    JWT Bearer Authentication: A standard for token-based authentication.

    Polly: A .NET resilience and transient-fault-handling library.

    Swagger/OpenAPI: A framework for designing, building, documenting, and consuming RESTful web services.

Getting Started
Prerequisites

    .NET SDK 8.0 or later.

    A code editor like Visual Studio or Visual Studio Code.

Clone the Repository

To get a local copy up and running, follow these simple steps.

git clone https://github.com/shpatiradoniqi/CurrencyConverterAPI.git
cd CurrencyConverterAPI

Configuration

Update the appsettings.json file with your specific JWT and Frankfurter API settings. For production environments, it is highly recommended to use User Secrets to store sensitive information like the Jwt:Key.

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

Run the Application

You can run the project from Visual Studio or using the .NET CLI.

dotnet run

The API will be available at http://localhost:5000 (or another port as configured). You can access the Swagger documentation at http://localhost:5000/swagger.
API Endpoints

All endpoints are prefixed with /api/v1/currency and require a valid JWT Bearer token in the Authorization header.
1. Get Latest Exchange Rates

Retrieves the latest exchange rates for a given base currency.

    URL: /api/v1/currency/latest/{baseCurrency}

    Method: GET

    Authorization: User, Admin

    Route Parameters:

        baseCurrency: The base currency code (e.g., USD).

    Example Request:

curl -X GET "http://localhost:5000/api/v1/currency/latest/USD" \
-H "Authorization: Bearer YOUR_JWT_TOKEN"

2. Convert Currency

Converts a specified amount from one currency to another.

    URL: /api/v1/currency/convert

    Method: POST

    Authorization: User, Admin

    Request Body:
    A JSON object representing the ConvertCurrencyRequest with the following properties:

        from: The source currency code (e.g., USD).

        to: The target currency code (e.g., EUR).

        amount: The amount to convert.

    Example Request:

curl -X POST "http://localhost:5000/api/v1/currency/convert" \
-H "Authorization: Bearer YOUR_JWT_TOKEN" \
-H "Content-Type: application/json" \
-d '{"from":"USD", "to":"EUR", "amount":100}'

3. Get Historical Exchange Rates

Retrieves historical exchange rates for a date range.

    URL: /api/v1/currency/history

    Method: GET

    Authorization: Admin

    Query Parameters:

        baseCurrency: The base currency code (e.g., USD).

        startDate: The start date in YYYY-MM-DD format.

        endDate: The end date in YYYY-MM-DD format.

    Example Request:

curl -X GET "http://localhost:5000/api/v1/currency/history?baseCurrency=USD&startDate=2023-01-01&endDate=2023-01-05" \
-H "Authorization: Bearer YOUR_ADMIN_TOKEN"

Authentication

This API is secured with JWT Bearer tokens. The [Authorize] attribute is used on controllers and endpoints to enforce authentication and role-based authorization.

The application does not include a user registration or login endpoint. A valid JWT must be generated externally and provided in the Authorization header for protected endpoints.
Example Token Payload

Tokens are expected to contain a role claim, which is used for authorization.

{
  "sub": "testuser",
  "role": "User",
  "iss": "MyIssuer",
  "aud": "MyAudience",
  "exp": 1893456000
}

Testing

The project includes unit and integration tests to ensure code quality and functionality. The test projects are located in the Main folder.

To run all tests from the command line, use the following command from the project's root directory:

dotnet test

Contributing

We welcome contributions! Please feel free to open issues or submit pull requests.
License

This project is licensed under the MIT License. See the LICENSE file for details.