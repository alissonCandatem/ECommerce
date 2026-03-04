using ECommerce.IA.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.IA.Api.Data
{
  public sealed class IADbContext : DbContext
  {
    public IADbContext(DbContextOptions<IADbContext> options) : base(options) { }

    public DbSet<SchemaEmbedding> SchemaEmbeddings => Set<SchemaEmbedding>();
    public DbSet<QueryCache> QueryCache => Set<QueryCache>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.HasPostgresExtension("vector");

      modelBuilder.Entity<SchemaEmbedding>(builder =>
      {
        builder.ToTable("schema_embeddings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Tabela).HasColumnName("tabela").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Banco).HasColumnName("banco").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Descricao).HasColumnName("descricao").IsRequired();
        builder.Property(x => x.CriadoEm).HasColumnName("criado_em").IsRequired();
        builder.Property(x => x.Embedding)
            .HasColumnName("embedding")
            .HasColumnType("vector(768)")
            .IsRequired();
      });

      modelBuilder.Entity<QueryCache>(builder =>
      {
        builder.ToTable("query_cache");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Pergunta).HasColumnName("pergunta").IsRequired();
        builder.Property(x => x.SqlGerado).HasColumnName("sql_gerado").IsRequired();
        builder.Property(x => x.Sucesso).HasColumnName("sucesso");
        builder.Property(x => x.CriadoEm).HasColumnName("criado_em");
        builder.Property(x => x.Embedding)
            .HasColumnName("embedding")
            .HasColumnType("vector(768)")
            .IsRequired();
      });
    }
  }
}
