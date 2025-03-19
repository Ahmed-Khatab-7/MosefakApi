namespace MosefakApp.Infrastructure.constants
{
    public static class Permissions
    {
        public static string Type { get; } = "permissions";

        public static class Users
        {
            public const string View = "Permissions.Users.View";
            public const string ViewById = "Permissions.Users.ViewUserById";
            public const string Create = "Permissions.Users.Create";
            public const string Edit = "Permissions.Users.Edit";
            public const string Delete = "Permissions.Users.Delete";
            public const string UnLock = "Permissions.Users.UnLock";
        }

        public static class Roles
        {
            public const string View = "Permissions.Roles.View";
            public const string ViewById = "Permissions.Roles.ViewRoleById";
            public const string Create = "Permissions.Roles.Create";
            public const string Edit = "Permissions.Roles.Edit";
            public const string Delete = "Permissions.Roles.Delete";
            public const string AssignPermissionToRole = "Permissions.Roles.AssignPermissionToRole";
        }

        public static class Doctors
        {
            public const string View = "Permissions.Doctors.View";
            public const string ViewById = "Permissions.Doctors.ViewDoctorById";
            public const string ViewProfile = "Permissions.Doctors.ViewDoctorProfile";
            public const string ViewTopTen = "Permissions.Doctors.ViewTopTenDoctors";
            public const string Search = "Permissions.Doctors.SearchDoctors";
            public const string SearchBySpeciality = "Permissions.Doctors.SearchDoctorsBySpeciality";
            public const string ViewAvailableTimeSlots = "Permissions.Doctors.ViewAvailableTimeSlots";
            public const string ViewUpcomingAppointments = "Permissions.Doctors.ViewUpcomingAppointmentsForDoctor";
            public const string ViewPastAppointments = "Permissions.Doctors.ViewPastAppointmentsForDoctor";
            public const string GetTotalAppointments = "Permissions.Doctors.GetTotalAppointmentsAsync";
            public const string Create = "Permissions.Doctors.Create";
            public const string UploadProfileImage = "Permissions.Doctors.UploadImage";
            public const string CompleteProfile = "Permissions.Doctors.CompleteDoctorProfile";
            public const string Edit = "Permissions.Doctors.Edit";
            public const string EditProfile = "Permissions.Doctors.EditDoctorProfile";
            public const string Delete = "Permissions.Doctors.Delete";
            public const string ViewReviews = "Permissions.Doctors.ViewDoctorReviews";
            public const string ViewAverageRating = "Permissions.Doctors.ViewAverageRating";
            public const string ViewTotalPatientsServed = "Permissions.Doctors.ViewTotalPatientsServed";
            public const string ViewEarningsReport = "Permissions.Doctors.ViewEarningsReport";
            public const string UpdateWorkingTimesAsync = "Permissions.Doctors.UpdateWorkingTimesAsync"; 
        }

        public static class AppointmentTypes
        {
            public const string View = "Permissions.Doctors.ViewAppointmentTypes";
            public const string Add = "Permissions.Doctors.AddAppointmentTypes";
            public const string Edit = "Permissions.Doctors.EditAppointmentTypes";
            public const string Delete = "Permissions.Doctors.DeleteAppointmentTypes";
        }

        public static class Appointments
        {
            public const string ViewPatientAppointments = "Permissions.Appointments.ViewPatientAppointments";
            public const string ViewDoctorAppointments = "Permissions.Appointments.ViewDoctorAppointments";
            public const string ViewPendingForDoctor = "Permissions.Appointments.ViewPendingAppointmentsForDoctor";
            public const string ViewInRangeForDoctor = "Permissions.Appointments.ViewAppointmentsForDoctorInRange";
            public const string View = "Permissions.Appointments.ViewAppointment";
            public const string ViewInRange = "Permissions.Appointments.ViewAppointmentsInRange";
            public const string ViewStatus = "Permissions.Appointments.ViewAppointmentStatus";
            public const string CancelByDoctor = "Permissions.Appointments.CancelAppointmentByDoctor";
            public const string CancelByPatient = "Permissions.Appointments.CancelAppointmentByPatient";
            public const string Approve = "Permissions.Appointments.ApproveAppointment";
            public const string Reject = "Permissions.Appointments.RejectAppointment";
            public const string MarkAsCompleted = "Permissions.Appointments.MarkAppointmentAsCompleted";
            public const string Reschedule = "Permissions.Appointments.RescheduleAppointment";
            public const string Book = "Permissions.Appointments.BookAppointment";
            public const string CreatePaymentIntent = "Permissions.Appointments.CreatePaymentIntent";
            public const string ConfirmPayment = "Permissions.Appointments.ConfirmAppointmentPayment";
        }

        public static class Patients
        {
            public const string ViewProfile = "Permissions.Patients.ViewProfile";
            public const string EditProfile = "Permissions.Patients.EditPatientProfile";
            public const string UploadProfileImage = "Permissions.Patients.UploadPatientProfileImage";
        }

        public static class Specializations
        {
            public const string View = "Permissions.Specializations.ViewSpecializations";
            public const string Create = "Permissions.Specializations.CreateSpecialization";
            public const string Edit = "Permissions.Specializations.EditSpecialization";
            public const string Remove = "Permissions.Specializations.RemoveSpecialization";
        }

        public static class Experiences
        {
            public const string View = "Permissions.Experiences.ViewExperiences";
            public const string Create = "Permissions.Experiences.CreateExperience";
            public const string Edit = "Permissions.Experiences.EditExperience";
            public const string Remove = "Permissions.Experiences.RemoveExperience";
        }

        public static class Awards
        {
            public const string View = "Permissions.Awards.ViewAwards";
            public const string Create = "Permissions.Awards.CreateAward";
            public const string Edit = "Permissions.Awards.EditAward";
            public const string Remove = "Permissions.Awards.RemoveAward";
        }

        public static class Educations
        {
            public const string View = "Permissions.Educations.ViewEducations";
            public const string Create = "Permissions.Educations.CreateEducation";
            public const string Edit = "Permissions.Educations.EditEducation";
            public const string Remove = "Permissions.Educations.RemoveEducation";
        }

        public static class Clinics
        {
            public const string View = "Permissions.Clinics.ViewClinics";
            public const string Create = "Permissions.Clinics.CreateClinic";
            public const string Edit = "Permissions.Clinics.EditClinic";
            public const string Remove = "Permissions.Clinics.RemoveClinic";
        }

        public static class Chatbot
        {
            public const string Ask = "Permissions.Chatbot.Ask";
        }

        public static class Reviews
        {
            public const string View = "Permissions.Reviews.View"; 
            public const string Create = "Permissions.Reviews.Create"; 
            public const string Edit = "Permissions.Reviews.Edit";
            public const string Delete = "Permissions.Reviews.Delete"; 
        }

        public static class Contacts
        {
            public const string View = "Permissions.Contacts.View";
        }
        /// <summary>
        /// Retrieves a list of all permission constants.
        /// </summary>
        public static IList<string> GetPermissions()
        {
            return typeof(Permissions)
                .GetNestedTypes()
                .SelectMany(t => t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                    .Select(f => f.GetValue(null) as string))
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList()!;
        }
    }

  
}
