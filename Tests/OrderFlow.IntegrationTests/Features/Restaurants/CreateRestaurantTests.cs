using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Features.Resturant.CreateRestaurant;
using System.Net;
using System.Net.Http.Json;
namespace OrderFlow.IntegrationTests.Features.Restaurants
{
    public class CreateRestaurantTests : BaseIntegrationTest
    {
        public CreateRestaurantTests(IntegrationTestWebFactory factory) : base(factory)
        {
        }
        [Fact]
        public async Task CreateRestaurant_ShouldReturn201_WhenRequestIsValid()
        {
            // Arrange
            var command = new CreateRestaurantCommand(
                Name: "Integration Grill",
                Address: "Docker Street 10",
                Description: "Testing with real SQL",
                OwnerId: Guid.NewGuid()
            );
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            // Act
            var response = await Client.PostAsJsonAsync("/api/v1/Restaurants", command, cts.Token);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();

            var result = await response.Content.ReadFromJsonAsync<CreateRestaurantResponse>(cts.Token);
            result.Should().NotBeNull();
            result!.RestaurantId.Should().NotBeEmpty();
        }
        [Fact]
        public async Task Create_ShouldReturn400BadRequest_WhenDataIsInvalid()
        {
            // Arrange
            var command = new CreateRestaurantCommand(
                Name: "",
                Address: "Docker Street 10",
                Description: "Testing with real SQL",
                OwnerId: Guid.NewGuid()
            );
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));


            //act

            var response = await Client.PostAsJsonAsync("/api/v1/Restaurants", command, cts.Token);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(cts.Token);
            problemDetails!.Should().NotBeNull();
            problemDetails!.Title.Should().NotBeNullOrEmpty();
        }
        [Fact]
        public async Task CreateRestaurant_ShouldReturn401Or403_WhenUserIsNotAdmin()
        {
            // Arrange
            var command = new CreateRestaurantCommand(
                Name: "Integration Grill",
                Address: "Docker Street 10",
                Description: "Testing with real SQL",
                OwnerId: Guid.NewGuid()
            );

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var anonymousClient = Factory.CreateClient();

            // Act
            var response = await anonymousClient.PostAsJsonAsync("/api/v1/Restaurants", command, cts.Token);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(cts.Token);
            problemDetails!.Should().NotBeNull();
            problemDetails!.Title.Should().NotBeNullOrEmpty();
        }
    }
}
