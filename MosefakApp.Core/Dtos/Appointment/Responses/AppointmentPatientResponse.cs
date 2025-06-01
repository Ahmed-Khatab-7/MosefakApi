using System.Text.Json.Serialization;

namespace MosefakApp.Core.Dtos.Appointment.Responses
{
    public class AppointmentPatientResponse
    {
        public string Id { get; set; } = null!;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AppointmentStatus AppointmentStatus { get; set; }
        public string PatientId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string  Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? PatientImage { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public AppointmentTypeResponse AppointmentType { get; set; } = null!;
    }
}
