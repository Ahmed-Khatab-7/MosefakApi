﻿namespace MosefakApp.Core.Dtos.Appointment.Responses
{
    public class AppointmentDto
    {
        public string Id { get; set; } = null!;
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public AppointmentStatus Status { get; set; }  // "Upcoming", "Completed", "Canceled"
        public string? ProblemDescription { get; set; }

        // 🔹 Patient Information
        public string PatientId { get; set; } = null!;
        public string PatientName { get; set; } = null!;
        public string? PatientPhone { get; set; } 

        // 🔹 Appointment Type
        public string VisitType { get; set; } = null!; // "Online", "In-Person", "Emergency"
        public decimal ConsultationFee { get; set; }
        public TimeSpan Duration { get; set; }

    }
}
