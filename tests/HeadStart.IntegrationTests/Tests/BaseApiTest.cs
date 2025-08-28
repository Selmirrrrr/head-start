using HeadStart.IntegrationTests.Data;
using Microsoft.Kiota.Abstractions;
using Shouldly;

namespace HeadStart.IntegrationTests.Tests;

/// <summary>
/// Base class for API integration tests providing common functionality
/// and ensuring test independence.
/// </summary>
public abstract class BaseApiTest(ApiTestDataClass apiTestDataClass)
{
    /// <summary>
    /// Asserts that an API call throws an unauthorized exception
    /// </summary>
    protected static async Task AssertUnauthorizedAsync(Func<Task> apiCall)
    {
        var exception = await Should.ThrowAsync<ApiException>(apiCall);
        exception.ResponseStatusCode.ShouldBe(401);
    }

    /// <summary>
    /// Asserts that an API call throws a forbidden exception
    /// </summary>
    protected static async Task AssertForbiddenAsync(Func<Task> apiCall)
    {
        var exception = await Should.ThrowAsync<ApiException>(apiCall);
        exception.ResponseStatusCode.ShouldBe(403);
    }

    /// <summary>
    /// Asserts that an API call throws a not found exception
    /// </summary>
    protected static async Task AssertNotFoundAsync(Func<Task> apiCall)
    {
        var exception = await Should.ThrowAsync<ApiException>(apiCall);
        exception.ResponseStatusCode.ShouldBe(404);
    }

    /// <summary>
    /// Asserts that an API call throws a bad request exception
    /// </summary>
    protected static async Task AssertBadRequestAsync(Func<Task> apiCall)
    {
        var exception = await Should.ThrowAsync<ApiException>(apiCall);
        exception.ResponseStatusCode.ShouldBe(400);
    }

    /// <summary>
    /// Measures the execution time of an async operation
    /// </summary>
    protected static async Task<(T Result, TimeSpan Elapsed)> MeasureExecutionTimeAsync<T>(Func<Task<T>> operation)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await operation();
        stopwatch.Stop();
        return (result, stopwatch.Elapsed);
    }

    /// <summary>
    /// Creates a unique test identifier for data isolation
    /// </summary>
    protected static string GenerateTestId(string prefix = "test")
    {
        return $"{prefix}_{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Retry an operation with exponential backoff
    /// Useful for eventual consistency scenarios
    /// </summary>
    protected static async Task<T> RetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        int initialDelayMs = 100)
    {
        var delay = initialDelayMs;
        Exception? lastException = null;

        for (var i = 0; i <= maxRetries; i++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (i < maxRetries)
            {
                lastException = ex;
                await Task.Delay(delay);
                delay *= 2; // Exponential backoff
            }
        }

        throw lastException ?? new InvalidOperationException("Retry failed");
    }

    /// <summary>
    /// Asserts that two collections are equivalent (same items, any order)
    /// </summary>
    protected static void AssertCollectionsEquivalent<T>(IEnumerable<T> expected, IEnumerable<T> actual)
    {
        var expectedList = expected.ToList();
        var actualList = actual.ToList();

        expectedList.Count.ShouldBe(actualList.Count);
        expectedList.ShouldBeSubsetOf(actualList);
        actualList.ShouldBeSubsetOf(expectedList);
    }
}
