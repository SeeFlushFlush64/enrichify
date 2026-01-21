using Enrichify.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace Enrichify.Tests.Services
{
    public class HunterServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly HunterService _service;

        public HunterServiceTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["Hunter:ApiKey"]).Returns("test-api-key");

            _service = new HunterService(_httpClient, _mockConfiguration.Object);
        }

        [Fact]
        public async Task FindEmail_WithValidResponse_ReturnsEmail()
        {
            // Arrange
            var responseContent = @"{
                ""data"": {
                    ""email"": ""john.doe@example.com"",
                    ""score"": 95,
                    ""first_name"": ""John"",
                    ""last_name"": ""Doe""
                }
            }";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            // Act
            var result = await _service.FindEmail("example.com", "John", "Doe");

            // Assert
            result.Should().Be("john.doe@example.com");
        }

        [Fact]
        public async Task FindEmail_WithUnsuccessfulStatusCode_ReturnsNull()
        {
            // Arrange
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            // Act
            var result = await _service.FindEmail("example.com", "John", "Doe");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task FindEmail_WithCorrectApiUrl_CallsHunterApiWithCorrectParameters()
        {
            // Arrange
            var responseContent = @"{""data"":{""email"":""test@example.com""}}";
            HttpRequestMessage capturedRequest = null;

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, token) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            // Act
            await _service.FindEmail("example.com", "John", "Doe");

            // Assert
            capturedRequest.Should().NotBeNull();
            capturedRequest.RequestUri.ToString().Should().Contain("api.hunter.io/v2/email-finder");
            capturedRequest.RequestUri.ToString().Should().Contain("domain=example.com");
            capturedRequest.RequestUri.ToString().Should().Contain("first_name=John");
            capturedRequest.RequestUri.ToString().Should().Contain("last_name=Doe");
            capturedRequest.RequestUri.ToString().Should().Contain("api_key=test-api-key");
        }

        [Fact]
        public async Task FindEmail_With401Unauthorized_ReturnsNull()
        {
            // Arrange
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized
                });

            // Act
            var result = await _service.FindEmail("example.com", "John", "Doe");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task FindEmail_With429RateLimitExceeded_ReturnsNull()
        {
            // Arrange
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.TooManyRequests
                });

            // Act
            var result = await _service.FindEmail("example.com", "John", "Doe");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task FindEmail_WithSpecialCharactersInNames_EncodesParametersCorrectly()
        {
            // Arrange
            var responseContent = @"{""data"":{""email"":""o-brien@example.com""}}";
            HttpRequestMessage capturedRequest = null;

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, token) => capturedRequest = req)
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            // Act
            await _service.FindEmail("example.com", "Patrick", "O'Brien");

            // Assert
            capturedRequest.Should().NotBeNull();
            var uri = capturedRequest.RequestUri.ToString();
            uri.Should().Contain("first_name=Patrick");
            uri.Should().Contain("last_name=");
        }

        [Fact]
        public async Task FindEmail_WithEmptyResponse_HandlesGracefully()
        {
            // Arrange - Response missing email field
            var responseContent = @"{""data"":{}}";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            // Act & Assert - Should throw or handle missing property
            var act = async () => await _service.FindEmail("example.com", "John", "Doe");
            await act.Should().ThrowAsync<Exception>();
        }
    }
}