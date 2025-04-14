namespace MosefakApp.Infrastructure.Data.EntitiesConfig
{
    public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
    {
        public void Configure(EntityTypeBuilder<Doctor> builder)
        {
            builder.ToTable("Doctors").HasKey(x => x.Id);

            builder.Property(x => x.AboutMe).HasMaxLength(512).IsRequired();
            builder.Property(x => x.LicenseNumber).HasMaxLength(256).IsRequired();
        }
    }
}
