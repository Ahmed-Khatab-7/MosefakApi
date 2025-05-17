namespace MosefakApp.Infrastructure.Repositories.Non_Generic
{
    public class AppointmentRepositoryAsync : GenericRepositoryAsync<Appointment>, IAppointmentRepositoryAsync
    {
        private readonly AppDbContext _context;
        public AppointmentRepositoryAsync(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AppointmentResponse>> GetAppointments(Expression<Func<Appointment, bool>> expression, int pageNumber = 1, int pageSize = 10)
        {
            // Ensure pageNumber is at least 1 to avoid negative skips
            pageNumber = pageNumber < 1 ? 1 : pageNumber;

            var appointments = await _context.Appointments
                                                   .Include(x => x.AppointmentType)
                                                   .Include(x => x.Doctor)
                                                   .ThenInclude(x => x.Specializations)
                                                   .Where(expression)
                                                   .Select(x =>
                                                                new AppointmentResponse
                                                                {
                                                                    Id = x.Id.ToString(),
                                                                    DoctorId = x.Doctor.AppUserId.ToString(),
                                                                    StartDate = x.StartDate,
                                                                    EndDate = x.EndDate,
                                                                    AppointmentType = new AppointmentTypeResponse
                                                                    {
                                                                        Id = x.AppointmentType.Id.ToString(),
                                                                        ConsultationFee = x.AppointmentType.ConsultationFee,
                                                                        Duration = x.AppointmentType.Duration,
                                                                        VisitType = x.AppointmentType.VisitType
                                                                    },
                                                                    DoctorSpecialization = x.Doctor.Specializations.Select(s => new SpecializationResponse
                                                                    {
                                                                        Id = s.Id.ToString(),
                                                                        Name = s.Name,
                                                                        Category = s.Category,
                                                                    })
                                                       .ToList()
                                                                })
                                                   .Skip((pageNumber - 1) * pageSize)
                                                   .Take(pageSize)
                                                   .ToListAsync();

            return appointments;
        }

        public async Task<bool> IsTimeSlotAvailable(int doctorId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var overlappingAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                            a.AppointmentStatus != AppointmentStatus.CanceledByDoctor &&
                            a.AppointmentStatus != AppointmentStatus.CanceledByPatient &&
                            a.StartDate < endDate &&
                            a.EndDate > startDate)
                .ToListAsync();

            return !overlappingAppointments.Any();
        }
    }
}
