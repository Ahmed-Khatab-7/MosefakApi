namespace MosefakApp.Core.Dtos.Review.Responses
{
    public class ReviewResponse
    {
        public string Id { get; set; } = null!;
        public int Rate { get; set; }
        public string? Comment { get; set; }
        public string FullName { get; set; } = null!;
        public string ImagePath { get; set; } = null!;
        public string AppUserId { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
