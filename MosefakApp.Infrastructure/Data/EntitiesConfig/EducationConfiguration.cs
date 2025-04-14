namespace MosefakApp.Infrastructure.Data.EntitiesConfig
{
    public class EducationConfiguration : IEntityTypeConfiguration<Education>
    {
        public void Configure(EntityTypeBuilder<Education> builder)
        {
            builder.ToTable("Educations").HasKey(x => x.Id);

            builder.Property(x => x.Degree).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Major).HasMaxLength(256).IsRequired();
            builder.Property(x => x.UniversityName).HasMaxLength(256).IsRequired();
            builder.Property(x => x.UniversityLogoPath).HasMaxLength(256).IsRequired(false);
            builder.Property(x => x.Location).HasMaxLength(256).IsRequired();
            builder.Property(x => x.AdditionalNotes).HasMaxLength(512).IsRequired(false);
        }
    }
}
