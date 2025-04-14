namespace MosefakApp.Infrastructure.Data.EntitiesConfig
{
    public class AwardConfiguration : IEntityTypeConfiguration<Award>
    {
        public void Configure(EntityTypeBuilder<Award> builder)
        {
            builder.ToTable("Awards").HasKey(x => x.Id);

            builder.Property(x => x.Title).HasMaxLength(512).IsRequired();
            builder.Property(x => x.Organization).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(512).IsRequired(false);
        }
    }
}
