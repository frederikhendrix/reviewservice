using Microsoft.AspNetCore.Mvc;
using Moq;
using ReviewService.Bus;
using ReviewService.Controllers;
using ReviewService.Interfaces;
using ReviewService.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ReviewServiceTest
{
    public class ReviewControllerTest
    {
        private readonly Mock<IReviewService> _mockReviewService;
        private readonly Mock<MessageSender> _mockMessageSender;
        private readonly ReviewsController _controller;

        public ReviewControllerTest()
        {
            _mockReviewService = new Mock<IReviewService>();

            // Provide mock values for the constructor parameters
            var mockServiceBusConnectionString = "Endpoint=sb://mock.servicebus.windows.net/;SharedAccessKeyName=MockKey;SharedAccessKey=MockAccessKey";
            var mockTopicName = "mock-topic";

            _mockMessageSender = new Mock<MessageSender>(mockServiceBusConnectionString, mockTopicName);
            _controller = new ReviewsController(_mockReviewService.Object, _mockMessageSender.Object);
        }

        [Fact]
        public async Task GetReviewByIdAsync_Test_Function_That_Returns_No_Review()
        {
            // Arrange 
            _mockReviewService.Setup(x => x.GetReviewByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Review)null);

            // Act
            var result = await _controller.GetReview(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
