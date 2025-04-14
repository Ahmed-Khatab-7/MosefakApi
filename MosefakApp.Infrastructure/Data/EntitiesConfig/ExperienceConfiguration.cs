namespace MosefakApp.Infrastructure.Data.EntitiesConfig
{
    public class ExperienceConfiguration : IEntityTypeConfiguration<Experience>
    {
        public void Configure(EntityTypeBuilder<Experience> builder)
        {
            builder.ToTable("Experiences").HasKey(x => x.Id);

            builder.Property(x => x.HospitalLogo).HasMaxLength(512).IsRequired(false);
            builder.Property(x => x.Title).HasMaxLength(256).IsRequired();
            builder.Property(x => x.HospitalName).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Location).HasMaxLength(256).IsRequired();
            builder.Property(x => x.JobDescription).HasMaxLength(512).IsRequired(false);
        }
    }
}
