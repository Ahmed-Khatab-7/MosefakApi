namespace MosefakApp.Core.IServices
{
    public interface IReviewService
    {
        Task<List<ReviewResponse>> GetAllReviews(int doctorId);
        Task<List<ReviewResponse>> GetAllReviewsForDoctorDashboard(int doctorId); // from claims
        Task<bool> AddReview(int patientId, int doctorId, ReviewRequest request, CancellationToken cancellationToken = default);
        Task<bool> EditReview(int reviewId, ReviewRequest request, CancellationToken cancellationToken = default);
        Task<bool> RemoveReview(int reviewId, CancellationToken cancellationToken = default);
    }
}
