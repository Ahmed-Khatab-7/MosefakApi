namespace MosefakApp.API.Controllers
{
    [Route("api/reviews")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly IIdProtectorService _idProtectorService;
        public ReviewsController(IReviewService reviewService, IIdProtectorService idProtectorService)
        {
            _reviewService = reviewService;
            _idProtectorService = idProtectorService;
        }

        /// <summary>
        /// Retrieves all reviews for a specific doctor.
        /// </summary>
        /// <param name="doctorId">Doctor ID</param>
        /// <returns>List of reviews</returns>
        [HttpGet("doctor/{doctorId}")]
        [RequiredPermission(Permissions.Reviews.View)]
        public async Task<ActionResult<List<ReviewResponse>>> GetDoctorReviews(string doctorId)
        {
            var unprotectedDoctorId = UnprotectId(doctorId);

            if (unprotectedDoctorId == null)
                return BadRequest("Invalid ID");

            var reviews = await _reviewService.GetAllReviews(unprotectedDoctorId.Value);
            if (reviews == null || !reviews.Any())
                return NotFound(new { message = "No reviews found for this doctor." });

            reviews.ForEach(r => r.Id = ProtectId(r.Id));

            return Ok(reviews);
        }

        /// <summary>
        /// Adds a new review for a doctor.
        /// </summary>
        /// <param name="doctorId">Doctor ID</param>
        /// <param name="request">Review data</param>
        [HttpPost("doctor/{doctorId}")]
        [RequiredPermission(Permissions.Reviews.Create)]
        public async Task<ActionResult<bool>> AddReview(string doctorId, [FromBody] ReviewRequest request, CancellationToken cancellationToken)
        {
            var unprotectedDoctorId = UnprotectId(doctorId);

            if (unprotectedDoctorId == null)
                return BadRequest("Invalid ID");

            int patientId = User.GetUserId(); // Assume you have an extension method to get the logged-in user's ID

            bool result = await _reviewService.AddReview(patientId, unprotectedDoctorId.Value, request, cancellationToken);
            if (!result) return BadRequest(new { message = "Failed to add review." });

            return CreatedAtAction(nameof(GetDoctorReviews), new { doctorId }, new { message = "Review added successfully." });
        }

        /// <summary>
        /// Updates an existing review.
        /// </summary>
        /// <param name="reviewId">Review ID</param>
        /// <param name="request">Updated review data</param>
        [HttpPut("{reviewId}")]
        [RequiredPermission(Permissions.Reviews.Edit)]
        public async Task<ActionResult<bool>> UpdateReview(string reviewId, [FromBody] ReviewRequest request, CancellationToken cancellationToken)
        {
            var unprotectedReviewId = UnprotectId(reviewId);

            if (unprotectedReviewId == null)
                return BadRequest("Invalid ID");

            bool result = await _reviewService.EditReview(unprotectedReviewId.Value, request, cancellationToken);
            if (!result) return NotFound(new { message = "Review not found or failed to update." });

            return Ok(new { message = "Review updated successfully." });
        }

        /// <summary>
        /// Deletes a review.
        /// </summary>
        /// <param name="reviewId">Review ID</param>
        [HttpDelete("{reviewId}")]
        [RequiredPermission(Permissions.Reviews.Delete)]
        public async Task<ActionResult<bool>> DeleteReview(string reviewId, CancellationToken cancellationToken)
        {
            var unprotectedReviewId = UnprotectId(reviewId);

            if (unprotectedReviewId == null)
                return BadRequest("Invalid ID");

            bool result = await _reviewService.RemoveReview(unprotectedReviewId.Value, cancellationToken);
            if (!result) return NotFound(new { message = "Review not found or failed to delete." });

            return Ok(new { message = "Review deleted successfully." });
        }

        // 🔥 Utility Methods for ID Protection
        private string ProtectId(string id) => _idProtectorService.Protect(int.Parse(id));
        private int? UnprotectId(string id) => _idProtectorService.UnProtect(id);
    }
}
