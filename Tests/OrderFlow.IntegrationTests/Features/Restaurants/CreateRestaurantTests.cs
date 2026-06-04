using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Features.Restaurant.CreateRestaurant;
using OrderFlow.IntegrationTests.Fixtures;
using System.Net;
using System.Net.Http.Json;
namespace OrderFlow.IntegrationTests.Features.Restaurants
{
    public class CreateRestaurantTests : BaseIntegrationTest
    {
        public CreateRestaurantTests(IntegrationTestWebFactory factory) : base(factory)
        {
            Client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        }

        [Fact]
        public async Task Create_ShouldReturn201_WhenRequestIsValid()
        {
            var ct = TestContext.Current.CancellationToken;

            // Arrange
            var command = new CreateRestaurantCommand(
                Name: "Integration Grill",
                Address: "Docker Street 10",
                Latitude: 40.7128,
                Longitude: -74.0060,
                Description: "Testing with real SQL",
                OwnerId: Guid.NewGuid()
            );

            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/Restaurants", command, ct);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();

            var result = await response.Content.ReadFromJsonAsync<CreateRestaurantResponse>(ct);
            result.Should().NotBeNull();
            result!.RestaurantId.Should().NotBeEmpty();
        }
        [Fact]
        public async Task Create_ShouldReturn400BadRequest_WhenDataIsInvalid()
        {
            var ct = TestContext.Current.CancellationToken;

            // Arrange
            var command = new CreateRestaurantCommand(
                Name: "",
                Address: "Docker Street 10",
                Latitude: 40.7128,
                Longitude: -74.0060,
                Description: "Testing with real SQL",
                OwnerId: Guid.NewGuid()
            );

            //act

            var response = await Client.PostAsJsonAsync("/api/v1/Restaurants", command, ct);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(ct);
            problemDetails!.Should().NotBeNull();
            problemDetails!.Title.Should().NotBeNullOrEmpty();
        }
        [Fact]
        public async Task Create_ShouldReturn401Or403_WhenUserIsNotAdmin()
        {
            var ct = TestContext.Current.CancellationToken;

            // Arrange
            var command = new CreateRestaurantCommand(
                Name: "Integration Grill",
                Address: "Docker Street 10",
                Latitude: 40.7128,
                Longitude: -74.0060,
                Description: "Testing with real SQL",
                OwnerId: Guid.NewGuid()
            );

            var anonymousClient = Factory.CreateClient();

            // Act
            var response = await anonymousClient.PostAsJsonAsync("/api/v1/Restaurants", command, ct);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
        }
    }
}
