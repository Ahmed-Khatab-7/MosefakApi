namespace MosefakApp.Infrastructure.Data.EntitiesConfig
{
    public class ContactUsConfiguration : IEntityTypeConfiguration<ContactUs>
    {
        public void Configure(EntityTypeBuilder<ContactUs> builder)
        {
            builder.ToTable("ContactUs").HasKey(x => x.Id);

            builder.Property(x => x.Message).HasMaxLength(256).IsRequired();
        }
    }
}
