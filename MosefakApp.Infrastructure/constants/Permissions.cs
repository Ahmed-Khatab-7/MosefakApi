namespace MosefakApp.Infrastructure.constants
{
    public static class Permissions
    {
        public static string Type { get; } = "permissions";

        public static class Users
        {
            // "Permissions.Users.View" -> "U.V"
            public const string View = "U.V";
            // "Permissions.Users.ViewUserById" -> "U.VID"
            public const string ViewById = "U.VID";
            public const string Create = "U.C";
            public const string Edit = "U.E";
            public const string Delete = "U.D";
            public const string UnLock = "U.UL";
        }

        public static class Roles
        {
            public const string View = "R.V";
            public const string ViewById = "R.VID";
            public const string Create = "R.C";
            public const string Edit = "R.E";
            public const string Delete = "R.D";
            public const string AssignPermissionToRole = "R.APR";
        }

        public static class Doctors
        {
            public const string View = "D.V";
            public const string ViewById = "D.VID";
            public const string ViewProfile = "D.VP";
            public const string ViewTopTen = "D.VTT";
            public const string Search = "D.S";
            public const string SearchBySpeciality = "D.SBS";
            public const string ViewAvailableTimeSlots = "D.VATS";
            public const string ViewUpcomingAppointments = "D.VUA";
            public const string ViewPastAppointments = "D.VPA";
            public const string GetTotalAppointments = "D.GTA";
            public const string Create = "D.C";
            public const string UploadProfileImage = "D.UPI";
            public const string CompleteProfile = "D.CP";
            public const string Edit = "D.E";
            public const string EditProfile = "D.EP";
            public const string Delete = "D.D";
            public const string ViewReviews = "D.VR";
            public const string ViewAverageRating = "D.VAR";
            public const string ViewTotalPatientsServed = "D.VTPS";
            public const string ViewEarningsReport = "D.VER";
            public const string UpdateWorkingTimesAsync = "D.UWTA";
        }

        public static class AppointmentTypes
        {
            public const string View = "AT.V";
            public const string Add = "AT.A";
            public const string Edit = "AT.E";
            public const string Delete = "AT.D";
        }

        public static class Appointments
        {
            public const string ViewPatientAppointments = "AP.VPA";
            public const string ViewDoctorAppointments = "AP.VDA";
            public const string ViewPendingForDoctor = "AP.VPD";
            public const string ViewInRangeForDoctor = "AP.VIRD";
            public const string View = "AP.V";
            public const string ViewInRange = "AP.VI";
            public const string ViewStatus = "AP.VS";
            public const string CancelByDoctor = "AP.CBD";
            public const string CancelByPatient = "AP.CBP";
            public const string Approve = "AP.A";
            public const string Reject = "AP.R";
            public const string MarkAsCompleted = "AP.MAC";
            public const string Reschedule = "AP.RES";
            public const string Book = "AP.B";
            public const string CreatePaymentIntent = "AP.CPI";
            public const string ConfirmPayment = "AP.CP";
        }

        public static class Patients
        {
            public const string ViewProfile = "PT.VP";
            public const string EditProfile = "PT.EP";
            public const string UploadProfileImage = "PT.UPI";
        }

        public static class Specializations
        {
            public const string View = "SP.V";
            public const string Create = "SP.C";
            public const string Edit = "SP.E";
            public const string Remove = "SP.R";
        }

        public static class Experiences
        {
            public const string View = "EX.V";
            public const string Create = "EX.C";
            public const string Edit = "EX.E";
            public const string Remove = "EX.R";
        }

        public static class Awards
        {
            public const string View = "AW.V";
            public const string Create = "AW.C";
            public const string Edit = "AW.E";
            public const string Remove = "AW.R";
        }

        public static class Educations
        {
            public const string View = "ED.V";
            public const string Create = "ED.C";
            public const string Edit = "ED.E";
            public const string Remove = "ED.R";
        }

        public static class Clinics
        {
            public const string View = "CL.V";
            public const string Create = "CL.C";
            public const string Edit = "CL.E";
            public const string Remove = "CL.R";
        }

        public static class Chatbot
        {
            public const string Ask = "CB.A";
        }

        public static class Reviews
        {
            public const string View = "RV.V";
            public const string Create = "RV.C";
            public const string Edit = "RV.E";
            public const string Delete = "RV.D";
        }

        public static class Contacts
        {
            public const string View = "CT.V";
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
