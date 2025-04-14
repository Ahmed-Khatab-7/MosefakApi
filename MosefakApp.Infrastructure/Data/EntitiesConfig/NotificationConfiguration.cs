namespace MosefakApp.Infrastructure.Data.EntitiesConfig
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications").HasKey(x => x.Id);

            builder.Property(x => x.Message).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Title).HasMaxLength(256).IsRequired();
        }
    }
}
