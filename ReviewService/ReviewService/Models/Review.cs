namespace ReviewService.Models
{
    public class Review
    {
        public Guid ReviewId { get; set; }
        public Guid UserId { get; set; }
        public Guid MovieId { get; set; }
        public string Text { get; set; }
        public int Score { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsVisible { get; set; }
        public Guid ReviewedById { get; set; }
    }
}
