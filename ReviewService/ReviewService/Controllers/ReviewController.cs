using Microsoft.AspNetCore.Mvc;
using ReviewService.Interfaces;
using ReviewService.Models;

namespace ReviewService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReviews()
        {
            return Ok(await _reviewService.GetAllReviewsAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReview(Guid id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null)
                return NotFound();

            return Ok(review);
        }

        [HttpPost]
        public async Task<IActionResult> PostReview([FromBody] Review review)
        {
            var createdReview = await _reviewService.AddReviewAsync(review);
            return CreatedAtAction(nameof(GetReview), new { id = createdReview.ReviewId }, createdReview);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(Guid id, [FromBody] Review review)
        {
            if (id != review.ReviewId)
                return BadRequest();

            await _reviewService.UpdateReviewAsync(review);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(Guid id)
        {
            await _reviewService.DeleteReviewAsync(id);
            return NoContent();
        }
    }
}
