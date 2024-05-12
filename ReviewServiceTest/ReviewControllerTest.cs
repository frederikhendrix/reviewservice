using Microsoft.AspNetCore.Mvc;
using Moq;
using ReviewService.Controllers;
using ReviewService.Data;
using ReviewService.Interfaces;
using ReviewService.Models;
using ReviewService.Services;

namespace ReviewServiceTest
{
    public class ReviewControllerTest
    {

        private readonly Mock<IReviewService> _mockReviewService;
        private readonly ReviewsController _controller;


        public ReviewControllerTest()
        {
            _mockReviewService = new Mock<IReviewService>();
            _controller = new ReviewsController(_mockReviewService.Object);
        }

        /// <summary>
        /// This function calls GetReviewByIdAsync with .ReturnsAsync((Review)null) so the function always returns nothing.
        /// This is to make sure the Assert.IsType always passes.
        /// </summary>
        /// <returns>True</returns>
        [Fact]
        public async Task GetReviewByIdAsync_Test_Function_That_Returns_No_Review()
        {
            //Arrange
            _mockReviewService.Setup(x => x.GetReviewByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Review)null);

            //Act
            var result = await _controller.GetReview(Guid.NewGuid());

            //Assert
            Assert.IsType<NotFoundResult>(result);

        }
    }
}