namespace MosefakApp.API.Helpers
{
    public class AppointmentJob
    {
        private readonly IServiceProvider _services;

        public AppointmentJob(IServiceProvider services)
        {
            _services = services;
        }

        public void Run()
        {
            using (var scope = _services.CreateScope())
            {
                var appointmentService = scope.ServiceProvider.GetRequiredService<AppointmentService>();
                appointmentService.AutoCancelUnpaidAppointments();
            }
        }
    }
}
