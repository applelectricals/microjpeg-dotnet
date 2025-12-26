using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using Newtonsoft.Json;

namespace MicroJpeg.Tests
{
    public class MicroJpegClientTests : IDisposable
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly MicroJpegClient _client;

        public MicroJpegClientTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.microjpeg.com/v1/")
            };
            _client = new MicroJpegClient("test-api-key", _httpClient);
        }

        [Fact]
        public async Task GetUsageAsync_ReturnsUsageInfo()
        {
            // Arrange
            var expectedJson = JsonConvert.SerializeObject(new UsageInfo
            {
                Tier = "starter",
                Usage = new UsageStats { Compressions = 10 },
                Limits = new UsageLimits { CompressionLimit = 100 }
            });

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedJson)
                });

            // Act
            var usage = await _client.GetUsageAsync();

            // Assert
            Assert.Equal("starter", usage.Tier);
            Assert.Equal(10, usage.Usage.Compressions);
        }

        [Fact]
        public async Task API_Error_ThrowsMicroJpegApiException()
        {
            // Arrange
            var errorJson = JsonConvert.SerializeObject(new 
            { 
                error = "unauthorized", 
                message = "Invalid API key" 
            });

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = new StringContent(errorJson)
                });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<MicroJpegApiException>(() => _client.GetUsageAsync());
            Assert.Equal(401, ex.StatusCode);
            Assert.Equal("unauthorized", ex.ErrorCode);
            Assert.True(ex.IsUnauthorized);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
