using MosefakApp.Core.Dtos.Payment;

namespace MosefakApp.Core.IServices
{
    public interface IAppointmentService
    {
        // 🔹 Patient Methods
        Task<(List<AppointmentResponse> appointmentResponses, int totalPages)> GetPatientAppointments(int patientId, AppointmentStatus? status = null, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<AppointmentResponse> GetAppointmentById(int appointmentId, CancellationToken cancellationToken = default);
        Task<(List<AppointmentResponse> appointmentResponses, int totalPages)> GetAppointmentsByDateRange(int patientId, DateTimeOffset startDate, DateTimeOffset endDate, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<bool> RescheduleAppointmentAsync(int appointmentId, DateTime selectedDate, TimeSlot newTimeSlot); 
        Task<bool> CancelAppointmentByPatient(int patientId,int appointmentId, string? cancelationReason, CancellationToken cancellationToken = default); 
        Task<bool> BookAppointment(BookAppointmentRequest request, int appUserId, CancellationToken cancellationToken = default); 
        Task<AppointmentStatus> GetAppointmentStatus(int appointmentId); // Used by: Patients, Doctors, Admins, Background Jobs (Hangfire)
        Task<string> CreatePaymentIntent(int appointmentId, CancellationToken cancellationToken = default);
        Task<bool> ConfirmAppointmentPayment(int appointmentId, CancellationToken cancellationToken = default); // if manaully by fron-end not webhooks

        // 🔹 Doctor Methods
        Task<(List<AppointmentResponse> appointmentResponses, int totalPages)> GetDoctorAppointments(int doctorId, AppointmentStatus? status = null, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<(List<AppointmentPatientResponse> appointmentResponses, int totalPages)> GetDoctorAppointmentsWithPatientData(int doctorId, AppointmentStatus? status = null,
           int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<(List<AppointmentResponse> appointmentResponses, int totalPages)> GetPendingAppointmentsForDoctor(int doctorId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default); 
        Task<bool> ApproveAppointmentByDoctor(int appointmentId);
        Task<bool> RejectAppointmentByDoctor(int appointmentId, string? rejectionReason);
        Task<bool> CancelAppointmentByDoctor(int appointmentId, string? cancelationReason, CancellationToken cancellationToken = default);
        Task<bool> MarkAppointmentAsCompleted(int appointmentId);
        Task<(List<AppointmentResponse> appointmentResponses, int totalPages)> GetAppointmentsByDateRangeForDoctor(int doctorId, DateTimeOffset startDate, DateTimeOffset endDate, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

        Task<PaginatedResponse<PaymentResponse>> GetPayments(int pageNumber = 1, int pageSize = 10);
        Task<bool> DeletePayment(int paymentId);

        // 🔹 Utility Methods
        Task AutoCancelUnpaidAppointments(); // Handled by Hangfire
    }
}
