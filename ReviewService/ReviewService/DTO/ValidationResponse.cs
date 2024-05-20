namespace ReviewService.DTO
{
    public class ValidationResponse
    {
        public Guid MovieId { get; set; }
        public bool Exists { get; set; }
    }
}
