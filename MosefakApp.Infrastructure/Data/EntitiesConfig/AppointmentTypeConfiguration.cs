namespace MosefakApp.Infrastructure.Data.EntitiesConfig
{
    public class AppointmentTypeConfiguration : IEntityTypeConfiguration<AppointmentType>
    {
        public void Configure(EntityTypeBuilder<AppointmentType> builder)
        {
            builder.ToTable("AppointmentTypes").HasKey(x => x.Id);

            builder.Property(x => x.VisitType).HasMaxLength(256).IsRequired();
            builder.Property(x => x.ConsultationFee).HasColumnType("decimal").HasPrecision(10, 2);
        }
    }
}
