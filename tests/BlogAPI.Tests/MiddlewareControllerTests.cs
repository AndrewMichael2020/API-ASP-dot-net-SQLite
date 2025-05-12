using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BlogAPI.Tests;

public class ErrorHandlingMiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ErrorHandlingMiddlewareTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Returns_InternalServerError_On_Exception()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/throw"); // You need to add a test endpoint that throws
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
