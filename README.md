Unit Tests for CurrencyConverterAPI

This document provides an overview of the unit tests for the CurrencyConverterAPI project. The tests are designed to ensure the reliability, correctness, and resilience of the application's core logic, including its API controllers and external service integrations.

The test suite is built using the xUnit testing framework and the Moq library for creating mock objects, which allows for isolated testing of individual components.
1. CurrencyControllerTests

This test class focuses on validating the behavior of the CurrencyController, which handles the API endpoints. The tests ensure that the controller responds correctly to different types of requests and properly handles both successful and unsuccessful scenarios.
How it Works

    It uses Moq to create mock implementations of IExchangeRateService and ILogger.

    By setting up the mock service's methods with specific return values or exceptions (_mockExchangeRateService.Setup(...)), the tests can simulate various outcomes without relying on the actual service implementation or making external API calls.

    The tests assert on the type of IActionResult returned (e.g., OkObjectResult, BadRequestObjectResult) and the content of the response to verify the controller's behavior.

Key Tests

    GetLatest_ShouldReturnOk_WithValidData:

        Purpose: Verifies that the GetLatest endpoint returns an Ok status code and the correct data when the service successfully retrieves exchange rates.

        Methodology: Mocks the service to return a valid ExchangeRateDto and asserts that the controller's response is an OkObjectResult containing that same DTO.

    Convert_ShouldReturnOk_WithValidRequest:

        Purpose: Checks that the Convert endpoint returns an Ok status code with the converted amount when a valid conversion request is made.

        Methodology: Mocks the service to return a converted amount and asserts that the controller's response is an OkObjectResult with the expected value.

    Convert_ShouldReturnBadRequest_WhenArgumentExceptionIsThrown:

        Purpose: Tests the controller's error-handling mechanism. It ensures that if the service throws an ArgumentException (e.g., for an invalid currency), the controller correctly returns a BadRequest status code with the exception's message.

        Methodology: Mocks the service to throw an exception and asserts that the controller's response is a BadRequestObjectResult with the correct error message.

    GetHistory_ShouldReturnOk_WithValidData:

        Purpose: Verifies that the GetHistory endpoint returns an Ok status code and a collection of historical rates when provided with a valid date range.

        Methodology: Mocks the service to return a list of ExchangeRateDtos and asserts that the response is an OkObjectResult containing a non-empty collection.

2. FrankfurterExchangeRateServiceTests

This test class focuses on the FrankfurterExchangeRateService, which is the primary component for interacting with the external Frankfurter API. The tests are crucial for verifying not only the data fetching but also the implementation of caching and resilience policies.
How it Works

    It uses Moq to mock several key dependencies: IMemoryCache, IConfiguration, and the HttpMessageHandler of the HttpClient.

    The HttpMessageHandler mock is essential for simulating different HTTP responses (e.g., success, failure) without making actual network requests. This makes the tests fast and reliable.

    By mocking the cache and configuration, the tests can verify that the service's logic for fetching, storing, and retrieving data is working as expected.

Key Tests

    Constructor_ShouldThrowArgumentNullException_WhenBaseUrlIsMissing:

        Purpose: Validates the service's configuration. It ensures that the service throws an exception if the required Frankfurter:BaseUrl setting is missing from the configuration, preventing the application from starting in an invalid state.

    GetLatestRatesAsync_ShouldReturnCachedResult_WhenDataIsInCache:

        Purpose: Tests the caching functionality. It verifies that when data for a specific currency is already in the cache, the service retrieves it from the cache and does not make a new HTTP request.

    GetLatestRatesAsync_ShouldRetry_OnHttpRequestException:

        Purpose: Verifies the retry policy implemented with Polly. The test simulates a transient network failure by making the HttpClient throw an exception and then asserts that the service attempts to retry the request a specific number of times (as defined by the policy) before ultimately failing.

    GetLatestRatesAsync_ShouldBreakCircuit_AfterConsecutiveFailures:

        Purpose: Verifies the circuit breaker policy. This test simulates a series of consecutive failures. After the configured number of failures, the circuit breaker should "trip," and subsequent calls to the service should immediately fail with a BrokenCircuitException without even attempting an HTTP request. This prevents a failing service from being called repeatedly.

    GetLatestRatesAsync_ShouldFetchFromApi_AndSetCacheOnSuccess:

        Purpose: Tests the primary success path. It simulates a cache miss, an API call that returns a successful response, and then asserts that the service correctly parses the data, returns it, and also stores it in the cache for future requests.

How to Run the Tests

To run these tests from the command line, navigate to the root directory of the project and execute the following command:

dotnet test

This will discover and run all tests in the project, providing a detailed report of the results.
