using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReviewService;
using ReviewService.Bus;
using ReviewService.Data;
using ReviewService.Interfaces;
using ReviewService.Models;
using Xunit;
using Xunit.Abstractions;

namespace ReviewServiceTest
{
    public class ReviewServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly ITestOutputHelper _output;
        private readonly HttpClient _client;

        public ReviewServiceIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _output = output;
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                builder.ConfigureServices(services =>
                {
                    // Remove the app's DbContext registration
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DataContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add DbContext using an in-memory database for testing
                    services.AddDbContext<DataContext>(options =>
                    {
                        options.UseInMemoryDatabase("InMemoryDbForTesting");
                    });

                    // Ensure IMessageSender is mocked
                    var mockMessageSender = new Mock<IMessageSender>();
                    mockMessageSender.Setup(sender => sender.SendMessageAsync(It.IsAny<string>()))
                                     .Returns(Task.CompletedTask);

                    // Replace the real IMessageSender with the mock
                    services.AddSingleton(mockMessageSender.Object);
                });
            });

            _client = _factory.CreateClient();
        }

        private void SeedDatabase(DataContext context)
        {
            var reviews = new List<Review>
        {
            new Review { ReviewId = Guid.NewGuid(), UserId = "user1", MovieId = Guid.NewGuid(), Text = "Review 1", Score = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsVisible = true, ReviewedById = Guid.NewGuid() },
            new Review { ReviewId = Guid.NewGuid(), UserId = "user2", MovieId = Guid.NewGuid(), Text = "Review 2", Score = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsVisible = true, ReviewedById = Guid.NewGuid() }
        };

            context.Reviews.AddRange(reviews);
            context.SaveChanges();
        }

        public async Task InitializeAsync()
        {
            _output.WriteLine("Initializer is run");
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();
                SeedDatabase(context);
            }
        }


        /// <summary>
        /// Tests that the GetAllReviews endpoint returns all reviews.
        /// </summary>
        /// <returns>Should return true for all Asserts</returns>
        [Fact]
        public async Task GetAllReviews_ShouldReturnAllReviews()
        {
            // Act
            var response = await _client.GetAsync("/reviews");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<Review>>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.Equal(2, result.Count);
        }


        /// <summary>
        /// Tests that the GetReviewById endpoint returns the correct review.
        /// </summary>
        /// <returns>Should return true for all Asserts</returns>
        [Fact]
        public async Task GetReviewById_ShouldReturnReview()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var review = new Review { ReviewId = reviewId, UserId = "user2", MovieId = Guid.NewGuid(), Text = "Test Review", Score = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsVisible = true, ReviewedById = Guid.NewGuid() };

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                context.Reviews.Add(review);
                context.SaveChanges();
            }

            // Act
            var response = await _client.GetAsync($"/reviews/{reviewId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Review>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(result);
            Assert.Equal(reviewId, result.ReviewId);
            Assert.Equal("Test Review", result.Text);
        }

        /// <summary>
        /// Tests that the AddReview endpoint successfully adds a new review.
        /// </summary>
        /// <returns>Should return true for all Asserts</returns>
        [Fact]
        public async Task AddReview_ShouldAddReview()
        {
            // Arrange
            var review = new Review { UserId = "user2", MovieId = Guid.NewGuid(), Text = "New Review", Score = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsVisible = true, ReviewedById = Guid.NewGuid() };
            var content = new StringContent(JsonSerializer.Serialize(review), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/reviews", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Review>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var addedReview = await context.Reviews.FindAsync(result.ReviewId);

                Assert.NotNull(addedReview);
                Assert.Equal("New Review", addedReview.Text);
            }
        }

        /// <summary>
        /// Tests that the UpdateReview endpoint successfully updates an existing review.
        /// </summary>
        /// <returns>Should return true for all Asserts</returns>
        [Fact]
        public async Task UpdateReview_ShouldUpdateReview()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var review = new Review { ReviewId = reviewId, UserId = "user2", MovieId = Guid.NewGuid(), Text = "New Review", Score = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsVisible = true, ReviewedById = Guid.NewGuid() };

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                context.Reviews.Add(review);
                context.SaveChanges();
            }

            review.Text = "Updated Review";
            var content = new StringContent(JsonSerializer.Serialize(review), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/reviews/{reviewId}", content);

            // Assert
            response.EnsureSuccessStatusCode();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var updatedReview = await context.Reviews.FindAsync(reviewId);

                Assert.Equal("Updated Review", updatedReview.Text);
            }
        }

        /// <summary>
        /// Tests that the DeleteReview endpoint successfully deletes a review.
        /// </summary>
        /// <returns>Should return true for all Asserts</returns>
        [Fact]
        public async Task DeleteReview_ShouldDeleteReview()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var review = new Review { ReviewId = reviewId, UserId = "user1", MovieId = Guid.NewGuid(), Text = "Review 1", Score = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsVisible = true, ReviewedById = Guid.NewGuid() };

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                context.Reviews.Add(review);
                context.SaveChanges();
            }

            // Act
            var response = await _client.DeleteAsync($"/reviews/{reviewId}");

            // Assert
            response.EnsureSuccessStatusCode();

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var deletedReview = await context.Reviews.FindAsync(reviewId);

                Assert.Null(deletedReview);
            }
        }

        /// <summary>
        /// Tests that the GetReviewsByMovieId endpoint returns reviews for a specific movie.
        /// </summary>
        /// <returns>Should return true for all Asserts</returns>
        [Fact]
        public async Task GetReviewsByMovieId_ShouldReturnReviews()
        {
            // Arrange
            var movieId = Guid.NewGuid();
            var reviews = new List<Review>
            {
                new Review { ReviewId = Guid.NewGuid(), UserId = "user1", MovieId = movieId, Text = "Review 1", Score = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsVisible = true, ReviewedById = Guid.NewGuid() },
                new Review { ReviewId = Guid.NewGuid(), UserId = "user2", MovieId = movieId, Text = "Review 2", Score = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsVisible = true, ReviewedById = Guid.NewGuid() }
            };

            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                context.Reviews.AddRange(reviews);
                context.SaveChanges();
            }

            // Act
            var response = await _client.GetAsync($"/reviews/movie/{movieId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<Review>>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.Equal(2, result.Count);
        }

        public Task DisposeAsync() => Task.CompletedTask;

    }
}
