using ECommerce.Produtos.Infrastructure.OutBox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Produtos.Infrastructure.Mappings
{
  public sealed class OutboxMessageMap : IEntityTypeConfiguration<OutboxMessage>
  {
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
      builder.ToTable("outbox_messages");

      builder.HasKey(x => x.Id);

      builder.Property(x => x.Id)
          .HasColumnName("id");

      builder.Property(x => x.Type)
          .HasColumnName("type")
          .HasMaxLength(300)
          .IsRequired();

      builder.Property(x => x.Payload)
          .HasColumnName("payload")
          .IsRequired();

      builder.Property(x => x.OccurredAt)
          .HasColumnName("occurred_at")
          .IsRequired();

      builder.Property(x => x.ProcessedAt)
          .HasColumnName("processed_at");

      builder.Property(x => x.Error)
          .HasColumnName("error");

      builder.HasIndex(x => x.ProcessedAt);
    }
  }
}
