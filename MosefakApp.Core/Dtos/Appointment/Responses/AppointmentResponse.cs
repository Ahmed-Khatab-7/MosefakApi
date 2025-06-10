using System.Text.Json.Serialization;

namespace MosefakApp.Core.Dtos.Appointment.Responses
{
    public class AppointmentResponse
    {
        public string Id { get; set; } = null!;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AppointmentStatus AppointmentStatus { get; set; }
        public string DoctorId { get; set; } = null!;
        public string DoctorFullName { get; set; } = null!;
        public string? DoctorImage { get; set; }
        public string PatientFullName { get; set; } = null!; // new
        public string? PatientImage { get; set; } // new
        public List<SpecializationResponse> DoctorSpecialization { get; set; } = null!;
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public AppointmentTypeResponse AppointmentType { get; set; } = null!;
    }
}
