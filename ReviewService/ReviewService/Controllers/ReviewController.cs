﻿using Microsoft.AspNetCore.Mvc;
using ReviewService.Bus;
using ReviewService.Interfaces;
using ReviewService.Models;

namespace ReviewService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly IMessageSender _messageSender;

        public ReviewsController(IReviewService reviewService, IMessageSender messageSender)
        {
            _reviewService = reviewService;
            _messageSender = messageSender;
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

        [HttpGet("movie/{movieId}")]
        public async Task<IActionResult> GetReviewsByMovieId(Guid movieId)
        {
            var reviews = await _reviewService.GetReviewsByMovieIdAsync(movieId);
            return Ok(reviews);
        }

        [HttpPost]
        public async Task<IActionResult> PostReview([FromBody] Review review)
        {
            var createdReview = await _reviewService.AddReviewAsync(review);
            var messageBody = $"New review create: {createdReview.ReviewId}";
            await _messageSender.SendMessageAsync(messageBody);
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

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetReviewsByUserId(string userId)
        {
            var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);
            return Ok(reviews);
        }
    }
}
