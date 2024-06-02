using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReviewService.Data;
using ReviewService.Models;
using ReviewService.Services;
using Xunit;

namespace ReviewServiceTest
{
    public class ReviewServiceTests
    {
        private readonly ReviewComService _service;
        private readonly DataContext _context;

        public ReviewServiceTests()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DataContext(options);
            _service = new ReviewComService(_context);
        }

        [Fact]
        public async Task GetAllReviewsAsync_ShouldReturnAllReviews()
        {
            // Arrange
            var reviews = new List<Review>
            {
                new Review { ReviewId = Guid.NewGuid(), UserId = "user1", MovieId = Guid.NewGuid(), Text = "Review 1", Score = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsVisible = true, ReviewedById = Guid.NewGuid() },
                new Review { ReviewId = Guid.NewGuid(), UserId = "user2", MovieId = Guid.NewGuid(), Text = "Review 2", Score = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsVisible = true, ReviewedById = Guid.NewGuid() }
            };

            await _context.Reviews.AddRangeAsync(reviews);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllReviewsAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetReviewByIdAsync_ShouldReturnReview()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var review = new Review { ReviewId = reviewId, UserId = "user2", MovieId = Guid.NewGuid(), Text = "Test Review", Score = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsVisible = true, ReviewedById = Guid.NewGuid() };


            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetReviewByIdAsync(reviewId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(reviewId, result.ReviewId);
            Assert.Equal("Test Review", result.Text);
        }

        [Fact]
        public async Task AddReviewAsync_ShouldAddReview()
        {
            // Arrange
            var review = new Review { UserId = "user2", MovieId = Guid.NewGuid(), Text = "New Review", Score = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsVisible = true, ReviewedById = Guid.NewGuid() };


            // Act
            var result = await _service.AddReviewAsync(review);

            // Assert
            var addedReview = await _context.Reviews.FindAsync(result.ReviewId);
            Assert.NotNull(addedReview);
            Assert.Equal("New Review", addedReview.Text);
        }

        [Fact]
        public async Task UpdateReviewAsync_ShouldUpdateReview()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var review = new Review { ReviewId = reviewId, UserId = "user2", MovieId = Guid.NewGuid(), Text = "New Review", Score = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsVisible = true, ReviewedById = Guid.NewGuid() };

            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();

            review.Text = "Updated Review";

            // Act
            await _service.UpdateReviewAsync(review);

            // Assert
            var updatedReview = await _context.Reviews.FindAsync(reviewId);
            Assert.Equal("Updated Review", updatedReview.Text);
        }

        [Fact]
        public async Task DeleteReviewAsync_ShouldDeleteReview()
        {
            // Arrange
            var reviewId = Guid.NewGuid();
            var review = new Review { ReviewId = reviewId, UserId = "user1", MovieId = Guid.NewGuid(), Text = "Review 1", Score = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsVisible = true, ReviewedById = Guid.NewGuid() };


            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();

            // Act
            await _service.DeleteReviewAsync(reviewId);

            // Assert
            var deletedReview = await _context.Reviews.FindAsync(reviewId);
            Assert.Null(deletedReview);
        }

        [Fact]
        public async Task GetReviewsByMovieIdAsync_ShouldReturnReviews()
        {
            // Arrange
            var movieId = Guid.NewGuid();
            var reviews = new List<Review>
            {
                 new Review { ReviewId = Guid.NewGuid(), UserId = "user1", MovieId = movieId, Text = "Review 1", Score = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsVisible = true, ReviewedById = Guid.NewGuid() },
                new Review { ReviewId = Guid.NewGuid(), UserId = "user2", MovieId = movieId, Text = "Review 2", Score = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsVisible = true, ReviewedById = Guid.NewGuid() }
            };

            await _context.Reviews.AddRangeAsync(reviews);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetReviewsByMovieIdAsync(movieId);

            // Assert
            Assert.Equal(2, result.Count());
        }
    }
}
