namespace MosefakApp.Domains.Entities
{
    public class Payment : BaseEntity
    {
        public int AppointmentId { get; set; }
        public Guid TransactionId { get; set; } = Guid.NewGuid();
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public MosefakApp.Domains.Enums.PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string StripePaymentIntentId { get; set; } = null!; // For Stripe Payment Tracking
        public string ClientSecret { get; set; } = null!; // Safe to return to frontend
        public Appointment Appointment { get; set; } = null!;
    }
}
