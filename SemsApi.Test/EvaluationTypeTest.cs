using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using SemsApi.DTO.EvaluationType;

namespace SemsApi.Test
{
    public class EvaluationTypeTest : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public EvaluationTypeTest(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetEvaluationTypeId_ReturnsOkResult()
        {
            var response = await _client.GetAsync("api/evaluation/types/3");
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task CreateEvaluationType_ReturnsCreatedResult()
        {
            var response = await _client.PostAsJsonAsync("api/evaluation/types", new CreateEvaluationTypeDto { EvaluationTypeName = "Teacher" });
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task CreateEvaluationType_ReturnsBadRequestResult()
        {
            var response = await _client.PostAsJsonAsync("api/evaluation/types", new { });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
