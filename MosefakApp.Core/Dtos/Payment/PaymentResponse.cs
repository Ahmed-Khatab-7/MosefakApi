namespace MosefakApp.Core.Dtos.Payment
{
    public class PaymentResponse 
    {
        public string Id { get; set; } = null!;
        public string AppointmentId { get; set; } = null!;
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string FullName { get; set; } = null!;
        public string Image { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
