namespace MosefakApp.Infrastructure.Data.EntitiesConfig
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("Appointments").HasKey(x => x.Id);

            builder.Property(x => x.AppointmentStatus)
                .HasConversion(new EnumToStringConverter<AppointmentStatus>());

            builder.Property(x => x.PaymentStatus)
                .HasConversion(new EnumToStringConverter<PaymentStatus>());

            builder.HasIndex(x => new { x.DoctorId, x.StartDate, x.EndDate })
                   .IsUnique()
                   .HasFilter("[AppointmentStatus] != 'CanceledByDoctor' AND [AppointmentStatus] != 'CanceledByPatient'");

            builder.Property(x => x.ProblemDescription).HasMaxLength(256).IsRequired(false);
            builder.Property(x => x.CancellationReason).HasMaxLength(256).IsRequired(false);
        }
    }
}
