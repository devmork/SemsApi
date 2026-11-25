using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace SemsApi.Test
{
    public class BasicTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public BasicTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetEvaluationTypeId_ReturnsOkResult()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/evaluation/types/2");

            response.EnsureSuccessStatusCode();
        }
    }
}
