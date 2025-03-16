namespace MosefakApp.Infrastructure.Identity.Seeding
{
    public static class SeedIdentityData // this class need to optimize in distribue the permissions
    {
        public static async Task SeedRolesAndPermissionsAsync(RoleManager<IdentityRole> roleManager)
        {
            // Define roles
            var roles = new List<string> { "Admin", "Doctor", "Patient" };

            // Ensure roles exist
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Assign permissions to each role
            var rolePermissions = new Dictionary<string, List<string>>
        {
            {
                "Admin", new List<string>
                {
                    // User Management
                    Permissions.Users.View, Permissions.Users.ViewById, Permissions.Users.Create,
                    Permissions.Users.Edit, Permissions.Users.Delete, Permissions.Users.UnLock,

                    // Role Management
                    Permissions.Roles.View, Permissions.Roles.ViewById, Permissions.Roles.Create,
                    Permissions.Roles.Edit, Permissions.Roles.Delete, Permissions.Roles.AssignPermissionToRole,

                    // Doctor Management
                    Permissions.Doctors.View, Permissions.Doctors.ViewById, Permissions.Doctors.ViewProfile,
                    Permissions.Doctors.ViewTopTen, Permissions.Doctors.Search, Permissions.Doctors.ViewAvailableTimeSlots,
                    Permissions.Doctors.ViewUpcomingAppointments, Permissions.Doctors.ViewPastAppointments,
                    Permissions.Doctors.GetTotalAppointments, Permissions.Doctors.Create, Permissions.Doctors.UploadProfileImage,
                    Permissions.Doctors.CompleteProfile, Permissions.Doctors.Edit, Permissions.Doctors.EditProfile,
                    Permissions.Doctors.Delete, Permissions.Doctors.ViewReviews, Permissions.Doctors.ViewAverageRating,
                    Permissions.Doctors.ViewTotalPatientsServed, Permissions.Doctors.ViewEarningsReport,

                    // Appointments Management
                    Permissions.Appointments.ViewPatientAppointments, Permissions.Appointments.ViewDoctorAppointments,
                    Permissions.Appointments.ViewPendingForDoctor, Permissions.Appointments.ViewInRangeForDoctor,
                    Permissions.Appointments.View, Permissions.Appointments.ViewInRange, Permissions.Appointments.ViewStatus,
                    Permissions.Appointments.CancelByDoctor, Permissions.Appointments.CancelByPatient,
                    Permissions.Appointments.Approve, Permissions.Appointments.Reject, Permissions.Appointments.MarkAsCompleted,
                    Permissions.Appointments.Reschedule, Permissions.Appointments.Book, Permissions.Appointments.CreatePaymentIntent,
                    Permissions.Appointments.ConfirmPayment,

                    // Specializations
                    Permissions.Specializations.View, Permissions.Specializations.Create,
                    Permissions.Specializations.Edit, Permissions.Specializations.Remove,

                    // Experience
                    Permissions.Experiences.View, Permissions.Experiences.Create,
                    Permissions.Experiences.Edit, Permissions.Experiences.Remove,

                    // Awards
                    Permissions.Awards.View, Permissions.Awards.Create,
                    Permissions.Awards.Edit, Permissions.Awards.Remove,

                    // Educations
                    Permissions.Educations.View, Permissions.Educations.Create,
                    Permissions.Educations.Edit, Permissions.Educations.Remove,

                    // Clinics
                    Permissions.Clinics.View, Permissions.Clinics.Create,
                    Permissions.Clinics.Edit, Permissions.Clinics.Remove,

                    // Reviews
                    Permissions.Reviews.View, Permissions.Reviews.Delete,

                    // Search
                    Permissions.Doctors.SearchBySpeciality, Permissions.Doctors.Search,
                }
            },
            {
                "Doctor", new List<string>
                {
                    // Doctor Profile Management
                    Permissions.Doctors.ViewProfile, Permissions.Doctors.EditProfile,
                    Permissions.Doctors.UploadProfileImage, Permissions.Doctors.ViewReviews,
                    Permissions.Doctors.ViewAverageRating, Permissions.Doctors.ViewTotalPatientsServed,
                    Permissions.Doctors.ViewEarningsReport,

                    // Appointments Management
                    Permissions.Appointments.ViewDoctorAppointments, Permissions.Appointments.ViewPendingForDoctor,
                    Permissions.Appointments.ViewInRangeForDoctor, Permissions.Appointments.View,
                    Permissions.Appointments.ViewStatus, Permissions.Appointments.CancelByDoctor,
                    Permissions.Appointments.Approve, Permissions.Appointments.Reject,
                    Permissions.Appointments.MarkAsCompleted, Permissions.Appointments.Reschedule,

                    // Specializations Management
                    Permissions.Specializations.View, Permissions.Specializations.Create,
                    Permissions.Specializations.Edit, Permissions.Specializations.Remove,

                    // Experience Management
                    Permissions.Experiences.View, Permissions.Experiences.Create,
                    Permissions.Experiences.Edit, Permissions.Experiences.Remove,

                    // Awards Management
                    Permissions.Awards.View, Permissions.Awards.Create,
                    Permissions.Awards.Edit, Permissions.Awards.Remove,

                    // Education Management
                    Permissions.Educations.View, Permissions.Educations.Create,
                    Permissions.Educations.Edit, Permissions.Educations.Remove,

                    // Clinics Management
                    Permissions.Clinics.View, Permissions.Clinics.Create,
                    Permissions.Clinics.Edit, Permissions.Clinics.Remove,

                    // Appointment Types Management
                    Permissions.AppointmentTypes.View, Permissions.AppointmentTypes.Add,
                    Permissions.AppointmentTypes.Edit, Permissions.AppointmentTypes.Delete,

                    // Search
                    Permissions.Doctors.SearchBySpeciality, Permissions.Doctors.Search,
                }
            },
            {
                "Patient", new List<string>
                {
                    // Profile & Account
                    Permissions.Patients.ViewProfile, Permissions.Patients.EditProfile,
                    Permissions.Patients.UploadProfileImage,

                    // Search
                    Permissions.Doctors.SearchBySpeciality, Permissions.Doctors.Search,
                    
                    // Appointments Management
                    Permissions.Appointments.ViewPatientAppointments, Permissions.Appointments.View,
                    Permissions.Appointments.Book, Permissions.Appointments.CancelByPatient,
                    Permissions.Appointments.CreatePaymentIntent, Permissions.Appointments.ConfirmPayment,

                    // Reviews Management
                    Permissions.Reviews.View, Permissions.Reviews.Create, Permissions.Reviews.Edit, Permissions.Reviews.Delete
                }
            }
        };

            // Assign permissions to roles
            foreach (var rolePermission in rolePermissions)
            {
                var roleName = rolePermission.Key;
                var permissions = rolePermission.Value;

                var role = await roleManager.FindByNameAsync(roleName);
                if (role != null)
                {
                    foreach (var permission in permissions)
                    {
                        if (!(await roleManager.RoleExistsAsync(role.Name)))
                        {
                            await roleManager.CreateAsync(new IdentityRole(role.Name));
                        }

                        // Check if the role already has the claim
                        var existingClaims = await roleManager.GetClaimsAsync(role);
                        if (!existingClaims.Any(c => c.Type == Permissions.Type && c.Value == permission))
                        {
                            await roleManager.AddClaimAsync(role, new Claim(Permissions.Type, permission));
                        }
                    }
                }
            }
        }
    }

}
