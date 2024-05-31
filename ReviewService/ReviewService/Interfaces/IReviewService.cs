using Microsoft.AspNetCore.Mvc;
using ReviewService.Models;

namespace ReviewService.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<Review>> GetAllReviewsAsync();
        Task<Review> GetReviewByIdAsync(Guid id);
        Task<Review> AddReviewAsync(Review review);
        Task UpdateReviewAsync(Review review);
        Task DeleteReviewAsync(Guid id);
        Task<IEnumerable<Review>> GetReviewsByMovieIdAsync(Guid movieId);
    }
}
