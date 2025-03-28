﻿namespace MosefakApp.Core.IRepositories.Non_Generic
{
    public interface IAppointmentRepositoryAsync : IGenericRepositoryAsync<Appointment>
    {
        Task<IEnumerable<AppointmentResponse>> GetAppointments(Expression<Func<Appointment,bool>> expression, int pageNumber = 1, int pageSize = 10); // I had to add it for better performance
        Task<bool> IsTimeSlotAvailable(int doctorId, DateTimeOffset startDate, DateTimeOffset endDate);
    }
}
